using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private string AccessToken;

        public void StartFindingVaccine()
        {
            string stInfo = string.Empty;
            string inputMessage = string.Empty;

            #region Mobile Otp Validation
            var GenerateMobileOTPResponse = GenerateMobileOTP();
            if (GenerateMobileOTPResponse == null)
                return;

            inputMessage = "Please Enter OTP:";
            Console.WriteLine(inputMessage);
            var otp = Console.ReadLine();
            var ValidateMobileOTPResponse = ValidateMobileOTP(otp, GenerateMobileOTPResponse.txnId);
            while (ValidateMobileOTPResponse == null)
            {
                stInfo = "Invalid OTP. Please Retry.";
                logger.Error(stInfo + ": " + otp);
                //Console.WriteLine(stInfo);
                Console.WriteLine(inputMessage);
                otp = Console.ReadLine();
                ValidateMobileOTPResponse = ValidateMobileOTP(otp, GenerateMobileOTPResponse.txnId);
            }
            #endregion

            AccessToken = ValidateMobileOTPResponse.token;

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
                }
                else
                {
                    inputMessage = "Please enter your preference Center Number.";
                    Console.WriteLine(inputMessage);
                    var sessionNumberString = Console.ReadLine();
                    while (!int.TryParse(sessionNumberString, out sessionNumber) || sessionNumber > sessions.Count)
                    {
                        stInfo = "Invalid Input. Please Retry.";
                        logger.Error(stInfo + ": " + sessionNumberString);
                        Console.WriteLine(stInfo);
                        Console.WriteLine(inputMessage);
                        sessionNumberString = Console.ReadLine();
                    }
                }
                BookSlot(sessions, sessionNumber, UserDetails.UserPreference.SlotPreference);
            }
            #endregion
        }

        public GenerateMobileOTPResponse GenerateMobileOTP()
        {
            string stInfo = "GenerateMobileOTP Call Started for Phone: " + UserDetails.Phone;
            GenerateMobileOTPResponse response = null;
            try
            {
                logger.Info(stInfo);
                Console.WriteLine(stInfo);

                response = APIs.GenerateMobileOTP(UserDetails.Phone);

                if (response != null)
                {
                    stInfo = "OTP Sent Successfully to Phone: " + UserDetails.Phone;
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);
                }
                else
                {
                    stInfo = "Not able to send OTP";
                    logger.Info(stInfo);
                    //Console.WriteLine(stInfo);
                }
                return response;
            }
            catch (Exception ex)
            {
                stInfo = "Error in GenerateMobileOTP:\n" + ex;
                logger.Error(stInfo);
                Console.WriteLine(stInfo);
                return response;
            }
        }

        public ValidateMobileOTPResponse ValidateMobileOTP(string otp, string txnId)
        {
            string stInfo = "ValidateMobileOTP Call Started for otp: " + otp;
            logger.Info(stInfo);
            Console.WriteLine(stInfo);

            ValidateMobileOTPResponse response = null;
            try
            {
                var hasedOtp = Hash.ComputeSha256Hash(otp);
                response = APIs.ValidateMobileOTP(hasedOtp, txnId);

                if (response != null)
                {
                    stInfo = "OTP Verified. Bearer Token Generated Successfully.";
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);
                }
                else
                {
                    stInfo = "Unable to verifiy OTP: " + otp;
                    logger.Info(stInfo);
                    Console.WriteLine(stInfo);
                }
                return response;
            }
            catch (Exception ex)
            {
                stInfo = "Error in GenerateMobileOTP:\n" + ex;
                logger.Error(stInfo);
                Console.WriteLine(stInfo);
                return response;
            }
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
                response = APIs.GetBeneficiaries(AccessToken);

                if (response != null)
                {
                    stInfo = "Beneficiaries fetched Successfully!";
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);

                    while (!AreBeneficiariesVerified(response))
                    {
                        Console.WriteLine("\nBelow are the beneficiaries registered in your account:");
                        int counter = 0;
                        foreach (var ben in response.beneficiaries)
                        {
                            counter++;
                            var st = counter + ")" + " " + ben.Description;
                            Console.WriteLine(st);
                        }
                        Console.WriteLine("\nPlease enter comma separted beneficiary Ids");
                        var benInput = Console.ReadLine();
                        UserDetails.UserPreference.BeneficiaryIds = UserPreference.GetBeneficiaryIds(benInput);
                    }
                    areBeneficiariesVerified = true;

                    if (AppConfig.SaveUserDetails)
                    {
                        Console.WriteLine("Updating Default Settings");
                        AppConfig.UpdateConfig(UserDetails);
                        Console.WriteLine("Updated Default Settings");
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
                Console.WriteLine(stInfo);
                return false;
            }
        }

        public bool AreBeneficiariesVerified(GetBeneficiariesResponse response)
        {
            bool areBeneficiariesVerified = true;
            string stInfo = "Veryfying Beneficiaries";
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
                        stInfo = string.Format("Beneficiary Id {0} is valid, User Name: {1}", benId, benDetails.name);
                        Console.WriteLine(stInfo);
                        logger.Info(stInfo);
                    }
                    else
                    {
                        areBeneficiariesVerified = false;
                        stInfo = "Beneficiary Id is invalid: " + benId;
                        Console.WriteLine(stInfo);
                        logger.Info(stInfo);
                    }
                }
            }
            return areBeneficiariesVerified;
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
                while (!vaccineSlotFound && !errorOccured)
                {
                    stInfo = "Status Call End for Pin Codes: " + UserDetails.UserPreference.PinCodeString;
                    logger.Info(stInfo);
                    Console.WriteLine("\n" + stInfo);

                    int counter = 0;
                    StringBuilder slots = new StringBuilder();
                    foreach (var pinCode in UserDetails.UserPreference.PinCodes)
                    {
                        var sessionsByPin = CheckVaccineAvailabilityStatusByPin(pinCode, counter, ref slots);
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
                                sessionsByPin = sessionsByPin.OrderByDescending(a => a.availableCapacity).ToList();
                            }
                            sessions.AddRange(sessionsByPin);
                        }
                    }

                    if (vaccineSlotFound)
                    {
                        var slotDetails = string.Format("Hi{0},\n\nVaccine Slots are available for Pin Codes: {1}\n\n{2}\nPlease book your slots ASAP on {3}\n\nRegards,\nYour Vaccine Finder :)", (!string.IsNullOrWhiteSpace(UserDetails.FullName) ? " " + UserDetails.FullName : ""), UserDetails.UserPreference.PinCodeString, slots.ToString(), AppConfig.CoWIN_RegistrationURL);

                        stInfo = string.Format("\nSlots Found at {0}", DateTime.Now.ToDetailString());
                        Console.WriteLine(stInfo);
                        logger.Info(stInfo);
                        Console.WriteLine(slots.ToString());
                        Thread soundThread = new Thread(() => Sound.PlayBeep(4, 1500, 500));
                        soundThread.Start();

                        if (AppConfig.SendEmail)
                        {
                            var subject = AppConfig.Availablity_MailSubject + " for Pin Codes: " + UserDetails.UserPreference.PinCodeString;
                            Thread mailThread = new Thread(() => Email.SendEmail(slotDetails, subject));
                            mailThread.Start();
                        }
                        break;
                    }
                    else
                    {
                        stInfo = "No Slots Found for Pin Codes: " + UserDetails.UserPreference.PinCodeString + ". Last status checked: " + DateTime.Now.ToDetailString();
                        logger.Info(stInfo);
                        Console.WriteLine(stInfo);
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
                Console.WriteLine(stInfo);
                return sessions;
            }
        }

        public List<SessionProxy> CheckVaccineAvailabilityStatusByPin(string pinCode, int foundedCount, ref StringBuilder slots)
        {
            string stInfo = "Status Call Started for Pin Code: " + pinCode;
            logger.Info(stInfo);
            Console.WriteLine(stInfo);
            List<SessionProxy> sessions = new List<SessionProxy>();
            SessionProxy currSession = null;
            try
            {
                bool vaccineSlotFound = false;
                AvailabilityStatusAPIResponse response = APIs.CheckCalendarByPin(pinCode, Date, AccessToken);

                if (response == null)
                    return null;

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
                                vaccineSlotFound = true;
                                counter++;
                                var details = string.Format(counter + ") Date: {0}, Name: {1}, Pin Code: {2}, Vaccine: {3}, Min Age: {4}, Available Capacity Dose1: {5}, Available Capacity Dose2: {6}, Address: {7}", session.date, center.name, pinCode, session.vaccine, session.min_age_limit, session.available_capacity_dose1, session.available_capacity_dose2, center.address);
                                slots.Append(details + "\n");

                                stInfo = string.Format("Dose {0} is available", (isVaccineDose1 ? 1 : 2));
                                logger.Info(stInfo);

                                if (currSession == null)
                                    currSession = new SessionProxy();
                                currSession.session_id = session.session_id;
                                CultureInfo provider = CultureInfo.InvariantCulture;
                                DateTime date = new DateTime();
                                if (DateTime.TryParseExact(session.date, "dd-MM-yyyy", provider, DateTimeStyles.None, out date))
                                    currSession.date = date;
                                currSession.availableCapacity = chosenDoseAvailability;
                                currSession.slots.AddRange(session.slots);

                                sessions.Add(currSession);
                                currSession = null;
                            }
                            else
                            {
                                stInfo = string.Format("Other Dose {0} is available", (isVaccineDose1 ? 2 : 1));
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
                    Console.WriteLine(stInfo);
                    logger.Info(stInfo);
                    Thread soundThread = new Thread(() => Sound.PlayBeep(4, 1500, 500));
                    soundThread.Start();
                }
                else
                {
                    stInfo = "No Slots Found for Pin Code: " + pinCode + ". Last status checked: " + DateTime.Now.ToDetailString();
                    logger.Info(stInfo);
                    Console.WriteLine(stInfo);
                    //Thread.Sleep(TimeSpan.FromSeconds(UserDetails.UserPreference.PollingTime));
                }
                return sessions;
            }
            catch (Exception ex)
            {
                stInfo = "Error in CheckVaccineAvailabilityStatus:\n" + ex;
                logger.Error(stInfo);
                Console.WriteLine(stInfo);
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
            int retryCount = 1;
            int slot = 1;
            while (!slotBooked && retryCount <= 3) //Check if other slots are available
            {
                if (slot == slotPreference)
                    slot++;
                slotBooked = BookSlotActual(session, slot);
                retryCount++;
                slot++;
            }
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
                sessionId = session.session_id;
                slot = session.slots[slotNumber - 1];
                date = session.date;
            }
            string stInfo = string.Format("BookSlot Call Started for Date: {0}, Slot: {1}, Session Id: {2}.", date.ToString("dd-MM-yyyy"), slot, sessionId);
            logger.Info(stInfo);
            //Console.WriteLine(stInfo);

            SlotBookingResponse response = null;
            try
            {
                response = APIs.BookSlot(AccessToken, UserDetails.UserPreference.BeneficiaryIds, sessionId, slot, UserDetails.UserPreference.Dose, date);

                if (response != null)
                {
                    slotBooked = true;
                    var bookingDetails = string.Format("Hi{0},\n\nYour Vaccine Slot has been booked Successfully!\n\nBelow are the details:\n\tConfirmation number: {1}\n\tBeneficiary Ids: {2}\n\tDate: {3}\n\tSlot: {4}\n\nRegards,\nYour Vaccine Finder :)", (!string.IsNullOrWhiteSpace(UserDetails.FullName) ? " " + UserDetails.FullName : ""), response.appointment_confirmation_no, UserDetails.UserPreference.BeneficiaryIdsString, (session.date.IsDefault() ? "" : session.date.ToString("dd-MM-yyyy")), slot);

                    stInfo = "Vaccination slot has been booked Successfully!" + " - Confirmation number: " + response.appointment_confirmation_no;
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);

                    Thread soundThread = new Thread(() => Sound.PlayAsterisk(1));
                    soundThread.Start();

                    if (AppConfig.SendEmail)
                    {
                        Thread mailThread = new Thread(() => Email.SendEmail(bookingDetails, AppConfig.Booking_MailSubject));
                        mailThread.Start();
                    }
                }
                else
                {
                    slotBooked = false;
                    stInfo = "Unable to book Vaccination slot";
                    logger.Info(stInfo);
                    //Console.WriteLine(stInfo);
                }
                return slotBooked;
            }
            catch (Exception ex)
            {
                stInfo = "Error in BookSlot:\n" + ex;
                logger.Error(stInfo);
                Console.WriteLine(stInfo);
                return slotBooked;
            }
        }
    }
}
