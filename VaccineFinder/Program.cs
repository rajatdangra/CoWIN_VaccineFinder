using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using VaccineFinder.Models;

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
            ConsoleMethods.PrintSuccess(stInfo);

            AppConfig.UpdateConfig();

            CheckSoftwareVersion();

            string Phone = AppConfig.Phone;
            string emailIdsString = AppConfig.EmailIDs;
            List<string> EmailIDs = EmailNotifier.GetEmailIDs(emailIdsString);
            string beneficiaryIdsString = AppConfig.BeneficiaryIDs;
            List<string> BeneficiaryIds = UserPreference.GetBeneficiaryIds(beneficiaryIdsString);
            string pinCodesString = AppConfig.PinCodes;
            List<string> PinCodes = UserPreference.GetBeneficiaryIds(pinCodesString);
            DateTime date = DateTime.Now;

            UserDetails userDetails = new UserDetails(Phone, EmailIDs, PinCodes, AppConfig.MinAgeLimit, BeneficiaryIds, AppConfig.Dose, AppConfig.IsPrecautionDose, AppConfig.SlotPreference, AppConfig.PollingTime, AppConfig.AutoPickCenter, AppConfig.IncludePaidService, AppConfig.Vaccine, AppConfig.TelegramChatID)
            {
                FirstName = AppConfig.FirstName,
                LastName = AppConfig.LastName,
            };

            Console.WriteLine("Phone Number: " + userDetails.Phone);
            Console.WriteLine("Pin Codes: " + pinCodesString);
            Console.WriteLine("Beneficiary Ids: " + beneficiaryIdsString);
            Console.WriteLine("Minimum Age Limit: " + userDetails.UserPreference.AgeCriteria + "+");
            Console.WriteLine("Dose: " + userDetails.UserPreference.Dose);
            Console.WriteLine("Is Precaution Dose: " + userDetails.UserPreference.IsPrecautionDose);
            Console.WriteLine("Vaccine: " + userDetails.UserPreference.Vaccine);
            Console.WriteLine("Slot Preference: " + userDetails.UserPreference.SlotPreference);
            Console.WriteLine("Auto-Pick Center: " + userDetails.UserPreference.AutoPickCenter);
            Console.WriteLine("Include Paid Service: " + userDetails.UserPreference.IncludePaidService);
            Console.WriteLine("Email Ids: " + emailIdsString);
            Console.WriteLine("Telegram Chat ID: " + userDetails.TelegramChatID);
            Console.WriteLine("From Date: " + date.ToString("dd-MM-yyyy"));
            Console.WriteLine("Retry Frequency (Seconds): " + userDetails.UserPreference.PollingTime);
            Console.WriteLine("First Name (optional): " + userDetails.FirstName);
            Console.WriteLine("Last Name (optional): " + userDetails.LastName);

            var finalConfirmationMessage = "Are you good to go with these settings: Y/N ?";
            var finalConfirmation = TakeConfirmation(finalConfirmationMessage);
            string inputMessage = string.Empty;
            if (finalConfirmation.ToLower() == "n")
            {
                string updateConfirmationMessage = string.Empty;
                string updateConfirmation = string.Empty;
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Pin Codes");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter your Pin Codes (Comma separated):";
                        Console.WriteLine(inputMessage);
                        pinCodesString = Console.ReadLine();
                        PinCodes = UserPreference.GetPincodes(pinCodesString);
                        while (!userDetails.UserPreference.IsValidPinCodes(PinCodes))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + pinCodesString);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            pinCodesString = Console.ReadLine();
                            PinCodes = UserPreference.GetPincodes(pinCodesString);
                        }
                        userDetails.UserPreference.PinCodes = PinCodes.ToList();
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Min Age Criteria");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter your Min Age Criteria:";
                        Console.WriteLine(inputMessage);
                        var MinAgeCriteria = Console.ReadLine();
                        int age;
                        while (!int.TryParse(MinAgeCriteria, out age))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + MinAgeCriteria);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            MinAgeCriteria = Console.ReadLine();
                        }
                        userDetails.UserPreference.AgeCriteria = age;
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Dose number");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter your Dose number:";
                        Console.WriteLine(inputMessage);
                        var doseString = Console.ReadLine();
                        int dose;
                        while (!int.TryParse(doseString, out dose) || (dose != 1 && dose != 2))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + doseString);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            doseString = Console.ReadLine();
                        }
                        userDetails.UserPreference.Dose = dose;
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    inputMessage = "Is Precaution Dose: Y/N ?";
                    finalConfirmation = TakeConfirmation(inputMessage);
                    userDetails.UserPreference.IsPrecautionDose = (finalConfirmation.ToLower() == "y");

                    finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Phone Number");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter your Phone Number:";
                        Console.WriteLine(inputMessage);
                        Phone = Console.ReadLine();
                        while (!userDetails.IsValidMobileNumber(Phone))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + Phone);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            Phone = Console.ReadLine();
                        }
                        userDetails.Phone = Phone;
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Beneficiary IDs");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Clearing Beneficiary IDs, it will be updated in run time";
                        Console.WriteLine(inputMessage);

                        userDetails.UserPreference.BeneficiaryIds = new List<string>();
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Slot Preference");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter Slot Preference (1=> 09:00AM-11:00AM, 2=> 11:00AM-01:00PM, 3=> 01:00PM-03:00PM, 4=> After 03:00PM)";
                        Console.WriteLine(inputMessage);
                        var slotPreference = Console.ReadLine();
                        int slot;
                        while (!int.TryParse(slotPreference, out slot) && slot <= 4)
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + slotPreference);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            slotPreference = Console.ReadLine();
                        }
                        userDetails.UserPreference.SlotPreference = slot;
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Vaccine");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter your Vaccine: " + userDetails.UserPreference.GetVaccineNames();
                        Console.WriteLine(inputMessage);
                        var vaccine = Console.ReadLine();
                        while (!userDetails.UserPreference.IsValidVaccine(vaccine))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + vaccine);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            vaccine = Console.ReadLine();
                        }
                        userDetails.UserPreference.Vaccine = vaccine;
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Retry Frequency");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter Retry Frequency (Seconds):";
                        Console.WriteLine(inputMessage);
                        var retryFrequency = Console.ReadLine();
                        int pollingTime;
                        while (!int.TryParse(retryFrequency, out pollingTime))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + retryFrequency);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            retryFrequency = Console.ReadLine();
                        }
                        userDetails.UserPreference.PollingTime = pollingTime;
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    inputMessage = "Auto-Pick Center: Y/N ?";
                    finalConfirmation = TakeConfirmation(inputMessage);
                    userDetails.UserPreference.AutoPickCenter = (finalConfirmation.ToLower() == "y");

                    finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("From Date");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter 'From Date' (dd-MM-yyyy):";
                        Console.WriteLine(inputMessage);
                        var dateString = Console.ReadLine();
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        while (!DateTime.TryParseExact(dateString, "dd-MM-yyyy", provider, DateTimeStyles.None, out date))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + dateString);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            dateString = Console.ReadLine();
                        }
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Email Ids");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        inputMessage = "Please Enter your Email Ids (Comma separated):";
                        Console.WriteLine(inputMessage);
                        emailIdsString = Console.ReadLine();
                        EmailIDs = EmailNotifier.GetEmailIDs(emailIdsString);
                        while (!userDetails.IsValidEmailIds(EmailIDs))
                        {
                            stInfo = "Invalid Input. Please Retry.";
                            logger.Info(stInfo + ": " + emailIdsString);
                            ConsoleMethods.PrintError(stInfo);
                            Console.WriteLine(inputMessage);
                            emailIdsString = Console.ReadLine();
                            EmailIDs = EmailNotifier.GetEmailIDs(emailIdsString);
                        }
                        userDetails.EmailIDs = EmailIDs.ToList();
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("First/Last Name");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        Console.WriteLine("Please Enter your First Name (optional):");
                        var FirstName = Console.ReadLine();
                        userDetails.FirstName = FirstName;
                        Console.WriteLine("Please Enter your Last Name (optional):");
                        var LastName = Console.ReadLine();
                        userDetails.LastName = LastName;
                        finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                    }
                }
                if (finalConfirmation.ToLower() == "n")
                {
                    inputMessage = "Include Paid Service: Y/N ?";
                    finalConfirmation = TakeConfirmation(inputMessage);
                    userDetails.UserPreference.IncludePaidService = (finalConfirmation.ToLower() == "y");
                    finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                }

                if (finalConfirmation.ToLower() == "n")
                {
                    updateConfirmationMessage = CreateCustomMessage("Telegram Chat ID");
                    updateConfirmation = TakeConfirmation(updateConfirmationMessage);

                    if (updateConfirmation.ToLower() == "y")
                    {
                        if (!AppConfig.SendTelegramNotification)
                        {
                            stInfo = $"[WARNING] Telegram Notifications are Turned OFF in appsettings.json. Turn ON to get notified.";
                            ConsoleMethods.PrintError(stInfo);
                            logger.Info(stInfo);
                        }

                        var updates = APIs.FetchUpdatesFromTelegramBot();
                        if (updates != null)
                        {
                            bool isCorrectUserName = false;
                            while (!isCorrectUserName)
                            {
                                stInfo = "[INFO] Make sure to ping ";
                                ConsoleMethods.PrintProgress(stInfo, sameLine: true);
                                stInfo = "@Covid19VaccineFinderBot";
                                ConsoleMethods.PrintInfo(stInfo, sameLine: true);
                                stInfo = " on Telegram before proceeding.";
                                ConsoleMethods.PrintProgress(stInfo);

                                inputMessage = "Please enter your Telegram UserName:";
                                Console.WriteLine(inputMessage);
                                var telegramUserName = Console.ReadLine();

                                var messages = updates.result.Where(a => a.message != null).Select(a => a.message);
                                var filteredChats = messages.Where(a => a.from.username != null && a.from.username.ToLower() == telegramUserName.ToLower());
                                if (filteredChats.Count() > 0)
                                {
                                    isCorrectUserName = true; //break loop condition

                                    var counter = 1;
                                    HashSet<long> chatIDs = new HashSet<long>();
                                    foreach (var msg in filteredChats)
                                    {
                                        if (chatIDs.Add(msg.chat.id))
                                        {
                                            var st = counter + ")" + " " + msg.chat.description;
                                            Console.WriteLine(st);
                                            counter++;
                                        }
                                    }
                                    inputMessage = "Please enter your preferred Chat ID (number) to receive notifications:";
                                    ConsoleMethods.PrintProgress(inputMessage);
                                    var chatIDString = Console.ReadLine();
                                    int chatID;
                                    while (!int.TryParse(chatIDString, out chatID) || chatID > chatIDs.Count || chatID < 1)
                                    {
                                        stInfo = "Invalid Input. Please Retry.";
                                        logger.Info(stInfo + ": " + chatIDString);
                                        ConsoleMethods.PrintError(stInfo);
                                        ConsoleMethods.PrintProgress(inputMessage);
                                        chatIDString = Console.ReadLine();
                                    }
                                    userDetails.TelegramChatID = Convert.ToString(chatIDs.ElementAt(chatID - 1));
                                    //finalConfirmation = TakeConfirmation(finalConfirmationMessage);
                                }
                                else
                                {
                                    stInfo = $"Couldn't find update with the UserName: {telegramUserName}, in last 24 hours";
                                    ConsoleMethods.PrintError(stInfo);
                                }
                            }
                        }
                    }
                }

                if (AppConfig.SaveUserDetails)
                {
                    ConsoleMethods.PrintProgress("Updating Default Settings");
                    AppConfig.UpdateConfig(userDetails);
                    ConsoleMethods.PrintSuccess("Updated Default Settings");
                }
            }

            #region Final Validation before Call
            if (!userDetails.IsValidMobileNumber(Phone))
            {
                while (!userDetails.IsValidMobileNumber(Phone))
                {
                    stInfo = "Invalid Phone. Please Retry.";
                    logger.Info(stInfo + ": " + Phone);
                    ConsoleMethods.PrintError(stInfo);
                    Console.WriteLine("Please Enter your Phone Number:");
                    Phone = Console.ReadLine();
                }
                userDetails.Phone = Phone;
            }

            if (!userDetails.UserPreference.IsValidPinCodes(PinCodes))
            {
                while (!userDetails.UserPreference.IsValidPinCodes(PinCodes))
                {
                    stInfo = "Invalid Pin Codes. Please Retry.";
                    logger.Info(stInfo + ": " + pinCodesString);
                    ConsoleMethods.PrintError(stInfo);
                    Console.WriteLine("Please Enter your Pin Codes:");
                    pinCodesString = Console.ReadLine();
                    PinCodes = UserPreference.GetPincodes(pinCodesString);
                }
                userDetails.UserPreference.PinCodes = PinCodes;
            }

            if (!userDetails.IsValidEmailIds(EmailIDs))
            {
                while (!userDetails.IsValidEmailIds(EmailIDs))
                {
                    stInfo = "Invalid Email. Please Retry.";
                    logger.Info(stInfo + ": " + emailIdsString);
                    ConsoleMethods.PrintError(stInfo);
                    Console.WriteLine("Please Enter your Email Ids (Comma separated):");
                    emailIdsString = Console.ReadLine();
                    EmailIDs = EmailNotifier.GetEmailIDs(emailIdsString);
                }
                userDetails.EmailIDs = EmailIDs.ToList();
            }
            #endregion

            VaccineFinder vf = new VaccineFinder(userDetails, date);
            vf.StartFindingVaccine();

            Thread soundThread = new Thread(() => Sound.PlayAsterisk(1));
            soundThread.Start();

            int waitTime = AppConfig.AutomaticCloseProgramWaitTime;
            ConsoleMethods.PrintInfo("Program will be Automatically closed in " + waitTime + " Seconds", color: ConsoleColor.Cyan);
            Thread.Sleep(waitTime * 1000);
        }

        private static void CheckSoftwareVersion()
        {
            var shouldAppBeAllowedToRun = new VersionChecker().EvaluateCurrentSoftwareVersion();
            if (!shouldAppBeAllowedToRun)
                Environment.Exit(0);
        }

        public static string CreateCustomMessage(string propertyName)
        {
            return string.Format("Would you like to update {0} ? : Y / N", propertyName);
        }

        public static string TakeConfirmation(string message)
        {
            String stInfo = string.Empty;
            Console.WriteLine(message);

            var confirmation = Console.ReadLine();
            while (confirmation.ToLower() != "n" && confirmation.ToLower() != "y")
            {
                stInfo = "Invalid Input. Please Retry.";
                logger.Info(stInfo + ": " + confirmation);
                ConsoleMethods.PrintError(stInfo);
                Console.WriteLine(message);
                confirmation = Console.ReadLine();
            }
            return confirmation;
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            string errorInfo = e.ExceptionObject.ToString();
            string detailedErrorInfo = string.Format("An error has been encountered in Vaccine Finder Application, Details are mentioned below:\nUser Name: {0}\nEmail: {1}\nPhone: {2}\n\nError Information:\n{3}", AppConfig.FullName, AppConfig.EmailIDs, AppConfig.Phone, errorInfo);
            logger.Error(detailedErrorInfo);
            Console.WriteLine(detailedErrorInfo);
            if (AppConfig.SendEmail)
            {
                INotifier iNotifier = new EmailNotifier("Vaccine Finder - Error Occured", EmailNotifier.DeveloperEmail, EmailNotifier.DeveloperName, isHTMLBody: false);
                NotifierFactory notifier = new NotifierFactory(iNotifier);
                notifier.Notify(detailedErrorInfo);
            }
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}
