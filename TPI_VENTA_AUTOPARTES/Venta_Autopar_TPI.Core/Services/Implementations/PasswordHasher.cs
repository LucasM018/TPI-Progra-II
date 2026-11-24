using System;
using System.Security.Cryptography;

namespace Venta_Autopar_TPI.Core.Services.Implementations
{
    public static class PasswordHasher
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(
                    "La contraseña no puede estar vacía.");
            }

            // Generamos una sal aleatoria para cada contraseña.
            var salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Generamos el hash utilizando PBKDF2 y SHA-256.
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            // Guardamos algoritmo, iteraciones, sal y hash.
            return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            try
            {
                var parts = storedHash.Split('$');

                if (parts.Length != 4 || parts[0] != "PBKDF2")
                {
                    return false;
                }

                var iterations = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);

                if (iterations < Iterations ||
                    salt.Length != SaltSize ||
                    expectedHash.Length != HashSize)
                {
                    return false;
                }

                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    HashSize);

                return CryptographicOperations.FixedTimeEquals(
                    actualHash,
                    expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}