using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VoiceMap_API.AppClasses
{
    public class Methods
    {
        private static readonly byte[] IV2 = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
        static string passphrase = "pass@HGjF8xN0f7W2X";
        private const string secretKey = "mLvcPoWKSTQi1fCiqqCrpBvd3mTTvvzB";
     
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);

                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                foreach (var b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static string GenerateOtp(long userId)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[4];
                rng.GetBytes(randomBytes);

                byte[] userIdBytes = BitConverter.GetBytes(userId);
                byte[] combinedBytes = new byte[userIdBytes.Length + randomBytes.Length];
                Buffer.BlockCopy(userIdBytes, 0, combinedBytes, 0, userIdBytes.Length);
                Buffer.BlockCopy(randomBytes, 0, combinedBytes, userIdBytes.Length, randomBytes.Length);

                using (var sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(combinedBytes);

                    int otpNumber = BitConverter.ToInt32(hash, 0);

                    otpNumber = Math.Abs(otpNumber) % 1000000;

                    return otpNumber.ToString("D6");
                }
            }
        }
        public static async Task<string> DecryptAsync(byte[] encrypted)
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(passphrase);

            int ivLength = aes.BlockSize / 8; // 16 bytes
            byte[] iv = new byte[ivLength];
            Array.Copy(encrypted, 0, iv, 0, ivLength);
            aes.IV = iv;

            using MemoryStream input = new MemoryStream(encrypted, ivLength, encrypted.Length - ivLength);
            using CryptoStream cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream output = new MemoryStream();

            await cryptoStream.CopyToAsync(output);

            return Encoding.Unicode.GetString(output.ToArray());
        }

        public static async Task<byte[]> EncryptAsync(string clearText)
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(passphrase);

            // Generate a random IV
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using MemoryStream output = new MemoryStream();

            await output.WriteAsync(iv, 0, iv.Length);

            using CryptoStream cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);

            await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(clearText));

            cryptoStream.FlushFinalBlock();

            return output.ToArray();
        }
        private static byte[] DeriveKeyFromPassword(string password)
        {
            byte[] salt = new byte[] { 0x1f, 0xa6, 0x3c, 0x9d, 0x4b, 0x7e, 0x58, 0x12 };

            var iterations = 1000;
            var desiredKeyLength = 16; // 128 bits

            using var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.Unicode.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA384);

            return pbkdf2.GetBytes(desiredKeyLength);
        }

        public static string GenerateJwtToken(string Id)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Sid, Id)
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            // Remove any non-hexadecimal characters (e.g., dashes)
            string cleanedHexString = hexString.Replace("-", "");

            // Ensure the cleaned hex string has an even number of characters
            if (cleanedHexString.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid hex string length.");
            }

            // Convert the cleaned hex string to a byte array
            byte[] byteArray = new byte[cleanedHexString.Length / 2];
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(cleanedHexString.Substring(i * 2, 2), 16);
            }

            return byteArray;
        }
        public static async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var message = new MailMessage();
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;
            message.From = new MailAddress("ssam70207@gmail.com");

            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("ssam70207@gmail.com", "dyca cuov slvy oceu"),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(message);
        }

        public static async Task SendEmailWithAttachment(string toEmail, string subject, string htmlMessage, Stream? attachmentStream = null, string attachmentName = "Attachment.txt")
        {
            var message = new MailMessage
            {
                From = new MailAddress("ssam70207@gmail.com"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            if (attachmentStream != null)
            {
                attachmentStream.Position = 0;

                var attachment = new Attachment(attachmentStream, attachmentName, MediaTypeNames.Text.Plain);
                message.Attachments.Add(attachment);
            }

            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("ssam70207@gmail.com", "dyca cuov slvy oceu"),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
        }

        public static bool IsHexString(string input)
        {
            return input.All(c => "0123456789abcdefABCDEF".Contains(c));
        }

        public static async Task<string> UploadFileAsync(IFormFile file, string folderName, string rootDirectory = "wwwroot")
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), rootDirectory, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine($"/{folderName}", fileName).Replace("\\", "/");
        }

        public static string GenerateSecretKey()
        {
            const string chars = "g6zjcJcgE9HQPeSQwJJreAuhGAUvkE2M5vp1v68Ktddb70mKdDC9kmHj4mXGr0x";
            var random = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < 15; i++)
            {
                int index = random.Next(chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString(); 
        }

        private static string GenerateRandomString(int length)
        {
            Random random = new Random();

            const string chars = "yzNlAKLrB8Ak0IwPzxZqC70i8lvxuc4IMuC3k9qM7QHpLBWKVeAyFrXI43pOw9JzSDWDQw";
            char[] stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        public static string GetOrigin()
        {
            return "http://localhost:4200";
        }
        public static string GeneratePostUrl()
        {
            string origin = GetOrigin();
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = GenerateRandomString(8);
            return $"{origin}/main/{datePart}{randomPart}";
        }
    }
}
