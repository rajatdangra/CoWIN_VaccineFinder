using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace Co_WIN_Status
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            string stInfo = "Status API Program Started.";
            logger.Info(stInfo);
            Console.WriteLine(stInfo);
            PinCode = AppConfig.PinCode;
            Email = AppConfig.Email;
            AgeLimit = 18;
            Console.WriteLine("Pin Code: " + PinCode);
            Console.WriteLine("Email: " + Email);
            Console.WriteLine("Age-Limit: " + AgeLimit + "+");
            VaccineFinder vf = new VaccineFinder();
            vf.CheckVaccineAvailabilityStatus(Email, PinCode, AgeLimit);
        }

        private static string PinCode;
        private static string Email;
        private static int AgeLimit;
    }
}
