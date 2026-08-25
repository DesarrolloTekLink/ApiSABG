using System.Security.Cryptography;

namespace ApiPractice.Models {
    public static class KeyGenerator {
        /// <summary>
        /// Genera una clave de 32 caracteres ASCII a partir de información específica (ej. passphrase).
        /// </summary>
        /// <param name="informacion">Dato de entrada (contraseña, texto, etc.)</param>
        /// <param name="salt">Sal opcional (si no se provee, se usa uno fijo; para seguridad debe ser aleatorio y guardarse)</param>
        /// <param name="iterations">Número de iteraciones (recomendado mínimo 10000)</param>
        /// <returns>Clave de 32 caracteres ASCII válida para EncryptString128Bit</returns>
        public static string GenerarClave(string informacion, byte[] salt = null, int iterations = 10000) {
            if (string.IsNullOrEmpty(informacion))
                throw new ArgumentException("La información no puede estar vacía.");

            // Si no se proporciona salt, usar uno predeterminado (solo para demostración).
            // En producción, usa un salt aleatorio y guárdalo junto con los datos cifrados.
            if (salt == null) {
                salt = new byte[] { 0x1F, 0x2A, 0x3C, 0x4E, 0x5D, 0x6B, 0x7A, 0x8C, 0x9F, 0xA0, 0xB1, 0xC2, 0xD3, 0xE4, 0xF5, 0x06 };
            }

            // Derivar 32 bytes usando PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(informacion, salt, iterations, HashAlgorithmName.SHA256)) {
                byte[] keyBytes = pbkdf2.GetBytes(32); // 32 bytes = 256 bits

                // Convertir los bytes a una cadena Base64 (44 caracteres) y tomar los primeros 32
                string base64Key = Convert.ToBase64String(keyBytes);
                string asciiKey = base64Key.Substring(0, 32);

                // Verificar que todos los caracteres sean ASCII (Base64 lo garantiza: A-Z, a-z, 0-9, +, /)
                return asciiKey;
            }
        }
    }
}
