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
            slots = new List<string>();
        }
        public string session_id { get; set; }
        public int availableCapacity { get; set; }
        //public DateTime date { get; set; }
        public List<string> slots { get; set; }
    }
}
