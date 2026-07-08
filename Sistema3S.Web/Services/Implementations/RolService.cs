using System.Data;
using Microsoft.EntityFrameworkCore;
using Sistema3S.Web.Data;
using Sistema3S.Web.DTOs.Permiso;
using Sistema3S.Web.DTOs.Rol;
using Sistema3S.Web.Services.Interfaces;

namespace Sistema3S.Web.Services.Implementations
{
    public class RolService : IRolService
    {
        private readonly Bd3sContext _context;

        public RolService(Bd3sContext context)
        {
            _context = context;
        }

        public async Task<List<RolListadoDto>> ListarAsync(bool? soloActivos)
        {
            var roles = new List<RolListadoDto>();

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    r.IdRol,
                    r.Nombre,
                    r.Descripcion,
                    r.Estado,
                    COUNT(DISTINCT rp.IdPermiso) AS TotalPermisos,
                    COUNT(DISTINCT u.IdUsuario) AS TotalUsuarios
                FROM dbo.Rol r
                LEFT JOIN dbo.RolPermiso rp
                    ON rp.IdRol = r.IdRol
                LEFT JOIN dbo.Usuario u
                    ON u.IdRol = r.IdRol
                   AND u.Estado = 1
                WHERE
                    (@SoloActivos IS NULL OR r.Estado = @SoloActivos)
                GROUP BY
                    r.IdRol,
                    r.Nombre,
                    r.Descripcion,
                    r.Estado
                ORDER BY
                    CASE WHEN r.Nombre = 'Administrador' THEN 0 ELSE 1 END,
                    r.Nombre;
            ";

            var paramSoloActivos = command.CreateParameter();
            paramSoloActivos.ParameterName = "@SoloActivos";
            paramSoloActivos.Value = soloActivos.HasValue ? soloActivos.Value : DBNull.Value;
            command.Parameters.Add(paramSoloActivos);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                roles.Add(new RolListadoDto
                {
                    IdRol = Convert.ToInt32(reader["IdRol"]),
                    Nombre = Convert.ToString(reader["Nombre"]) ?? string.Empty,
                    Descripcion = reader["Descripcion"] == DBNull.Value
                        ? null
                        : Convert.ToString(reader["Descripcion"]),
                    Estado = Convert.ToBoolean(reader["Estado"]),
                    TotalPermisos = Convert.ToInt32(reader["TotalPermisos"]),
                    TotalUsuarios = Convert.ToInt32(reader["TotalUsuarios"])
                });
            }

            return roles;
        }

        public async Task<RolRespuestaListadoDto> CrearAsync(RolCrearDto dto)
        {
            var nombre = (dto.Nombre ?? string.Empty).Trim();
            var descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                ? null
                : dto.Descripcion.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new InvalidOperationException("Ingresa el nombre del rol.");
            }

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await using (var validar = connection.CreateCommand())
                {
                    validar.Transaction = transaction;
                    validar.CommandText = @"
                        SELECT COUNT(1)
                        FROM dbo.Rol
                        WHERE UPPER(LTRIM(RTRIM(Nombre))) = UPPER(@Nombre);
                    ";

                    var pNombre = validar.CreateParameter();
                    pNombre.ParameterName = "@Nombre";
                    pNombre.Value = nombre;
                    validar.Parameters.Add(pNombre);

                    var existe = Convert.ToInt32(await validar.ExecuteScalarAsync());

                    if (existe > 0)
                    {
                        throw new InvalidOperationException("Ya existe un rol con ese nombre.");
                    }
                }

                int idRol;

                await using (var insertar = connection.CreateCommand())
                {
                    insertar.Transaction = transaction;
                    insertar.CommandText = @"
                        INSERT INTO dbo.Rol (Nombre, Descripcion, Estado)
                        VALUES (@Nombre, @Descripcion, 1);

                        SELECT CAST(SCOPE_IDENTITY() AS INT);
                    ";

                    var pNombre = insertar.CreateParameter();
                    pNombre.ParameterName = "@Nombre";
                    pNombre.Value = nombre;
                    insertar.Parameters.Add(pNombre);

                    var pDescripcion = insertar.CreateParameter();
                    pDescripcion.ParameterName = "@Descripcion";
                    pDescripcion.Value = descripcion ?? (object)DBNull.Value;
                    insertar.Parameters.Add(pDescripcion);

                    idRol = Convert.ToInt32(await insertar.ExecuteScalarAsync());
                }

                await transaction.CommitAsync();

