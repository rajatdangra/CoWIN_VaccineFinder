using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VaccineFinder.Models;
using VaccineFinder.Templates.EmailTemplates;
using VaccineFinder.Templates.MessageTemplates;

namespace VaccineFinder
{
    public class VaccineFinder
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public VaccineFinder(UserDetails userDetails, DateTime date)
        {
            UserDetails = userDetails;
            Date = date;
        }
        private UserDetails UserDetails;
        private DateTime Date;

        public void StartFindingVaccine()
        {
            string stInfo = string.Empty;
            string inputMessage = string.Empty;

            #region Mobile Otp Validation

            var otpAuth = new OTPAuthenticator();
            otpAuth.ValidateUser(UserDetails.Phone);

            #endregion

            #region Verify Beneficiaries
            if (AppConfig.VerifyBeneficiaries)
            {
                if (!VerifyBeneficiaries())
                    return;
            }
            #endregion

            #region SlotAvailability and Bookings
            int sessionNumber = -1;
            var sessions = CheckVaccineAvailabilityStatus();
            if (sessions != null && sessions.Count > 0)
            {
                if (UserDetails.UserPreference.AutoPickCenter)
                {
                    ////Sort based on More Available Capacity
                    //sessions = sessions.OrderByDescending(a => a.availableCapacity).ToList();
                    sessionNumber = 1;
                    stInfo = "Proceeding to Book Slot Automatically (Auto-Pick Center is True)";
                    ConsoleMethods.PrintProgress(stInfo);
                    logger.Info(stInfo);
                }
                else
                {
                    stInfo = "Proceeding to Book Slot Manually (Auto-Pick Center is False)";
                    ConsoleMethods.PrintProgress(stInfo);
                    logger.Info(stInfo);

                    Thread soundThread = new Thread(() => Sound.PlayBeep(1));
                    soundThread.Start();

                    inputMessage = "Please enter your preferred Center Number:";
                    ConsoleMethods.PrintProgress(inputMessage);
                    var sessionNumberString = Console.ReadLine();
                    while (!int.TryParse(sessionNumberString, out sessionNumber) || sessionNumber > sessions.Count || sessionNumber < 1)
                    {
                        stInfo = "Invalid Input. Please Retry.";
                        logger.Info(stInfo + ": " + sessionNumberString);
                        ConsoleMethods.PrintError(stInfo);
                        ConsoleMethods.PrintProgress(inputMessage);
                        sessionNumberString = Console.ReadLine();
                    }
                }
                BookSlot(sessions, sessionNumber, UserDetails.UserPreference.SlotPreference);
            }
            #endregion
        }

