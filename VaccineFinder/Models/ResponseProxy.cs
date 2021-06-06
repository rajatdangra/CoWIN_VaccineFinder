using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder
{
    public class SessionProxy
    {
        public SessionProxy()
        {
            Slots = new List<string>();
        }
        public string SessionID { get; set; }
        public int AvailableCapacity { get; set; }
        public DateTime Date { get; set; }
        public List<string> Slots { get; set; }
        public string Vaccine { get; set; }
        public string CenterName { get; set; }
        public string Address { get; set; }
    }
}
