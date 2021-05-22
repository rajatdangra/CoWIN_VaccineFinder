using NLog;
using System;
using System.Collections.Generic;
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
                if (UserDetails.UserPreference.AutoBookCenter)
                {
                    //Sort based on More Available Capacity
                    sessions = sessions.OrderByDescending(a => a.availableCapacity).ToList();
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
            string stInfo = "Status Call Started for Pin Code: " + UserDetails.UserPreference.PinCode;
            logger.Info(stInfo);
            Console.WriteLine("\n" + stInfo);
            List<SessionProxy> sessions = new List<SessionProxy>();
            SessionProxy currSession = null;
            try
            {
                bool vaccineSlotFound = false;
                while (!vaccineSlotFound)
                {
                    StringBuilder slots = new StringBuilder();
                    AvailabilityStatusAPIResponse response = APIs.CheckCalendarByPin(UserDetails.UserPreference.PinCode, Date, AccessToken);

                    if (response == null)
                        break;

                    int counter = 0;
                    bool isVaccineDose1 = UserDetails.UserPreference.Dose == 1;

                    //var allSessions = response.centers.SelectMany(a => a.sessions).Where(x => (isVaccineDose1 ? x.available_capacity_dose1 > 0 : x.available_capacity_dose2 > 0) && x.min_age_limit <= UserDetails.UserPreference.AgeCriteria);
                    ////Sort based on Nearest date and More Available Capacity
                    //allSessions = allSessions.OrderBy(a => a.date).ThenByDescending(x => (isVaccineDose1 ? x.available_capacity_dose1 : x.available_capacity_dose2));

                    foreach (var center in response.centers)
                    {
                        foreach (var session in center.sessions)
                        {
                            if (session.available_capacity > 0 && session.min_age_limit <= UserDetails.UserPreference.AgeCriteria)
                            {
                                vaccineSlotFound = true;
                                counter++;
                                var details = string.Format(counter + ") Date: {0}, Name: {1}, Centre ID: {2}, Min Age: {3}, Available Capacity: {4}, Available Capacity Dose1: {5}, Available Capacity Dose2: {6}, Address: {7}, Session Id: {8}", session.date, center.name, center.center_id, session.min_age_limit, session.available_capacity, session.available_capacity_dose1, session.available_capacity_dose2, center.address, session.session_id);
                                slots.Append(details + "\n");

                                int chosenDoseAvailability = (isVaccineDose1 ? session.available_capacity_dose1 : session.available_capacity_dose2);
                                if (chosenDoseAvailability > 0)//For Dose 1 or 2 selection
                                {
                                    if (currSession == null)
                                        currSession = new SessionProxy();
                                    currSession.session_id = session.session_id;
                                    currSession.availableCapacity = chosenDoseAvailability;
                                    currSession.slots.AddRange(session.slots);

                                    sessions.Add(currSession);
                                    currSession = null;
                                }
                            }
                        }
                    }
                    if (vaccineSlotFound)
                    {
                        var slotDetails = "Hi" + (!string.IsNullOrWhiteSpace(UserDetails.FullName) ? " " + UserDetails.FullName : "") + ",\n\nVaccine Slots are available for Pin Code: " + UserDetails.UserPreference.PinCode + "\n\n" + slots.ToString() + "\nPlease book your slots ASAP on " + AppConfig.CoWIN_RegistrationURL + "\n\nRegards,\nYour Vaccine Finder :)";

                        stInfo = string.Format("\nSlots Found at {0}", DateTime.Now.ToDetailString());
                        Console.WriteLine(stInfo);
                        logger.Info(stInfo);
                        Console.WriteLine(slots.ToString());
                        Thread soundThread = new Thread(() => Sound.PlayBeep(4, 1500, 500));
                        soundThread.Start();

                        if (AppConfig.SendEmail)
                        {
                            var subject = AppConfig.Availablity_MailSubject + " for Pin Code: " + UserDetails.UserPreference.PinCode;
                            Thread mailThread = new Thread(() => Email.SendEmail(slotDetails, subject));
                            mailThread.Start();
                        }
                        break;
                    }
                    else
                    {
                        stInfo = "No Slots Found for Pin Code: " + UserDetails.UserPreference.PinCode + ". Last status checked: " + DateTime.Now.ToDetailString();
                        logger.Info(stInfo);
                        Console.WriteLine(stInfo);
                        Thread.Sleep(TimeSpan.FromSeconds(UserDetails.UserPreference.PollingTime));
                    }
                }
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

        public void BookSlot(List<SessionProxy> sessionIds, int sessionPreference, int slotPreference)
        {
            bool slotBooked = false;
            slotBooked = BookSlotWithSessionPref(sessionIds, sessionPreference, slotPreference);

            if (!slotBooked && AppConfig.AutoBookCenter) //if auto center booking. Check if other center are available
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
            string stInfo = "BookSlot Call Started for phone: " + UserDetails.Phone;
            logger.Info(stInfo);
            //Console.WriteLine(stInfo);

            SlotBookingResponse response = null;
            try
            {
                string sessionId = string.Empty;
                string slot = string.Empty;
                if (session != null)
                {
                    sessionId = session.session_id;
                    slot = session.slots[slotNumber - 1];
                }
                response = APIs.BookSlot(AccessToken, UserDetails.UserPreference.BeneficiaryIds, sessionId, slot, UserDetails.UserPreference.Dose);

                if (response != null)
                {
                    slotBooked = true;
                    var bookingDetails = "Hi" + (!string.IsNullOrWhiteSpace(UserDetails.FullName) ? " " + UserDetails.FullName : "") + ",\n\nYour Vaccine Slot has been booked Successfully!" + "\nBelow are the details:\n" + "Confirmation number: " + response.appointment_confirmation_no
                        + "\nBeneficiary Ids: " + UserDetails.UserPreference.BeneficiaryIds + "\nSlot: " + slot + "\n\nRegards,\nYour Vaccine Finder :)";

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
