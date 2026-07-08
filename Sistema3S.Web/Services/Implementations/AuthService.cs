using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Auth;
using Sistema3S.Web.Services.Interfaces;
using Sistema3S.Web.Services.Seguridad;

namespace Sistema3S.Web.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly Bd3sContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHashService _passwordHashService;

        public AuthService(
            Bd3sContext context,
            IConfiguration configuration,
            PasswordHashService passwordHashService
        )
        {
            _context = context;
            _configuration = configuration;
            _passwordHashService = passwordHashService;
        }

        public async Task<LoginResultadoDto> LoginAsync(LoginDto dto)
        {
            var correo = NormalizarCorreo(dto.Correo);
            var contrasena = dto.Contrasena ?? string.Empty;

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                throw new InvalidOperationException("Credenciales incorrectas.");
            }

            UsuarioLoginData usuario;

            await using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                await using var command = new SqlCommand("sp_LoginUsuario", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Correo", correo);

                try
                {
                    await using var reader = await command.ExecuteReaderAsync();

                    if (!await reader.ReadAsync())
                    {
                        throw new InvalidOperationException("Credenciales incorrectas.");
                    }

                    usuario = new UsuarioLoginData
                    {
                        IdUsuario = LeerInt(reader, "IdUsuario"),
                        IdRol = LeerInt(reader, "IdRol"),
                        Rol = LeerString(reader, "Rol"),
                        Correo = LeerString(reader, "Correo"),
                        ContrasenaHash = LeerString(reader, "ContrasenaHash"),
                        EstadoUsuario = LeerBool(reader, "EstadoUsuario"),
                        EstadoRol = LeerBool(reader, "EstadoRol")
                    };
                }
                catch (SqlException)
                {
                    throw new InvalidOperationException("Credenciales incorrectas.");
                }
            }

            if (!usuario.EstadoUsuario || !usuario.EstadoRol)
            {
                throw new InvalidOperationException("Credenciales incorrectas.");
            }

            if (!_passwordHashService.EsHashValido(usuario.ContrasenaHash))
            {
                throw new InvalidOperationException("La contraseña del usuario aún no fue actualizada al formato seguro.");
            }

            var passwordCorrecto = _passwordHashService.Verificar(
                contrasena,
                usuario.ContrasenaHash
            );

            if (!passwordCorrecto)
            {
                throw new InvalidOperationException("Credenciales incorrectas.");
            }

            var permisos = await ObtenerPermisosUsuarioAsync(usuario.IdUsuario);

            if (permisos.Count == 0)
            {
                throw new InvalidOperationException("El usuario no tiene permisos asignados.");
            }

            var expira = DateTime.Now.AddMinutes(ObtenerMinutosExpiracion());
            var token = GenerarToken(usuario, permisos, expira);

            return new LoginResultadoDto
            {
                IdUsuario = usuario.IdUsuario,
                IdRol = usuario.IdRol,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Token = token,
                Expira = expira,
                Permisos = permisos.Select(p => p.Nombre).ToList(),
                PermisosDetalle = permisos
            };
        }

        public async Task<CambiarContrasenaResultadoDto> CambiarContrasenaInicialAsync(
            CambiarContrasenaInicialDto dto
        )
        {
            var claveConfigurada = _configuration["Jwt:Key"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(dto.ClaveSeguridad) ||
                dto.ClaveSeguridad != claveConfigurada)
            {
                throw new InvalidOperationException("No autorizado para cambiar la contraseña inicial.");
            }

            if (dto.IdUsuario <= 0)
            {
                throw new InvalidOperationException("El usuario no es válido.");
            }

            ValidarContrasenaSegura(dto.NuevaContrasena);

            var hash = _passwordHashService.CrearHash(dto.NuevaContrasena);

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_CambiarContrasenaUsuario", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdUsuario", dto.IdUsuario);
            command.Parameters.AddWithValue("@ContrasenaHash", hash);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("No se pudo cambiar la contraseña.");
                }

                return new CambiarContrasenaResultadoDto
                {
                    Mensaje = LeerString(reader, "Mensaje"),
                    IdUsuario = LeerInt(reader, "IdUsuario")
                };
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        private async Task<List<PermisoSesionDto>> ObtenerPermisosUsuarioAsync(int idUsuario)
        {
            var permisos = new List<PermisoSesionDto>();

            await using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand("sp_ObtenerPermisosUsuario", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IdUsuario", idUsuario);

            try
            {
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    permisos.Add(new PermisoSesionDto
                    {
                        IdPermiso = LeerInt(reader, "IdPermiso"),
                        Nombre = LeerString(reader, "Nombre"),
                        Descripcion = LeerNullableString(reader, "Descripcion"),
                        Asignado = LeerBool(reader, "Asignado")
                    });
                }

                return permisos;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ObtenerMensajeSql(ex));
            }
        }

        private string GenerarToken(
            UsuarioLoginData usuario,
            List<PermisoSesionDto> permisos,
            DateTime expira
        )
        {
            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            {
                throw new InvalidOperationException("La clave JWT no está configurada correctamente.");
            }

            var issuer = _configuration["Jwt:Issuer"] ?? "Sistema3S";
            var audience = _configuration["Jwt:Audience"] ?? "Sistema3SAdmin";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol),

                new Claim("idUsuario", usuario.IdUsuario.ToString()),
                new Claim("idRol", usuario.IdRol.ToString()),
                new Claim("rol", usuario.Rol),
                new Claim("correo", usuario.Correo)
            };

            foreach (var permiso in permisos)
            {
                claims.Add(new Claim("permiso", permiso.Nombre));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: expira,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int ObtenerMinutosExpiracion()
        {
            var valor = _configuration["Jwt:ExpireMinutes"];

            if (int.TryParse(valor, out var minutos) && minutos > 0)
            {
                return minutos;
            }

            return 480;
        }

        private static void ValidarContrasenaSegura(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new InvalidOperationException("Ingresa una contraseña.");
            }

            if (contrasena.Length < 8)
            {
                throw new InvalidOperationException("La contraseña debe tener al menos 8 caracteres.");
            }

            if (!contrasena.Any(char.IsUpper))
            {
                throw new InvalidOperationException("La contraseña debe tener al menos una mayúscula.");
            }

            if (!contrasena.Any(char.IsLower))
            {
                throw new InvalidOperationException("La contraseña debe tener al menos una minúscula.");
            }

            if (!contrasena.Any(char.IsDigit))
            {
                throw new InvalidOperationException("La contraseña debe tener al menos un número.");
            }

            if (!contrasena.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new InvalidOperationException("La contraseña debe tener al menos un carácter especial.");
            }
        }

        private static string NormalizarCorreo(string? correo)
        {
            return (correo ?? string.Empty).Trim().ToLower();
        }

        private static string ObtenerMensajeSql(SqlException ex)
        {
            if (ex.Errors.Count > 0)
            {
                return ex.Errors[0].Message;
            }

            return ex.Message;
        }

        private static bool ExisteColumna(SqlDataReader reader, string columna)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), columna, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static int LeerInt(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(reader[columna]);
        }

        private static string LeerString(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return string.Empty;
            }

            return reader[columna]?.ToString() ?? string.Empty;
        }

        private static string? LeerNullableString(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return null;
            }

            var valor = reader[columna]?.ToString();

            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        private static bool LeerBool(SqlDataReader reader, string columna)
        {
            if (!ExisteColumna(reader, columna) || reader[columna] == DBNull.Value)
            {
                return false;
            }

            return Convert.ToBoolean(reader[columna]);
        }

        private class UsuarioLoginData
        {
            public int IdUsuario { get; set; }

            public int IdRol { get; set; }

            public string Rol { get; set; } = string.Empty;

            public string Correo { get; set; } = string.Empty;

            public string ContrasenaHash { get; set; } = string.Empty;

            public bool EstadoUsuario { get; set; }

            public bool EstadoRol { get; set; }
        }
    }
}