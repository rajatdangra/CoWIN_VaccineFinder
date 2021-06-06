using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using NLog;

namespace VaccineFinder
{
    public class Hash
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static string ComputeSha256Hash(string rawData)
        {
            logger.Info("Create a SHA256 of: " + rawData);
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                var hash = builder.ToString();
                logger.Info("SHA256 hash of {0} is generated as {1}", rawData, hash);
                return hash;
            }
        }
    }
}
