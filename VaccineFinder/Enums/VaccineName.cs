using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Enums
{
    public enum VaccineName
    {
        [Description("ANY")]
        Any,
        [Description("COVISHIELD")]
        Covishield,
        [Description("COVAXIN")]
        Covaxin,
        [Description("SPUTNIK V")]
        SputnikV,
    }
}
