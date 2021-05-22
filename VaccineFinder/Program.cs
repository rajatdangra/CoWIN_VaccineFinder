using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

            string Phone = AppConfig.Phone;
            string emailIdsString = AppConfig.EmailIDs;
            List<string> EmailIDs = Email.GetEmailIDs(emailIdsString);
            string beneficiaryIdsString = AppConfig.BeneficiaryIDs;
            List<string> BeneficiaryIds = UserPreference.GetBeneficiaryIds(beneficiaryIdsString);
            string PinCode = AppConfig.PinCode;
            DateTime date = DateTime.Now;

            UserDetails userDetails = new UserDetails(Phone, EmailIDs, PinCode, AppConfig.MinAgeLimit, BeneficiaryIds, AppConfig.Dose, AppConfig.SlotPreference, AppConfig.PollingTime, AppConfig.AutoBookCenter)
            {
                FirstName = AppConfig.FirstName,
                LastName = AppConfig.LastName,
            };

            Console.WriteLine("Phone Number: " + userDetails.Phone);
            Console.WriteLine("Pin Code: " + userDetails.UserPreference.PinCode);
            Console.WriteLine("Beneficiary Ids: " + beneficiaryIdsString);
            Console.WriteLine("Minimum Age Limit: " + userDetails.UserPreference.AgeCriteria + "+");
            Console.WriteLine("Dose: " + userDetails.UserPreference.Dose);
            Console.WriteLine("Slot Preference: " + userDetails.UserPreference.SlotPreference);
            Console.WriteLine("Email Ids: " + emailIdsString);
            Console.WriteLine("From Date: " + date.ToString("dd-MM-yyyy"));
            Console.WriteLine("Retry Frequency (Seconds): " + userDetails.UserPreference.PollingTime);
            Console.WriteLine("First Name (optional): " + userDetails.FirstName);
            Console.WriteLine("Last Name (optional): " + userDetails.LastName);
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
            string inputMessage = string.Empty;
            if (confirmation.ToLower() == "n")
            {
                inputMessage = "Please Enter your Phone Number: ";
                Console.WriteLine(inputMessage);
                Phone = Console.ReadLine();
                while (!userDetails.IsValidMobileNumber(Phone))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + Phone);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    Phone = Console.ReadLine();
                }
                userDetails.Phone = Phone;

                inputMessage = "Please Enter your Pin Code: ";
                Console.WriteLine(inputMessage);
                PinCode = Console.ReadLine();
                while (!userDetails.UserPreference.IsValidPinCode(PinCode))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + PinCode);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    PinCode = Console.ReadLine();
                }
                userDetails.UserPreference.PinCode = PinCode;

                inputMessage = "Please Enter your Min Age Criteria: ";
                Console.WriteLine(inputMessage);
                var MinAgeCriteria = Console.ReadLine();
                int age;
                while (!int.TryParse(MinAgeCriteria, out age))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + MinAgeCriteria);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    MinAgeCriteria = Console.ReadLine();
                }
                userDetails.UserPreference.AgeCriteria = age;

                inputMessage = "Please Enter your Dose number: ";
                Console.WriteLine(inputMessage);
                var doseString = Console.ReadLine();
                int dose;
                while (!int.TryParse(doseString, out dose) || (dose != 1 && dose != 2))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + doseString);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    doseString = Console.ReadLine();
                }
                userDetails.UserPreference.Dose = dose;

                inputMessage = "Please Enter Slot Preference (1=> 09:00AM-11:00AM, 2=> 11:00AM-01:00PM, 3=> 01:00PM-03:00PM, 4=> After 03:00PM)";
                Console.WriteLine(inputMessage);
                var slotPreference = Console.ReadLine();
                int slot;
                while (!int.TryParse(slotPreference, out slot) && slot <= 4)
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + slotPreference);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    slotPreference = Console.ReadLine();
                }
                userDetails.UserPreference.SlotPreference = slot;

                inputMessage = "Please Enter 'From Date' (dd-MM-yyyy): ";
                Console.WriteLine(inputMessage);
                var dateString = Console.ReadLine();
                CultureInfo provider = CultureInfo.InvariantCulture;
                while (!DateTime.TryParseExact(dateString, "dd-MM-yyyy", provider, DateTimeStyles.None, out date))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + dateString);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    dateString = Console.ReadLine();
                }

                inputMessage = "Please Enter Retry Frequency (Seconds): ";
                Console.WriteLine(inputMessage);
                var retryFrequency = Console.ReadLine();
                int pollingTime;
                while (!int.TryParse(retryFrequency, out pollingTime))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + retryFrequency);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    retryFrequency = Console.ReadLine();
                }

                inputMessage = "Please Enter your Email Ids (Comma separated): ";
                Console.WriteLine(inputMessage);
                emailIdsString = Console.ReadLine();
                EmailIDs = Email.GetEmailIDs(emailIdsString);
                while (!userDetails.IsValidEmailIds(EmailIDs))
                {
                    stInfo = "Invalid Input. Please Retry.";
                    logger.Error(stInfo + ": " + emailIdsString);
                    Console.WriteLine(stInfo);
                    Console.WriteLine(inputMessage);
                    emailIdsString = Console.ReadLine();
                    EmailIDs = Email.GetEmailIDs(emailIdsString);
                }
                userDetails.EmailIDs = EmailIDs.ToList();

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
            if (!userDetails.IsValidMobileNumber(Phone))
            {
                while (!userDetails.IsValidMobileNumber(Phone))
                {
                    stInfo = "Invalid Phone. Please Retry.";
                    logger.Error(stInfo + ": " + Phone);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Phone Number: ");
                    Phone = Console.ReadLine();
                }
                userDetails.Phone = Phone;
            }

            if (!userDetails.IsValidEmailIds(EmailIDs))
            {
                while (!userDetails.IsValidEmailIds(EmailIDs))
                {
                    stInfo = "Invalid Email. Please Retry.";
                    logger.Error(stInfo + ": " + emailIdsString);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Email Ids (Comma separated): ");
                    emailIdsString = Console.ReadLine();
                    EmailIDs = Email.GetEmailIDs(emailIdsString);
                }
                userDetails.EmailIDs = EmailIDs.ToList();
            }
            if (!userDetails.UserPreference.IsValidPinCode(PinCode))
            {
                while (!userDetails.UserPreference.IsValidPinCode(PinCode))
                {
                    stInfo = "Invalid Pin Code. Please Retry.";
                    logger.Error(stInfo + ": " + PinCode);
                    Console.WriteLine(stInfo);
                    Console.WriteLine("Please Enter your Pin Code: ");
                    PinCode = Console.ReadLine();
                }
                userDetails.UserPreference.PinCode = PinCode;
            }
            #endregion

            VaccineFinder vf = new VaccineFinder(userDetails, date);
            vf.StartFindingVaccine();

            int waitTime = 15;
            Console.WriteLine("Program will be Automatically closed in " + waitTime + " Seconds");
            Thread.Sleep(waitTime * 1000);
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
