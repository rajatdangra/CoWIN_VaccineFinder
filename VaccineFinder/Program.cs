using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace VaccineFinder
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionTrapper);
            string stInfo = "Vaccine Finder Program Started.";
            logger.Info(stInfo);
            Console.WriteLine(stInfo);

            string emailIdsString = AppConfig.EmailIDs;
            List<string> emailIDs = Email.GetEmailIDs(emailIdsString);
            string PinCode = AppConfig.PinCode;

            UserDetails userDetails = new UserDetails(emailIDs, PinCode, AppConfig.MinAgeLimit)
            {
                FirstName = AppConfig.FirstName,
                LastName = AppConfig.LastName,
                Phone = AppConfig.Phone,
            };

            Console.WriteLine("Pin Code: " + userDetails.PinCode);
            Console.WriteLine("Email Ids: " + emailIdsString);
            Console.WriteLine("Minimum Age Limit: " + userDetails.AgeCriteria + "+");
            Console.WriteLine("First Name (optional): " + userDetails.FirstName);
            Console.WriteLine("Last Name (optional): " + userDetails.LastName);
            Console.WriteLine("Phone (optional): " + userDetails.Phone);
            Console.WriteLine("Please verify to Proceed: Y/N");

            var confirmation = Console.ReadLine();
            while (confirmation.ToLower() != "n" && confirmation.ToLower() != "y")
            {
                stInfo = "Invalid Input. Please Retry.";
                logger.Error(stInfo + ": " + confirmation);
                Console.WriteLine(stInfo);
                Console.WriteLine("Please verify to Proceed: Y/N");
                confirmation = Console.ReadLine();
            }
            if (confirmation.ToLower() == "n")
            {
                Console.WriteLine("Please Enter your Email Ids (Comma separated): ");
                emailIdsString = Console.ReadLine();
                emailIDs = Email.GetEmailIDs(emailIdsString);
                while (!userDetails.IsValidEmailIds(emailIDs))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + emailIdsString);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Email Ids (Comma separated): ");
                    emailIdsString = Console.ReadLine();
                    emailIDs = Email.GetEmailIDs(emailIdsString);
                }
                userDetails.EmailIDs = emailIDs.ToList();

                Console.WriteLine("Please Enter your Pin Code: ");
                PinCode = Console.ReadLine();
                while (!userDetails.isValidPinCode(PinCode))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + PinCode);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Pin Code: ");
                    PinCode = Console.ReadLine();
                }
                userDetails.PinCode = PinCode;

                Console.WriteLine("Please Enter your Min Age Criteria: ");
                var MinAgeCriteria = Console.ReadLine();
                int age;
                while (!int.TryParse(MinAgeCriteria, out age))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + MinAgeCriteria);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Min Age Criteria: ");
                    MinAgeCriteria = Console.ReadLine();
                }
                userDetails.AgeCriteria = age;

                Console.WriteLine("Please Enter your Phone (optional): ");
                var Phone = Console.ReadLine();
                userDetails.Phone = Phone;

                Console.WriteLine("Please Enter your First Name (optional): ");
                var FirstName = Console.ReadLine();
                userDetails.FirstName = FirstName;
                Console.WriteLine("Please Enter your Last Name (optional): ");
                var LastName = Console.ReadLine();
                userDetails.LastName = LastName;

                if (AppConfig.SaveUserDetails)
                {
                    Console.WriteLine("Updating Default Settings");
                    AppConfig.UpdateConfig(userDetails);
                    Console.WriteLine("Updated Default Settings");
                }
            }

            #region Final Validation before Call
            if (!userDetails.IsValidEmailIds(emailIDs))
            {
                while (!userDetails.IsValidEmailIds(emailIDs))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + emailIdsString);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Email Ids (Comma separated): ");
                    emailIdsString = Console.ReadLine();
                    emailIDs = Email.GetEmailIDs(emailIdsString);
                }
                userDetails.EmailIDs = emailIDs.ToList();
            }
            if (!userDetails.isValidPinCode(PinCode))
            {
                while (!userDetails.isValidPinCode(PinCode))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + PinCode);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Pin Code: ");
                    PinCode = Console.ReadLine();
                }
                userDetails.PinCode = PinCode;
            }
            #endregion

            VaccineFinder vf = new VaccineFinder();
            vf.CheckVaccineAvailabilityStatus(userDetails);
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error(e.ExceptionObject.ToString());
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}
