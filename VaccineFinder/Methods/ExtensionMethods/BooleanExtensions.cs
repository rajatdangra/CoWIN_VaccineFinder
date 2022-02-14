using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Methods.ExtensionMethods
{
    public static class BooleanExtensions
    {
        public static string ConvertToString(this bool TrueOrFalse)
        {
            return TrueOrFalse ? "Yes" : "No";
        }
    }
}
