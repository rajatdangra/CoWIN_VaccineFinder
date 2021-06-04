using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NLog;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace VaccineFinder
{
    public class AppConfig
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static IConfiguration _configuration;
        public static void UpdateConfig()
        {
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();
            }
        }

        internal static string Phone => Convert.ToString(_configuration["UserDetails:Phone"]);
        internal static string PinCodes => Convert.ToString(_configuration["UserDetails:UserPreference:PinCodes"]);
        internal static string BeneficiaryIDs => Convert.ToString(_configuration["UserDetails:UserPreference:BeneficiaryIDs"]);
        internal static int SlotPreference => Convert.ToInt32(_configuration["UserDetails:UserPreference:SlotPreference"]);
        internal static bool AutoPickCenter => String.Equals(_configuration["UserDetails:UserPreference:AutoPickCenter"], "1");
        internal static bool IncludePaidService => String.Equals(_configuration["UserDetails:UserPreference:IncludePaidService"], "1");
        internal static bool VerifyBeneficiaries => String.Equals(_configuration["UserDetails:UserPreference:VerifyBeneficiaries"], "1");
        internal static string EmailIDs => Convert.ToString(_configuration["UserDetails:EmailIDs"]);
        internal static int MinAgeLimit => Convert.ToInt32(_configuration["UserDetails:UserPreference:MinAgeLimit"]);
        internal static int Dose => Convert.ToInt32(_configuration["UserDetails:UserPreference:Dose"]);
        internal static string Vaccine => Convert.ToString(_configuration["UserDetails:UserPreference:Vaccine"]);
        internal static string FirstName => Convert.ToString(_configuration["UserDetails:FirstName"]);
        internal static string LastName => Convert.ToString(_configuration["UserDetails:LastName"]);
        internal static string FullName => FirstName + (string.IsNullOrWhiteSpace(FirstName) ? "" : " ") + LastName;
        internal static int PollingTime => Convert.ToInt32(_configuration["UserDetails:UserPreference:PollingTime"]);

        internal static bool SaveUserDetails => String.Equals(_configuration["UserDetails:UserPreference:SaveUserDetails"], "1");
        #region Mail Settings
        internal static bool SendEmail => String.Equals(_configuration["MailSettings:SendEmail"], "1");
        internal static string Availablity_MailSubject => Convert.ToString(_configuration["MailSettings:Availablity_MailSubject"]);
        internal static string Booking_MailSubject => Convert.ToString(_configuration["MailSettings:Booking_MailSubject"]);
        #endregion

        #region Co-WIN APIs
        internal static string Cowin_BaseUrl => Convert.ToString(_configuration["CoWinAPI:BaseURL"]);
        internal static string CoWIN_RegistrationURL => Convert.ToString(_configuration["CoWinAPI:RegistrationURL"]);
        internal static string GenerateOTPUrl => Cowin_BaseUrl + Convert.ToString(_configuration["CoWinAPI:Authentication:GenerateOTPUrl"]);
        internal static string ConfirmOTPUrl => Cowin_BaseUrl + Convert.ToString(_configuration["CoWinAPI:Authentication:ConfirmOTPUrl"]);
        internal static string Secret => Convert.ToString(_configuration["CoWinAPI:Authentication:Secret"]);
        internal static string GetBeneficiariesUrl => Cowin_BaseUrl + Convert.ToString(_configuration["CoWinAPI:ProtectedAPI:GetBeneficiariesUrl"]);
        internal static string CalendarByPinUrl => Cowin_BaseUrl + Convert.ToString(_configuration["CoWinAPI:ProtectedAPI:CalendarByPinUrl"]);
        internal static string CalendarByDistrictUrl => Cowin_BaseUrl + Convert.ToString(_configuration["CoWinAPI:ProtectedAPI:CalendarByDistrictUrl"]);
        internal static string ScheduleAppointmentUrl => Cowin_BaseUrl + Convert.ToString(_configuration["CoWinAPI:ProtectedAPI:ScheduleAppointmentUrl"]);
        //internal static int OtpExpiryTimeLimit => Convert.ToInt32(_configuration["OtpExpiryTimeLimit"]);
        //internal static int TokenExpiryTimeLimit => Convert.ToInt32(_configuration["TokenExpiryTimeLimit"]);
        #endregion
        //internal static int Retry_Count => Convert.ToInt32(_configuration["Retry_Count"]);

        public static void UpdateConfig(UserDetails defaultDetails)
        {
            logger.Info("UpdateConfig started");
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");
                string json = File.ReadAllText(filePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                Dictionary<string, string> appSettings = new Dictionary<string, string>();
                appSettings.Add("UserDetails:Phone", defaultDetails.Phone);
                appSettings.Add("UserDetails:EmailIDs", defaultDetails.EmailIdsString);
                appSettings.Add("UserDetails:UserPreference:BeneficiaryIDs", defaultDetails.UserPreference.BeneficiaryIdsString);
                appSettings.Add("UserDetails:UserPreference:SlotPreference", Convert.ToString(defaultDetails.UserPreference.SlotPreference));
                appSettings.Add("UserDetails:UserPreference:PinCodes", defaultDetails.UserPreference.PinCodeString);
                appSettings.Add("UserDetails:UserPreference:MinAgeLimit", Convert.ToString(defaultDetails.UserPreference.AgeCriteria));
                appSettings.Add("UserDetails:UserPreference:Dose", Convert.ToString(defaultDetails.UserPreference.Dose));
                appSettings.Add("UserDetails:UserPreference:AutoPickCenter", Convert.ToString(Convert.ToInt32(defaultDetails.UserPreference.AutoPickCenter)));
                appSettings.Add("UserDetails:UserPreference:IncludePaidService", Convert.ToString(Convert.ToInt32(defaultDetails.UserPreference.IncludePaidService)));
                appSettings.Add("UserDetails:FirstName", defaultDetails.FirstName);
                appSettings.Add("UserDetails:LastName", defaultDetails.LastName);
                appSettings.Add("UserDetails:UserPreference:Vaccine", defaultDetails.UserPreference.Vaccine);
                appSettings.Add("UserDetails:UserPreference:PollingTime", Convert.ToString(defaultDetails.UserPreference.PollingTime));

                bool change = false;
                foreach (var kv in appSettings)
                {
                    var key = kv.Key;
                    var value = kv.Value;
                    bool isUpdated = SetValueRecursively(key, jsonObj, value);
                    if (isUpdated)
                    {
                        change = true;
                    }
                }
                if (change)
                {
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(filePath, output);
                    logger.Info("Updated appsettings.json file");
                }
                else
                {
                    logger.Info("No need to update appsettings.json file because appsetting are accurate in appsettings.json file");
                }
            }
            catch (Exception ex)
            {
                logger.Info("Unable to update appsettings.json file" + "\n" + ex);
            }
            finally
            {
                logger.Info("UpdateConfig end");
            }
        }

        private static bool SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            bool isSettingUpdated = false;
            // split the string at the first ':' character
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];

            if (jsonObj[currentSection] == null)
            {
                logger.Warn("While reading the appsettings.json, this line had no key/value attributes modify: " + sectionPathKey, value);
                isSettingUpdated = false;
            }
            else
            {
                if (remainingSections.Length > 1)
                {
                    // continue with the procress, moving down the tree
                    var nextSection = remainingSections[1];
                    isSettingUpdated = SetValueRecursively(nextSection, jsonObj[currentSection], value);
                }
                else
                {
                    // we've got to the end of the tree, set the value
                    if (jsonObj[currentSection].Value != value)
                    {
                        isSettingUpdated = true;
                        jsonObj[currentSection] = value;
                    }
                    else
                    {
                        isSettingUpdated = false;
                        //Same Value, no need to update
                    }
                }
            }
            return isSettingUpdated;
        }
    }
}