                return new RolRespuestaListadoDto
                {
                    Mensaje = "Rol creado correctamente.",
                    Roles = await ListarAsync(null)
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RolRespuestaListadoDto> ActualizarAsync(int idRol, RolActualizarDto dto)
        {
            var nombre = (dto.Nombre ?? string.Empty).Trim();
            var descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                ? null
                : dto.Descripcion.Trim();

            if (idRol <= 0)
            {
                throw new InvalidOperationException("Selecciona un rol válido.");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new InvalidOperationException("Ingresa el nombre del rol.");
            }

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string? nombreActual;

                await using (var obtener = connection.CreateCommand())
                {
                    obtener.Transaction = transaction;
                    obtener.CommandText = @"
                        SELECT Nombre
                        FROM dbo.Rol
                        WHERE IdRol = @IdRol;
                    ";

                    var pIdRol = obtener.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    obtener.Parameters.Add(pIdRol);

                    nombreActual = Convert.ToString(await obtener.ExecuteScalarAsync());
                }

                if (string.IsNullOrWhiteSpace(nombreActual))
                {
                    throw new InvalidOperationException("No se encontró el rol seleccionado.");
                }

                if (nombreActual.Trim().Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    dto.Estado = true;
                    nombre = "Administrador";
                }

                await using (var validar = connection.CreateCommand())
                {
                    validar.Transaction = transaction;
                    validar.CommandText = @"
                        SELECT COUNT(1)
                        FROM dbo.Rol
                        WHERE IdRol <> @IdRol
                          AND UPPER(LTRIM(RTRIM(Nombre))) = UPPER(@Nombre);
                    ";

                    var pIdRol = validar.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    validar.Parameters.Add(pIdRol);

                    var pNombre = validar.CreateParameter();
                    pNombre.ParameterName = "@Nombre";
                    pNombre.Value = nombre;
                    validar.Parameters.Add(pNombre);

                    var existe = Convert.ToInt32(await validar.ExecuteScalarAsync());

                    if (existe > 0)
                    {
                        throw new InvalidOperationException("Ya existe otro rol con ese nombre.");
                    }
                }

                await using (var actualizar = connection.CreateCommand())
                {
                    actualizar.Transaction = transaction;
                    actualizar.CommandText = @"
                        UPDATE dbo.Rol
                        SET
                            Nombre = @Nombre,
                            Descripcion = @Descripcion,
                            Estado = @Estado
                        WHERE IdRol = @IdRol;
                    ";

                    var pNombre = actualizar.CreateParameter();
                    pNombre.ParameterName = "@Nombre";
                    pNombre.Value = nombre;
                    actualizar.Parameters.Add(pNombre);

                    var pDescripcion = actualizar.CreateParameter();
                    pDescripcion.ParameterName = "@Descripcion";
                    pDescripcion.Value = descripcion ?? (object)DBNull.Value;
                    actualizar.Parameters.Add(pDescripcion);

                    var pEstado = actualizar.CreateParameter();
                    pEstado.ParameterName = "@Estado";
                    pEstado.Value = dto.Estado;
                    actualizar.Parameters.Add(pEstado);

                    var pIdRol = actualizar.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    actualizar.Parameters.Add(pIdRol);

                    await actualizar.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return new RolRespuestaListadoDto
                {
                    Mensaje = "Rol actualizado correctamente.",
                    Roles = await ListarAsync(null)
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RolOperacionResultadoDto> DesactivarAsync(int idRol)
        {
            if (idRol <= 0)
            {
                throw new InvalidOperationException("Selecciona un rol válido.");
            }

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string? nombre;

                await using (var obtener = connection.CreateCommand())
                {
                    obtener.Transaction = transaction;
                    obtener.CommandText = @"
                        SELECT Nombre
                        FROM dbo.Rol
                        WHERE IdRol = @IdRol;
                    ";

                    var pIdRol = obtener.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    obtener.Parameters.Add(pIdRol);

                    nombre = Convert.ToString(await obtener.ExecuteScalarAsync());
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    throw new InvalidOperationException("No se encontró el rol seleccionado.");
                }

                if (nombre.Trim().Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("No se puede desactivar el rol Administrador.");
                }

                await using (var validarUsuarios = connection.CreateCommand())
                {
                    validarUsuarios.Transaction = transaction;
                    validarUsuarios.CommandText = @"
                        SELECT COUNT(1)
                        FROM dbo.Usuario
                        WHERE IdRol = @IdRol
                          AND Estado = 1;
                    ";

                    var pIdRol = validarUsuarios.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    validarUsuarios.Parameters.Add(pIdRol);

                    var totalUsuarios = Convert.ToInt32(await validarUsuarios.ExecuteScalarAsync());

                    if (totalUsuarios > 0)
                    {
                        throw new InvalidOperationException("No se puede desactivar un rol con usuarios activos asignados.");
                    }
                }

                await using (var desactivar = connection.CreateCommand())
                {
                    desactivar.Transaction = transaction;
                    desactivar.CommandText = @"
                        UPDATE dbo.Rol
                        SET Estado = 0
                        WHERE IdRol = @IdRol;
                    ";

                    var pIdRol = desactivar.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    desactivar.Parameters.Add(pIdRol);

                    await desactivar.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return new RolOperacionResultadoDto
                {
                    IdRol = idRol,
                    Mensaje = "Rol desactivado correctamente."
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<PermisoDto>> ListarPermisosAsync()
        {
            var permisos = new List<PermisoDto>();

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    IdPermiso,
                    Nombre,
                    Descripcion,
                    Estado
                FROM dbo.Permiso
                WHERE Estado = 1
                ORDER BY Nombre;
            ";

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                permisos.Add(new PermisoDto
                {
                    IdPermiso = Convert.ToInt32(reader["IdPermiso"]),
                    Nombre = Convert.ToString(reader["Nombre"]) ?? string.Empty,
                    Descripcion = reader["Descripcion"] == DBNull.Value
                        ? null
                        : Convert.ToString(reader["Descripcion"]),
                    Estado = Convert.ToBoolean(reader["Estado"]),
                    Asignado = false
                });
            }

            return permisos;
        }

        public async Task<List<PermisoDto>> ObtenerPermisosPorRolAsync(int idRol)
        {
            if (idRol <= 0)
            {
                throw new InvalidOperationException("Selecciona un rol válido.");
            }

            var permisos = new List<PermisoDto>();

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    p.IdPermiso,
                    p.Nombre,
                    p.Descripcion,
                    p.Estado,
                    CASE
                        WHEN rp.IdRol IS NULL THEN CAST(0 AS BIT)
                        ELSE CAST(1 AS BIT)
                    END AS Asignado
                FROM dbo.Permiso p
                LEFT JOIN dbo.RolPermiso rp
                    ON rp.IdPermiso = p.IdPermiso
                   AND rp.IdRol = @IdRol
                WHERE p.Estado = 1
                ORDER BY p.Nombre;
            ";

            var pIdRol = command.CreateParameter();
            pIdRol.ParameterName = "@IdRol";
            pIdRol.Value = idRol;
            command.Parameters.Add(pIdRol);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                permisos.Add(new PermisoDto
                {
                    IdPermiso = Convert.ToInt32(reader["IdPermiso"]),
                    Nombre = Convert.ToString(reader["Nombre"]) ?? string.Empty,
                    Descripcion = reader["Descripcion"] == DBNull.Value
                        ? null
                        : Convert.ToString(reader["Descripcion"]),
                    Estado = Convert.ToBoolean(reader["Estado"]),
                    Asignado = Convert.ToBoolean(reader["Asignado"])
                });
            }

            return permisos;
        }

        public async Task<RolOperacionResultadoDto> AsignarPermisosAsync(
            int idRol,
            RolPermisosActualizarDto dto
        )
        {
            if (idRol <= 0)
            {
                throw new InvalidOperationException("Selecciona un rol válido.");
            }

            dto.IdsPermisos ??= new List<int>();

            var idsPermisos = dto.IdsPermisos
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            await using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                string? nombreRol;

                await using (var obtenerRol = connection.CreateCommand())
                {
                    obtenerRol.Transaction = transaction;
                    obtenerRol.CommandText = @"
                        SELECT Nombre
                        FROM dbo.Rol
                        WHERE IdRol = @IdRol
                          AND Estado = 1;
                    ";

                    var pIdRol = obtenerRol.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    obtenerRol.Parameters.Add(pIdRol);

                    nombreRol = Convert.ToString(await obtenerRol.ExecuteScalarAsync());
                }

                if (string.IsNullOrWhiteSpace(nombreRol))
                {
                    throw new InvalidOperationException("No se encontró el rol seleccionado.");
                }

                if (nombreRol.Trim().Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("El rol Administrador mantiene todos los permisos activos.");
                }

                await using (var eliminar = connection.CreateCommand())
                {
                    eliminar.Transaction = transaction;
                    eliminar.CommandText = @"
                        DELETE FROM dbo.RolPermiso
                        WHERE IdRol = @IdRol;
                    ";

                    var pIdRol = eliminar.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    eliminar.Parameters.Add(pIdRol);

                    await eliminar.ExecuteNonQueryAsync();
                }

                foreach (var idPermiso in idsPermisos)
                {
                    await using var insertar = connection.CreateCommand();

                    insertar.Transaction = transaction;
                    insertar.CommandText = @"
                        IF EXISTS (
                            SELECT 1
                            FROM dbo.Permiso
                            WHERE IdPermiso = @IdPermiso
                              AND Estado = 1
                        )
                        BEGIN
                            INSERT INTO dbo.RolPermiso (IdRol, IdPermiso)
                            VALUES (@IdRol, @IdPermiso);
                        END;
                    ";

                    var pIdRol = insertar.CreateParameter();
                    pIdRol.ParameterName = "@IdRol";
                    pIdRol.Value = idRol;
                    insertar.Parameters.Add(pIdRol);

                    var pIdPermiso = insertar.CreateParameter();
                    pIdPermiso.ParameterName = "@IdPermiso";
                    pIdPermiso.Value = idPermiso;
                    insertar.Parameters.Add(pIdPermiso);

                    await insertar.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return new RolOperacionResultadoDto
                {
                    IdRol = idRol,
                    Mensaje = "Permisos del rol actualizados correctamente."
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}