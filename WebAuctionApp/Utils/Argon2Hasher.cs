using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace WebAuctionApp.Utils
{
    public class Argon2Hasher<AppUser> : IPasswordHasher<AppUser> where AppUser : class
    {
        private const int saltSize = 16;
        private const int hashSize = 16;

        public string HashPassword(AppUser user, string password)
        {
            return Convert.ToBase64String(HashPassword(password));
        }

        public PasswordVerificationResult VerifyHashedPassword(AppUser user, string hashedPassword, string providedPassword)
        {
            if (VerifyHashedPassword(hashedPassword, providedPassword).Equals(true))
                return PasswordVerificationResult.Success;
            else return PasswordVerificationResult.Failed;
        }

        private static byte[] HashPassword(string password)
        {
            byte[] salt = CreateSalt();
            byte[] convPassword = Encoding.ASCII.GetBytes(password);
            byte[] hash = GenerateArgon2Hash(convPassword, salt);

            // Final array that contains the salt + password hash bytes
            byte[] outputBytes = new byte[saltSize + hashSize];
            Buffer.BlockCopy(salt, 0, outputBytes, 0, saltSize);
            Buffer.BlockCopy(hash, 0, outputBytes, saltSize, hashSize);

            // Return the final bytes
            return outputBytes;
        }

        private static bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            try
            {
                byte[] decodedPassword = Convert.FromBase64String(hashedPassword);

                byte[] salt = new byte[saltSize];
                Buffer.BlockCopy(decodedPassword, 0, salt, 0, saltSize);

                // Get the Password Length and read from the offset
                byte[] expectedSubkey = new byte[hashSize];
                Buffer.BlockCopy(decodedPassword, saltSize, expectedSubkey, 0, hashSize);

                // Convert the password to bytes and then perform hashing
                byte[] actualSubkey = GenerateArgon2Hash(Encoding.ASCII.GetBytes(providedPassword), salt);

                // Perform the comparison
                return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] GenerateArgon2Hash(byte[] password, byte[] salt)
        {
            int degreeOfParallelism = 8;
            int iterations = 4;
            int memorySize = 1024 * 1024;

            var argon2 = new Argon2id(password)
            {
                Salt = salt,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize
            };

            return argon2.GetBytes(hashSize);
        }

        private static byte[] CreateSalt()
        {
            var buffer = new byte[saltSize];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);

            return buffer;
        }

        //private static bool ByteArraysEqual(byte[] a, byte[] b)
        //{
        //    if (a == null && b == null)
        //    {
        //        return true;
        //    }
        //    if (a == null || b == null || a.Length != b.Length)
        //    {
        //        return false;
        //    }
        //    var areSame = true;
        //    for (var i = 0; i < a.Length; i++)
        //    {
        //        areSame &= (a[i] == b[i]);
        //    }
        //    return areSame;
        //}
    }
}
