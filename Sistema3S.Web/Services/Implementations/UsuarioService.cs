using System.Data;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Usuario;
using Sistema3S.Web.Services.Interfaces;
using Sistema3S.Web.Services.Seguridad;

namespace Sistema3S.Web.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly Bd3sContext _context;
        private readonly PasswordHashService _passwordHashService;

        public UsuarioService(
            Bd3sContext context,
            PasswordHashService passwordHashService
        )
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        public async Task<List<UsuarioListadoDto>> ListarAsync(
            string? buscar,
            int? idRol,
            bool? estado
        )
        {
            var usuarios = new List<UsuarioListadoDto>();

            buscar = string.IsNullOrWhiteSpace(buscar)
                ? null
                : buscar.Trim();

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    u.IdUsuario,
                    u.IdRol,
                    r.Nombre AS Rol,
                    u.Correo,
                    u.Estado,
                    u.FechaRegistro
                FROM dbo.Usuario u
                INNER JOIN dbo.Rol r
                    ON r.IdRol = u.IdRol
                WHERE
                    (@Buscar IS NULL
                        OR u.Correo LIKE '%' + @Buscar + '%'
                        OR r.Nombre LIKE '%' + @Buscar + '%')
                    AND (@IdRol IS NULL OR u.IdRol = @IdRol)
                    AND (@Estado IS NULL OR u.Estado = @Estado)
                ORDER BY
                    CASE WHEN r.Nombre = 'Administrador' THEN 0 ELSE 1 END,
                    u.Estado DESC,
                    u.FechaRegistro DESC,
                    u.Correo;
            ";

            AgregarParametro(command, "@Buscar", buscar);
            AgregarParametro(command, "@IdRol", idRol.HasValue && idRol.Value > 0 ? idRol.Value : null);
            AgregarParametro(command, "@Estado", estado);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                usuarios.Add(new UsuarioListadoDto
                {
                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                    IdRol = Convert.ToInt32(reader["IdRol"]),
                    Rol = Convert.ToString(reader["Rol"]) ?? string.Empty,
                    Correo = Convert.ToString(reader["Correo"]) ?? string.Empty,
                    Estado = Convert.ToBoolean(reader["Estado"]),
                    FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
                });
            }

            return usuarios;
        }

        public async Task<UsuarioOperacionResultadoDto> CrearAsync(UsuarioCrearDto dto)
        {
            var correo = NormalizarCorreo(dto.Correo);
            var contrasena = dto.ObtenerContrasenaInicial();

            if (dto.IdRol <= 0)
            {
                throw new InvalidOperationException("Selecciona un rol válido.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new InvalidOperationException("Ingresa el correo del usuario.");
            }

            if (!correo.Contains("@") || !correo.Contains("."))
            {
                throw new InvalidOperationException("Ingresa un correo válido.");
            }

            ValidarContrasena(contrasena);

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await ValidarRolActivoAsync(connection, transaction, dto.IdRol);

                await ValidarCorreoDisponibleAsync(
                    connection,
                    transaction,
                    correo,
                    null
                );

                var hash = _passwordHashService.CrearHash(contrasena);
                int idUsuario;

                await using (var insertar = connection.CreateCommand())
                {
                    insertar.Transaction = transaction;

                    insertar.CommandText = @"
                        INSERT INTO dbo.Usuario (
                            IdRol,
                            Correo,
                            ContrasenaHash,
                            Estado,
                            FechaRegistro
                        )
                        VALUES (
                            @IdRol,
                            @Correo,
                            @ContrasenaHash,
                            1,
                            GETDATE()
                        );

                        SELECT CAST(SCOPE_IDENTITY() AS INT);
                    ";

                    AgregarParametro(insertar, "@IdRol", dto.IdRol);
                    AgregarParametro(insertar, "@Correo", correo);
                    AgregarParametro(insertar, "@ContrasenaHash", hash);

                    idUsuario = Convert.ToInt32(await insertar.ExecuteScalarAsync());
                }

                await transaction.CommitAsync();

                return new UsuarioOperacionResultadoDto
                {
                    IdUsuario = idUsuario,
                    Mensaje = "Usuario registrado correctamente."
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UsuarioOperacionResultadoDto> ActualizarAsync(
            int idUsuario,
            UsuarioActualizarDto dto
        )
        {
            var correo = NormalizarCorreo(dto.Correo);
            var nuevaContrasena = string.IsNullOrWhiteSpace(dto.ObtenerNuevaContrasena())
                ? null
                : dto.ObtenerNuevaContrasena();

            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("Selecciona un usuario válido.");
            }

            if (dto.IdRol <= 0)
            {
                throw new InvalidOperationException("Selecciona un rol válido.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new InvalidOperationException("Ingresa el correo del usuario.");
            }

            if (!correo.Contains("@") || !correo.Contains("."))
            {
                throw new InvalidOperationException("Ingresa un correo válido.");
            }

            if (!string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                ValidarContrasena(nuevaContrasena);
            }

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var usuarioActual = await ObtenerUsuarioBasicoAsync(
                    connection,
                    transaction,
                    idUsuario
                );

                if (usuarioActual == null)
                {
                    throw new InvalidOperationException("No se encontró el usuario seleccionado.");
                }

                var esAdminPrincipal = usuarioActual.Value.IdUsuario == 1;

                if (esAdminPrincipal)
                {
                    dto.Estado = true;

                    var rolAdministrador = await ObtenerIdRolAdministradorAsync(
                        connection,
                        transaction
                    );

                    if (rolAdministrador.HasValue)
                    {
                        dto.IdRol = rolAdministrador.Value;
                    }
                }

                await ValidarRolActivoAsync(connection, transaction, dto.IdRol);

                await ValidarCorreoDisponibleAsync(
                    connection,
                    transaction,
                    correo,
                    idUsuario
                );

                var hash = string.IsNullOrWhiteSpace(nuevaContrasena)
                    ? null
                    : _passwordHashService.CrearHash(nuevaContrasena);

                await using (var actualizar = connection.CreateCommand())
                {
                    actualizar.Transaction = transaction;

                    actualizar.CommandText = @"
                        UPDATE dbo.Usuario
                        SET
                            IdRol = @IdRol,
                            Correo = @Correo,
                            Estado = @Estado
                    ";

                    if (!string.IsNullOrWhiteSpace(hash))
                    {
                        actualizar.CommandText += @",
                            ContrasenaHash = @ContrasenaHash
                        ";
                    }

                    actualizar.CommandText += @"
                        WHERE IdUsuario = @IdUsuario;
                    ";

                    AgregarParametro(actualizar, "@IdRol", dto.IdRol);
                    AgregarParametro(actualizar, "@Correo", correo);
                    AgregarParametro(actualizar, "@Estado", dto.Estado);
                    AgregarParametro(actualizar, "@IdUsuario", idUsuario);

                    if (!string.IsNullOrWhiteSpace(hash))
                    {
                        AgregarParametro(actualizar, "@ContrasenaHash", hash);
                    }

                    await actualizar.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return new UsuarioOperacionResultadoDto
                {
                    IdUsuario = idUsuario,
                    Mensaje = "Usuario actualizado correctamente."
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UsuarioOperacionResultadoDto> CambiarContrasenaAsync(
            int idUsuario,
            UsuarioCambiarContrasenaDto dto
        )
        {
            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("Selecciona un usuario válido.");
            }

            var nuevaContrasena = dto.ObtenerContrasena();

            ValidarContrasena(nuevaContrasena);

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var hash = _passwordHashService.CrearHash(nuevaContrasena);

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE dbo.Usuario
                SET ContrasenaHash = @ContrasenaHash
                WHERE IdUsuario = @IdUsuario;
            ";

            AgregarParametro(command, "@ContrasenaHash", hash);
            AgregarParametro(command, "@IdUsuario", idUsuario);

            var filas = await command.ExecuteNonQueryAsync();

            if (filas <= 0)
            {
                throw new InvalidOperationException("No se encontró el usuario seleccionado.");
            }

            return new UsuarioOperacionResultadoDto
            {
                IdUsuario = idUsuario,
                Mensaje = "Contraseña actualizada correctamente."
            };
        }

        public async Task<UsuarioOperacionResultadoDto> DesactivarAsync(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("Selecciona un usuario válido.");
            }

            if (idUsuario == 1)
            {
                throw new InvalidOperationException("No se puede desactivar el usuario administrador principal.");
            }

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE dbo.Usuario
                SET Estado = 0
                WHERE IdUsuario = @IdUsuario;
            ";

            AgregarParametro(command, "@IdUsuario", idUsuario);

            var filas = await command.ExecuteNonQueryAsync();

            if (filas <= 0)
            {
                throw new InvalidOperationException("No se encontró el usuario seleccionado.");
            }

            return new UsuarioOperacionResultadoDto
            {
                IdUsuario = idUsuario,
                Mensaje = "Usuario desactivado correctamente."
            };
        }

        public async Task<UsuarioOperacionResultadoDto> ActivarAsync(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new InvalidOperationException("Selecciona un usuario válido.");
            }

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE dbo.Usuario
                SET Estado = 1
                WHERE IdUsuario = @IdUsuario;
            ";

            AgregarParametro(command, "@IdUsuario", idUsuario);

            var filas = await command.ExecuteNonQueryAsync();

            if (filas <= 0)
            {
                throw new InvalidOperationException("No se encontró el usuario seleccionado.");
            }

            return new UsuarioOperacionResultadoDto
            {
                IdUsuario = idUsuario,
                Mensaje = "Usuario activado correctamente."
            };
        }

        private static string NormalizarCorreo(string? correo)
        {
            return (correo ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static void ValidarContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new InvalidOperationException("Ingresa la contraseña.");
            }

            if (contrasena.Length < 8)
            {
                throw new InvalidOperationException("La contraseña debe tener mínimo 8 caracteres.");
            }

            if (!contrasena.Any(char.IsUpper))
            {
                throw new InvalidOperationException("La contraseña debe incluir al menos una mayúscula.");
            }

            if (!contrasena.Any(char.IsLower))
            {
                throw new InvalidOperationException("La contraseña debe incluir al menos una minúscula.");
            }

            if (!contrasena.Any(char.IsDigit))
            {
                throw new InvalidOperationException("La contraseña debe incluir al menos un número.");
            }

            if (!contrasena.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new InvalidOperationException("La contraseña debe incluir al menos un símbolo.");
            }
        }

        private static void AgregarParametro(
            IDbCommand command,
            string nombre,
            object? valor
        )
        {
            var parametro = command.CreateParameter();
            parametro.ParameterName = nombre;
            parametro.Value = valor ?? DBNull.Value;
            command.Parameters.Add(parametro);
        }

        private static async Task ValidarRolActivoAsync(
            System.Data.Common.DbConnection connection,
            System.Data.Common.DbTransaction transaction,
            int idRol
        )
        {
            await using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = @"
                SELECT COUNT(1)
                FROM dbo.Rol
                WHERE IdRol = @IdRol
                  AND Estado = 1;
            ";

            AgregarParametro(command, "@IdRol", idRol);

            var existe = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (existe <= 0)
            {
                throw new InvalidOperationException("El rol seleccionado no existe o está desactivado.");
            }
        }

        private static async Task ValidarCorreoDisponibleAsync(
            System.Data.Common.DbConnection connection,
            System.Data.Common.DbTransaction transaction,
            string correo,
            int? idUsuarioActual
        )
        {
            await using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = @"
                SELECT COUNT(1)
                FROM dbo.Usuario
                WHERE UPPER(LTRIM(RTRIM(Correo))) = UPPER(@Correo)
                  AND (@IdUsuarioActual IS NULL OR IdUsuario <> @IdUsuarioActual);
            ";

            AgregarParametro(command, "@Correo", correo);
            AgregarParametro(command, "@IdUsuarioActual", idUsuarioActual);

            var existe = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (existe > 0)
            {
                throw new InvalidOperationException("Ya existe un usuario registrado con ese correo.");
            }
        }

        private static async Task<(int IdUsuario, string Rol)?> ObtenerUsuarioBasicoAsync(
            System.Data.Common.DbConnection connection,
            System.Data.Common.DbTransaction transaction,
            int idUsuario
        )
        {
            await using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = @"
                SELECT
                    u.IdUsuario,
                    r.Nombre AS Rol
                FROM dbo.Usuario u
                INNER JOIN dbo.Rol r
                    ON r.IdRol = u.IdRol
                WHERE u.IdUsuario = @IdUsuario;
            ";

            AgregarParametro(command, "@IdUsuario", idUsuario);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return (
                Convert.ToInt32(reader["IdUsuario"]),
                Convert.ToString(reader["Rol"]) ?? string.Empty
            );
        }

        private static async Task<int?> ObtenerIdRolAdministradorAsync(
            System.Data.Common.DbConnection connection,
            System.Data.Common.DbTransaction transaction
        )
        {
            await using var command = connection.CreateCommand();

            command.Transaction = transaction;
            command.CommandText = @"
                SELECT TOP 1 IdRol
                FROM dbo.Rol
                WHERE Nombre = 'Administrador'
                  AND Estado = 1
                ORDER BY IdRol;
            ";

            var resultado = await command.ExecuteScalarAsync();

            if (resultado == null || resultado == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(resultado);
        }
    }
}