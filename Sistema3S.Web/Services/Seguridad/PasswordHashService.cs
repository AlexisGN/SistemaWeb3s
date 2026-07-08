using System.Security.Cryptography;

namespace Sistema3S.Web.Services.Seguridad
{
    public class PasswordHashService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iteraciones = 210000;

        public string CrearHash(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new InvalidOperationException("La contraseña no puede estar vacía.");
            }

            using var rng = RandomNumberGenerator.Create();

            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var key = Rfc2898DeriveBytes.Pbkdf2(
                password: contrasena,
                salt: salt,
                iterations: Iteraciones,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: KeySize
            );

            return $"PBKDF2$SHA256${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public bool Verificar(string contrasena, string hashGuardado)
        {
            if (string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(hashGuardado))
            {
                return false;
            }

            var partes = hashGuardado.Split('$');

            if (partes.Length != 5)
            {
                return false;
            }

            if (!partes[0].Equals("PBKDF2", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!partes[1].Equals("SHA256", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!int.TryParse(partes[2], out var iteraciones))
            {
                return false;
            }

            byte[] salt;
            byte[] keyGuardada;

            try
            {
                salt = Convert.FromBase64String(partes[3]);
                keyGuardada = Convert.FromBase64String(partes[4]);
            }
            catch
            {
                return false;
            }

            var keyCalculada = Rfc2898DeriveBytes.Pbkdf2(
                password: contrasena,
                salt: salt,
                iterations: iteraciones,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: keyGuardada.Length
            );

            return CryptographicOperations.FixedTimeEquals(keyCalculada, keyGuardada);
        }

        public bool EsHashValido(string? hashGuardado)
        {
            if (string.IsNullOrWhiteSpace(hashGuardado))
            {
                return false;
            }

            var partes = hashGuardado.Split('$');

            return partes.Length == 5 &&
                   partes[0].Equals("PBKDF2", StringComparison.OrdinalIgnoreCase) &&
                   partes[1].Equals("SHA256", StringComparison.OrdinalIgnoreCase);
        }
    }
}