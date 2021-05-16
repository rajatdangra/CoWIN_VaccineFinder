using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Co_WIN_Status
{
    public class UserDetails
    {
        public UserDetails(string email, string pinCode, int ageCriteria)
        {
            Email = email;
            PinCode = pinCode;
            AgeCriteria = ageCriteria;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PinCode { get; set; }
        public string Phone { get; set; }
        public int AgeCriteria { get; set; }
    }
}