        public bool VerifyBeneficiaries()
        {
            bool areBeneficiariesVerified = false;
            string stInfo = "VerifyBeneficiaries Call Started for phone: " + UserDetails.Phone;
            logger.Info(stInfo);
            Console.WriteLine("\n" + stInfo);

            GetBeneficiariesResponse response = null;
            try
            {
                response = APIs.GetBeneficiaries(UserDetails.Phone);

                if (response != null)
                {
                    stInfo = "Beneficiaries fetched Successfully!";
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);

                    bool updateRequired = false;
                    while (true)
                    {
                        while (!AreBeneficiariesVerified(response))
                        {
                            updateRequired = true;
                            Console.WriteLine("\nBelow are the beneficiaries registered in your account:");
                            int counter = 0;
                            foreach (var ben in response.beneficiaries)
                            {
                                counter++;
                                var st = counter + ")" + " " + ben.Description;
                                Console.WriteLine(st);
                            }
                            Console.WriteLine("\nPlease enter comma separated beneficiary Ids:");
                            var benInput = Console.ReadLine();
                            UserDetails.UserPreference.BeneficiaryIds = UserPreference.GetBeneficiaryIds(benInput);
                        }
                        string previousVaccine;
                        if (!HaveSameDoseAndVaccine(response, out previousVaccine))
                        {
                            Console.WriteLine("\nPlease enter comma separated beneficiary Ids with Same 'Dose and Vaccine':");
                            var benInput = Console.ReadLine();
                            UserDetails.UserPreference.BeneficiaryIds = UserPreference.GetBeneficiaryIds(benInput);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(previousVaccine)) //Scenario for Dose > 1
                            {
                                stInfo = "Checking if vaccine specified is same";
                                ConsoleMethods.PrintProgress(stInfo);
                                logger.Info(stInfo);
                                if (previousVaccine.ToUpper().Equals(UserDetails.UserPreference.Vaccine.ToUpper()))
                                {
                                    stInfo = "Vaccine specified is same as previous vaccine";
                                    ConsoleMethods.PrintSuccess(stInfo);
                                    logger.Info(stInfo);
                                }
                                else
                                {
                                    stInfo = $"Vaccine specified: {UserDetails.UserPreference.Vaccine.ToUpper()}, is not same as Previous Vaccine: {previousVaccine}";
                                    ConsoleMethods.PrintInfo(stInfo, ConsoleColor.DarkYellow);
                                    logger.Info(stInfo);
                                    stInfo = $"Updating Vaccine: {previousVaccine}";
                                    ConsoleMethods.PrintInfo(stInfo, ConsoleColor.DarkCyan);
                                    logger.Info(stInfo);
                                    UserDetails.UserPreference.Vaccine = previousVaccine;
                                    updateRequired = true;
                                }
                                //update this code
                                if (!IsDoseSpecifiedValid(dose: 2))
                                    updateRequired = true;
                            }
                            else //Scenario for Dose:1
                            {
                                if (!IsDoseSpecifiedValid(dose: 1))
                                    updateRequired = true;
                            }
                            break;
                        }
                    }
                    areBeneficiariesVerified = true;

                    if (updateRequired && AppConfig.SaveUserDetails)
                    {
                        ConsoleMethods.PrintProgress("Updating Default Settings");
                        AppConfig.UpdateConfig(UserDetails);
                        ConsoleMethods.PrintSuccess("Updated Default Settings");
                    }
                }
                else
                {
                    areBeneficiariesVerified = false;
                    stInfo = "Unable to GetBeneficiaries";
                    logger.Info(stInfo);
                    //Console.WriteLine(stInfo);
                }
                return areBeneficiariesVerified;
            }
            catch (Exception ex)
            {
                stInfo = "Error in GetBeneficiaries:\n" + ex;
                logger.Error(stInfo);
                ConsoleMethods.PrintError(stInfo);
                return false;
            }
        }

        public bool AreBeneficiariesVerified(GetBeneficiariesResponse response)
        {
            bool areBeneficiariesVerified = true;
            string stInfo = "Verifying Beneficiaries";
            Console.WriteLine("\n" + stInfo);
            logger.Info(stInfo);
            bool benInputStEmpty = string.IsNullOrWhiteSpace(UserDetails.UserPreference.BeneficiaryIdsString);
            if (!benInputStEmpty)
            {
                foreach (var benId in UserDetails.UserPreference.BeneficiaryIds)
                {
                    var benDetails = response.beneficiaries.FirstOrDefault(a => a.beneficiary_reference_id == benId);
                    if (benDetails != null)
                    {
                        stInfo = $"Beneficiary Id {benId} is valid, User Name: {benDetails.name}, Status: {benDetails.vaccination_status}, Vaccine: {benDetails.vaccine}";
                        ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkCyan);
                        logger.Info(stInfo);
                    }
                    else
                    {
                        areBeneficiariesVerified = false;
                        stInfo = "Beneficiary Id is invalid: " + benId;
                        ConsoleMethods.PrintError(stInfo);
                        logger.Info(stInfo);
                    }
                }
            }
            else //In case Beneficiary IDs are not specified
            {
                areBeneficiariesVerified = false;
            }
            return areBeneficiariesVerified;
        }

        public bool HaveSameDoseAndVaccine(GetBeneficiariesResponse response, out string previousVaccineName)
        {
            previousVaccineName = string.Empty;
            bool areDoseAndVaccineVerified = false;
            string stInfo = "Verifying if beneficiaries have same(valid) Dose and Vaccine";
            Console.WriteLine("\n" + stInfo);
            logger.Info(stInfo);

            List<string> previousVaccines = response.beneficiaries.Where(a => UserDetails.UserPreference.BeneficiaryIds.Contains(a.beneficiary_reference_id)).Select(a => a.vaccine).ToList();
            if (previousVaccines.Distinct().Count() > 1)
            {
                areDoseAndVaccineVerified = false;
                stInfo = $"Multiple vaccines found: {string.Join(", ", previousVaccines.Distinct())}";
                ConsoleMethods.PrintError(stInfo);
                logger.Info(stInfo);
            }
            else
            {
                previousVaccineName = previousVaccines.First();
                areDoseAndVaccineVerified = true;
                if (!string.IsNullOrWhiteSpace(previousVaccineName))
                {
                    stInfo = $"Vaccine: {string.Join(", ", previousVaccines.Distinct())}";
                    ConsoleMethods.PrintProgress(stInfo);
                    logger.Info(stInfo);
                }
            }

            return areDoseAndVaccineVerified;
        }

        public bool IsDoseSpecifiedValid(int dose)
        {
            bool isDoseSpecifiedValid = false;
            string stInfo = "Checking if Dose specified is valid";
            ConsoleMethods.PrintProgress(stInfo);
            logger.Info(stInfo);
            if (UserDetails.UserPreference.Dose == dose)
            {
                isDoseSpecifiedValid = true;
                stInfo = $"Dose specified {UserDetails.UserPreference.Dose} is valid";
                ConsoleMethods.PrintSuccess(stInfo);
                logger.Info(stInfo);
            }
            else
            {
                isDoseSpecifiedValid = false;
                stInfo = $"Dose specified {UserDetails.UserPreference.Dose} is invalid";
                ConsoleMethods.PrintInfo(stInfo, ConsoleColor.DarkYellow);
                logger.Info(stInfo);

                stInfo = $"Updating Dose: {dose}";
                ConsoleMethods.PrintInfo(stInfo, ConsoleColor.DarkCyan);
                logger.Info(stInfo);
                UserDetails.UserPreference.Dose = dose;
            }
            return isDoseSpecifiedValid;
        }

        public List<SessionProxy> CheckVaccineAvailabilityStatus()
        {
            string stInfo = "Status Call Started for Pin Codes: " + UserDetails.UserPreference.PinCodeString;
            logger.Info(stInfo);
            Console.WriteLine("\n" + stInfo);

            List<SessionProxy> sessions = new List<SessionProxy>();
            try
            {
                bool vaccineSlotFound = false;
                bool errorOccured = false;
                int retryCount = 0;
                while (!vaccineSlotFound && !errorOccured)
                {
                    stInfo = "Status Call End for Pin Codes: " + UserDetails.UserPreference.PinCodeString;
                    logger.Info(stInfo);
                    //Console.WriteLine("\n" + stInfo);

                    int counter = 0;
                    StringBuilder slots = new StringBuilder();
                    foreach (var pinCode in UserDetails.UserPreference.PinCodes)
                    {
                        var sessionsByPin = CheckVaccineAvailabilityStatusByPin(pinCode, counter, ref slots, ref retryCount);
                        if (sessionsByPin == null)
                        {
                            errorOccured = true;
                            break;
                        }
                        if (sessionsByPin.Count > 0)
                        {
                            counter += sessionsByPin.Count;
                            vaccineSlotFound = true;
                            if (UserDetails.UserPreference.AutoPickCenter)
                            {
                                //Sort based on More Available Capacity
                                sessionsByPin = sessionsByPin.OrderByDescending(a => a.AvailableCapacity).ToList();
                            }
                            sessions.AddRange(sessionsByPin);
                        }
                    }

                    if (vaccineSlotFound)
                    {
                        var slotDetails = slots.ToString();
                        var slotDetailsCopy = string.Empty;
                        stInfo = string.Format("\nSlots Found at {0}", DateTime.Now.ToDetailString());
                        ConsoleMethods.PrintSuccess(stInfo);
                        logger.Info(stInfo);
                        ConsoleMethods.PrintInfo(slots.ToString());
                        Thread soundThread = new Thread(() => Sound.PlayBeep(4, 1500, 500));
                        soundThread.Start();

                        if (AppConfig.SendEmail)
                        {
                            slotDetailsCopy = slotDetails;

                            var templatePath = Path.GetFullPath("Templates/EmailTemplates/SlotsAvailable.html");
                            string mailBody = string.Empty;
                            if (File.Exists(templatePath))
                            {
                                slotDetailsCopy = slotDetailsCopy.Replace("\n", "<br />"); //For New Line Breaks
                                mailBody = EmailBody.CreateSlotsAvailableEmailBody(templatePath, UserDetails.FullName, UserDetails.UserPreference.PinCodeString, slotDetailsCopy, AppConfig.CoWIN_RegistrationURL, "Co-WIN: Self Registration");
                            }
                            else
                            {
                                stInfo = $"Template not found: {templatePath}, kindly check the original app package.";
                                logger.Info(stInfo);
                                ConsoleMethods.PrintError(stInfo);
                                mailBody = slotDetailsCopy;
                            }

                            var subject = AppConfig.Availablity_MailSubject + " for Pin Codes: " + UserDetails.UserPreference.PinCodeString;
                            INotifier iNotifier = new EmailNotifier(subject, UserDetails.EmailIdsString, UserDetails.FullName, isHTMLBody: true);
                            NotifierFactory notifier = new NotifierFactory(iNotifier);
                            Thread notifierThread = new Thread(() => notifier.Notify(mailBody));
                            notifierThread.Start();
                        }
                        else
                        {
                            stInfo = $"[WARNING] Email Settings are Turned OFF in appsettings.json";
                            ConsoleMethods.PrintError(stInfo);
                            logger.Info(stInfo);
                        }

                        if (AppConfig.SendTelegramNotification)
                        {
                            if (!string.IsNullOrWhiteSpace(UserDetails.TelegramChatID))
                            {
                                slotDetailsCopy = slotDetails;

                                var templatePath = Path.GetFullPath("Templates/MessageTemplates/SlotsAvailable.md");
                                string messageBody = string.Empty;
                                if (File.Exists(templatePath))
                                {
                                    messageBody = MessageBody.CreateSlotsAvailableMessageBody(templatePath, UserDetails.FullName, UserDetails.UserPreference.PinCodeString, slotDetailsCopy, AppConfig.CoWIN_RegistrationURL);
                                }
                                else
                                {
                                    stInfo = $"Template not found: {templatePath}, kindly check the original app package.";
                                    logger.Info(stInfo);
                                    ConsoleMethods.PrintError(stInfo);
                                    messageBody = slotDetailsCopy;
                                }

                                //To Escape Characters (for Telegram MarkDown)
                                messageBody = MessageBody.EscapeCharacters(messageBody);

                                INotifier iNotifier = new TelegramNotifier(UserDetails.TelegramChatID);
                                NotifierFactory notifier = new NotifierFactory(iNotifier);
                                Thread notifierThread = new Thread(() => notifier.Notify(messageBody));
                                notifierThread.Start();
                            }
                        }
                        else
                        {
                            stInfo = $"[WARNING] Telegram Notifications are Turned OFF in appsettings.json";
                            ConsoleMethods.PrintError(stInfo);
                            logger.Info(stInfo);
                        }
                        break;
                    }
                    else
                    {
                        stInfo = "No Slots Found for Pin Codes: " + UserDetails.UserPreference.PinCodeString + ". Last status checked: " + DateTime.Now.ToDetailString();
                        logger.Info(stInfo);
                        //Console.WriteLine(stInfo);
                        if (!errorOccured)
                            Thread.Sleep(TimeSpan.FromSeconds(UserDetails.UserPreference.PollingTime));
                    }
                }

                stInfo = "Status Call End for Pin Codes: " + UserDetails.UserPreference.PinCodeString;
                logger.Info(stInfo);
                Console.WriteLine("\n" + stInfo);

                return sessions;
            }
            catch (Exception ex)
            {
                stInfo = "Error in CheckVaccineAvailabilityStatus:\n" + ex;
                logger.Error(stInfo);
                ConsoleMethods.PrintError(stInfo);
                return sessions;
            }
        }

        public List<SessionProxy> CheckVaccineAvailabilityStatusByPin(string pinCode, int foundedCount, ref StringBuilder slots, ref int retryCount)
        {
            retryCount++;
            string stInfo = $"Status Call Started for Pin Code: {pinCode}, Retry Count: {retryCount}";
            logger.Info(stInfo);
            Console.WriteLine(stInfo);
            List<SessionProxy> sessions = new List<SessionProxy>();
            SessionProxy currSession = null;
            try
            {
                bool vaccineSlotFound = false;
                AvailabilityStatusAPIResponse response = APIs.CheckCalendarByPin(pinCode, Date, UserDetails.UserPreference.IsPrecautionDose, UserDetails.Phone);

                if (response == null)
                    return null;
                else if (response.SessionRelatedError)
                    return sessions;//Empty sessions, to retry again.

                int counter = 0;
                counter += foundedCount;
                bool isVaccineDose1 = UserDetails.UserPreference.Dose == 1;

                //var allSessions = response.centers.SelectMany(a => a.sessions).Where(x => (isVaccineDose1 ? x.available_capacity_dose1 > 0 : x.available_capacity_dose2 > 0) && x.min_age_limit <= UserDetails.UserPreference.AgeCriteria);
                ////Sort based on Nearest date and More Available Capacity
                //allSessions = allSessions.OrderBy(a => a.date).ThenByDescending(x => (isVaccineDose1 ? x.available_capacity_dose1 : x.available_capacity_dose2));

                foreach (var center in response.centers)
                {
                    if (!UserDetails.UserPreference.IncludePaidService && center.fee_type.ToLower() != "free")//fee_type = "Paid"
                    {
                        stInfo = string.Format("Fee Type for {0} center is {1}", center.name, center.fee_type);
                        logger.Info(stInfo);
                        continue;
                    }
                    foreach (var session in center.sessions)
                    {
                        if (session.available_capacity > 0 && session.min_age_limit <= UserDetails.UserPreference.AgeCriteria)
                        {
                            int chosenDoseAvailability = (isVaccineDose1 ? session.available_capacity_dose1 : session.available_capacity_dose2);
                            if (chosenDoseAvailability > 0)//For Dose 1 or 2 selection
                            {
                                stInfo = string.Format("Dose {0} is available in Center: {1}", (isVaccineDose1 ? 1 : 2), center.name);
                                logger.Info(stInfo);

                                bool isPreferenceVaccine = UserDetails.UserPreference.IsPreferenceVaccine(session.vaccine);
                                if (isPreferenceVaccine)
                                {
                                    vaccineSlotFound = true;
                                    counter++;
                                    var details = $"{counter}) Date: {session.date}, Name: {center.name}, Pin Code: {pinCode}, Vaccine: {session.vaccine}, Min Age: {session.min_age_limit}, Available Capacity Dose1: {session.available_capacity_dose1}, Available Capacity Dose2: {session.available_capacity_dose2}, Address: {center.address}";
                                    slots.AppendLine(details);
                                    logger.Info(details);

                                    stInfo = string.Format("Vaccine {0} is available in Center: {1}", session.vaccine, center.name);
                                    logger.Info(stInfo);

                                    if (currSession == null)
                                        currSession = new SessionProxy();
                                    currSession.SessionID = session.session_id;
                                    CultureInfo provider = CultureInfo.InvariantCulture;
                                    DateTime date = new DateTime();
                                    if (DateTime.TryParseExact(session.date, "dd-MM-yyyy", provider, DateTimeStyles.None, out date))
                                        currSession.Date = date;
                                    currSession.AvailableCapacity = chosenDoseAvailability;
                                    currSession.Vaccine = session.vaccine;
                                    currSession.CenterName = center.name;
                                    currSession.Address = center.address;
                                    currSession.Slots.AddRange(session.slots);

                                    sessions.Add(currSession);
                                    currSession = null;
                                }
                                else
                                {
                                    stInfo = string.Format("Other Vaccine {0} is available in Center: {1}", session.vaccine, center.name);
                                    ConsoleMethods.PrintProgress(stInfo);
                                    logger.Info(stInfo);
                                }
                            }
                            else
                            {
                                stInfo = string.Format("Other Dose {0} is available in Center: {1}", (isVaccineDose1 ? 2 : 1), center.name);
                                logger.Info(stInfo);
                                //var details = string.Format(counter + ") Date: {0}, Name: {1}, Pin Code: {2}, Vaccine: {3}, Min Age: {4}, Available Capacity Dose1: {5}, Available Capacity Dose2: {6}, Address: {7}", session.date, center.name, pinCode, session.vaccine, session.min_age_limit, session.available_capacity_dose1, session.available_capacity_dose2, center.address);
                                //otherDoseSlots.Append(details + "\n");
                            }
                        }
                    }
                }
                if (vaccineSlotFound)
                {
                    stInfo = string.Format("\nSlots Found for PinCode: {0} at {1}", pinCode, DateTime.Now.ToDetailString());
                    ConsoleMethods.PrintSuccess(stInfo);
                    logger.Info(stInfo);
                    Thread soundThread = new Thread(() => Sound.PlayBeep(4, 1500, 500));
                    soundThread.Start();
                }
                else
                {
                    stInfo = "No Slots Found for Pin Code: " + pinCode + ". Last status checked: " + DateTime.Now.ToDetailString();
                    logger.Info(stInfo);
                    ConsoleMethods.PrintProgress(stInfo);
                    //Thread.Sleep(TimeSpan.FromSeconds(UserDetails.UserPreference.PollingTime));
                }
                return sessions;
            }
            catch (Exception ex)
            {
                stInfo = "Error in CheckVaccineAvailabilityStatus:\n" + ex;
                logger.Error(stInfo);
                ConsoleMethods.PrintError(stInfo);
                return null;
            }
        }

        public void BookSlot(List<SessionProxy> sessionIds, int sessionPreference, int slotPreference)
        {
            bool slotBooked = false;
            slotBooked = BookSlotWithSessionPref(sessionIds, sessionPreference, slotPreference);

            if (!slotBooked && UserDetails.UserPreference.AutoPickCenter) //if auto center booking. Check if other center are available
            {
                int retryCount = 1;
                int sessionNumber = 1;
                while (!slotBooked && retryCount <= sessionIds.Count - 1)
                {
                    if (sessionNumber == sessionPreference)
                        sessionNumber++;
                    slotBooked = BookSlotWithSessionPref(sessionIds, sessionNumber, slotPreference);
                    retryCount++;
                    sessionNumber++;
                }
            }
        }

        public bool BookSlotWithSessionPref(List<SessionProxy> sessionIds, int sessionPreference, int slotPreference)
        {
            bool slotBooked = false;
            var session = sessionIds[sessionPreference - 1];
            slotBooked = BookSlotActual(session, slotPreference);

            #region Try to Book Other Slots
            if (AppConfig.TryToBookOtherSlots)
            {
                int retryCount = 1;
                int slot = 1;
                while (!slotBooked && retryCount < session.Slots.Count) //Check if other slots are available
                {
                    if (slot == slotPreference)
                        slot++;
                    slotBooked = BookSlotActual(session, slot);
                    retryCount++;
                    slot++;
                }
            }
            #endregion

            return slotBooked;
        }

        public bool BookSlotActual(SessionProxy session, int slotNumber)
        {
            bool slotBooked = false;
            string sessionId = string.Empty;
            string slot = string.Empty;
            DateTime date = default(DateTime);
            if (session != null)
            {
                sessionId = session.SessionID;
                slot = session.Slots[slotNumber - 1];
                date = session.Date;
            }
            string stInfo = string.Format("BookSlot Call Started for Date: {0}, Slot: {1}, Session Id: {2}.", date.ToString("dd-MM-yyyy"), slot, sessionId);
            logger.Info(stInfo);
            //Console.WriteLine(stInfo);

            SlotBookingResponse response = null;
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                response = APIs.BookSlot(UserDetails.UserPreference.BeneficiaryIds, sessionId, slot, UserDetails.UserPreference.Dose, date, UserDetails.Phone);

                if (response != null)
                {
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    var timeTakenToBook = ts.TotalSeconds;

                    slotBooked = true;

                    var bookingDetails = $"\t- Confirmation number: {response.appointment_confirmation_no}\n\t- Phone: {UserDetails.Phone}\n\t- Beneficiary IDs: {UserDetails.UserPreference.BeneficiaryIdsString}\n\t- Date: {(session.Date.IsDefault() ? "" : session.Date.ToString("dd-MM-yyyy"))}\n\t- Slot: {slot}\n\t- Dose: {UserDetails.UserPreference.Dose}\n\t- Vaccine: {session.Vaccine}\n\t- Center: {session.CenterName}\n\t- Address: {session.Address}";
                    var bookingDetailsCopy = string.Empty;

                    stInfo = "Vaccination slot has been booked Successfully!" + " - Confirmation number: " + response.appointment_confirmation_no;
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);

                    stInfo = string.Format("Time taken to book slot: {0} seconds.", timeTakenToBook);
                    Console.WriteLine(stInfo);
                    logger.Info(stInfo);

                    Thread soundThread = new Thread(() => Sound.PlayAsterisk(1));
                    soundThread.Start();

                    if (AppConfig.SendEmail)
                    {
                        bookingDetailsCopy = bookingDetails;

                        var templatePath = Path.GetFullPath("Templates/EmailTemplates/SlotBooked.html");
                        string mailBody = string.Empty;
                        if (File.Exists(templatePath))
                        {
                            bookingDetailsCopy = bookingDetailsCopy.Replace("\n", "<br />"); //For New Line Breaks
                            bookingDetailsCopy = bookingDetailsCopy.Replace("\t", "&#9;"); //For Tab Character
                            mailBody = EmailBody.CreateSlotsBookedEmailBody(templatePath, UserDetails.FullName, bookingDetailsCopy, AppConfig.CoWIN_RegistrationURL, "Co-WIN: Self Registration");
                        }
                        else
                        {
                            stInfo = $"Template not found: {templatePath}, kindly check the original app package.";
                            logger.Info(stInfo);
                            ConsoleMethods.PrintError(stInfo);
                            mailBody = bookingDetailsCopy;
                        }

                        INotifier iNotifier = new EmailNotifier(AppConfig.Booking_MailSubject, UserDetails.EmailIdsString, UserDetails.FullName, isHTMLBody: true);
                        NotifierFactory notifier = new NotifierFactory(iNotifier);
                        Thread notifierThread = new Thread(() => notifier.Notify(mailBody));
                        notifierThread.Start();
                    }
                    else
                    {
                        stInfo = $"[WARNING] Email Settings are Turned OFF in appsettings.json";
                        ConsoleMethods.PrintError(stInfo);
                        logger.Info(stInfo);
                    }

                    if (AppConfig.SendTelegramNotification)
                    {
                        if (!string.IsNullOrWhiteSpace(UserDetails.TelegramChatID))
                        {
                            bookingDetailsCopy = bookingDetails;

                            var templatePath = Path.GetFullPath("Templates/MessageTemplates/SlotBooked.md");
                            var messageBody = string.Empty;
                            if (File.Exists(templatePath))
                            {
                                messageBody = MessageBody.CreateSlotsBookedMessageBody(templatePath, UserDetails.FullName, bookingDetailsCopy, AppConfig.CoWIN_RegistrationURL);
                            }
                            else
                            {
                                stInfo = $"Template not found: {templatePath}, kindly check the original app package.";
                                logger.Info(stInfo);
                                ConsoleMethods.PrintError(stInfo);
                                messageBody = bookingDetailsCopy;
                            }

                            //To Escape Characters (for Telegram MarkDown)
                            messageBody = MessageBody.EscapeCharacters(messageBody);

                            INotifier iNotifier = new TelegramNotifier(UserDetails.TelegramChatID);
                            NotifierFactory notifier = new NotifierFactory(iNotifier);
                            Thread notifierThread = new Thread(() => notifier.Notify(messageBody));
                            notifierThread.Start();
                        }
                    }
                    else
                    {
                        stInfo = $"[WARNING] Telegram Notifications are Turned OFF in appsettings.json";
                        ConsoleMethods.PrintError(stInfo);
                        logger.Info(stInfo);
                    }
                }
                else
                {
                    slotBooked = false;
                    stInfo = "Unable to book Vaccination slot";
                    logger.Info(stInfo);
                    //Console.WriteLine(stInfo);
                }
                stopwatch.Stop();

                return slotBooked;
            }
            catch (Exception ex)
            {
                stInfo = "Error in BookSlot:\n" + ex;
                logger.Error(stInfo);
                ConsoleMethods.PrintError(stInfo);
                return slotBooked;
            }
        }
    }
}
