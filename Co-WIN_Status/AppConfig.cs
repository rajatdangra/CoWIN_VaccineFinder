using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Co_WIN_Status
{
    public class AppConfig
    {
        internal static string Email => Convert.ToString(ConfigurationManager.AppSettings["Email"]);
        internal static string PinCode => Convert.ToString(ConfigurationManager.AppSettings["PinCode"]);
        internal static string Phone => Convert.ToString(ConfigurationManager.AppSettings["Phone"]);
        internal static string FirstName => Convert.ToString(ConfigurationManager.AppSettings["FirstName"]);
        internal static string LastName => Convert.ToString(ConfigurationManager.AppSettings["LastName"]);
        internal static string FullName => FirstName + " " + LastName;
        internal static bool SaveUserDetails => String.Equals(ConfigurationManager.AppSettings["SaveUserDetails"], "1");
        internal static int PollingTime => Convert.ToInt32(ConfigurationManager.AppSettings["PollingTime"]);
        internal static string CoWIN_URL => Convert.ToString(ConfigurationManager.AppSettings["CoWIN_URL"]);
        internal static string Mail_Subject => Convert.ToString(ConfigurationManager.AppSettings["Mail_Subject"]);
        internal static int Retry_Count => Convert.ToInt32(ConfigurationManager.AppSettings["Retry_Count"]);
    }
}
