using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccineFinder.Enums;

namespace VaccineFinder.Providers
{
    public class Authenticator
    {
        public TokenType TokenType { get; set; }
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsEncode { get; set; }
    }
}
