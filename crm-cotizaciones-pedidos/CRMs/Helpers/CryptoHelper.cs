using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CRMs.Helpers
{
    /// <summary>
    /// Provee funciones para encriptar y desencriptar texto usando AES con IV aleatorio y expiración.
    /// </summary>
    public class CryptoHelper
    {
        // Clave AES (16, 24 o 32 bytes). Se lee de Web.config (appSettings "CryptoKey"); nunca la incluyas en el código.
        private static readonly byte[] Clave = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["CryptoKey"] ?? throw new InvalidOperationException("Falta appSetting CryptoKey"));

        /// <summary>
        /// Encripta un texto con AES + IV aleatorio + expiración (en ticks).
        /// </summary>
        /// <param name="plainText">Texto plano a encriptar.</param>
        /// <param name="expiracion">Tiempo de vida del token.</param>
        /// <returns>Token seguro en base64.</returns>
        public static string EncriptarConIV(string plainText, TimeSpan expiracion)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Clave;
                aes.GenerateIV(); // IV aleatorio

                var iv = aes.IV;

                // Incluir expiración en ticks
                var tiempoExpiracion = DateTime.UtcNow.Add(expiracion).Ticks;
                var dataConExpiracion = $"{plainText}|{tiempoExpiracion}";
                var plainBytes = Encoding.UTF8.GetBytes(dataConExpiracion);

                var encryptor = aes.CreateEncryptor();
                var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Combinar IV + cifrado
                var resultado = new byte[iv.Length + cipherBytes.Length];
                Buffer.BlockCopy(iv, 0, resultado, 0, iv.Length);
                Buffer.BlockCopy(cipherBytes, 0, resultado, iv.Length, cipherBytes.Length);

                return Convert.ToBase64String(resultado);
            }            
        }

        /// <summary>
        /// Desencripta un token encriptado con IV + AES + expiración.
        /// </summary>
        /// <param name="encryptedText">Texto en base64 que contiene IV + AES</param>
        /// <returns>Texto original si no ha expirado.</returns>
        /// <exception cref="Exception">Si el token es inválido o ha expirado.</exception>
        public static string DesencriptarConIV(string encryptedText)
        {
            var fullBytes = Convert.FromBase64String(encryptedText);

            using (var aes = Aes.Create())
            {
                aes.Key = Clave;

                // Separar IV (primeros 16 bytes) y mensaje cifrado
                var iv = new byte[16];
                var cipher = new byte[fullBytes.Length - 16];

                Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullBytes, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                var decryptor = aes.CreateDecryptor();
                var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                var data = Encoding.UTF8.GetString(plainBytes);

                // Validar formato y expiración
                var partes = data.Split('|');
                if (partes.Length != 2)
                    throw new Exception("Token inválido");

                var ticksExp = long.Parse(partes[1]);
                if (DateTime.UtcNow.Ticks > ticksExp)
                    throw new Exception("Token expirado");

                return partes[0];
            }            
        }
    }
}