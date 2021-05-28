using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NLog;

namespace VaccineFinder
{
    public class AppConfig
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        internal static string Phone => Convert.ToString(ConfigurationManager.AppSettings["Phone"]);
        internal static string PinCodes => Convert.ToString(ConfigurationManager.AppSettings["PinCodes"]);
        internal static string BeneficiaryIDs => Convert.ToString(ConfigurationManager.AppSettings["BeneficiaryIDs"]);
        internal static int SlotPreference => Convert.ToInt32(ConfigurationManager.AppSettings["SlotPreference"]);
        internal static bool AutoPickCenter => String.Equals(ConfigurationManager.AppSettings["AutoPickCenter"], "1");
        internal static bool IncludePaidService => String.Equals(ConfigurationManager.AppSettings["IncludePaidService"], "1");
        internal static bool VerifyBeneficiaries => String.Equals(ConfigurationManager.AppSettings["VerifyBeneficiaries"], "1");
        internal static string EmailIDs => Convert.ToString(ConfigurationManager.AppSettings["EmailIDs"]);
        internal static int MinAgeLimit => Convert.ToInt32(ConfigurationManager.AppSettings["MinAgeLimit"]);
        internal static int Dose => Convert.ToInt32(ConfigurationManager.AppSettings["Dose"]);
        internal static string Vaccine => Convert.ToString(ConfigurationManager.AppSettings["Vaccine"]);
        internal static string FirstName => Convert.ToString(ConfigurationManager.AppSettings["FirstName"]);
        internal static string LastName => Convert.ToString(ConfigurationManager.AppSettings["LastName"]);
        internal static string FullName => FirstName + " " + LastName;
        internal static int PollingTime => Convert.ToInt32(ConfigurationManager.AppSettings["PollingTime"]);

        internal static bool SaveUserDetails => String.Equals(ConfigurationManager.AppSettings["SaveUserDetails"], "1");
        internal static bool SendEmail => String.Equals(ConfigurationManager.AppSettings["SendEmail"], "1");

        #region Co-WIN APIs
        internal static string Secret => Convert.ToString(ConfigurationManager.AppSettings["Secret"]);
        internal static string Cowin_BaseUrl => Convert.ToString(ConfigurationManager.AppSettings["Cowin_BaseUrl"]);
        internal static string GenerateOTPUrl => Cowin_BaseUrl + Convert.ToString(ConfigurationManager.AppSettings["GenerateOTPUrl"]);
        internal static string ConfirmOTPUrl => Cowin_BaseUrl + Convert.ToString(ConfigurationManager.AppSettings["ConfirmOTPUrl"]);
        internal static string GetBeneficiariesUrl => Cowin_BaseUrl + Convert.ToString(ConfigurationManager.AppSettings["GetBeneficiariesUrl"]);
        internal static string CalendarByPinUrl => Cowin_BaseUrl + Convert.ToString(ConfigurationManager.AppSettings["CalendarByPinUrl"]);
        internal static string CalendarByDistrictUrl => Cowin_BaseUrl + Convert.ToString(ConfigurationManager.AppSettings["CalendarByDistrictUrl"]);
        internal static string ScheduleAppointmentUrl => Cowin_BaseUrl + Convert.ToString(ConfigurationManager.AppSettings["ScheduleAppointmentUrl"]);
        internal static string CoWIN_RegistrationURL => Convert.ToString(ConfigurationManager.AppSettings["CoWIN_RegistrationURL"]);
        internal static string Availablity_MailSubject => Convert.ToString(ConfigurationManager.AppSettings["Availablity_MailSubject"]);
        internal static string Booking_MailSubject => Convert.ToString(ConfigurationManager.AppSettings["Booking_MailSubject"]);
        internal static int OtpExpiryTimeLimit => Convert.ToInt32(ConfigurationManager.AppSettings["OtpExpiryTimeLimit"]);
        internal static int TokenExpiryTimeLimit => Convert.ToInt32(ConfigurationManager.AppSettings["TokenExpiryTimeLimit"]);
        #endregion
        //internal static int Retry_Count => Convert.ToInt32(ConfigurationManager.AppSettings["Retry_Count"]);

        public static void UpdateConfig(UserDetails defaultDetails)
        {
            logger.Info("UpdateConfig started");
            try
            {
                System.Configuration.Configuration configFile = null;
                //if (System.Web.HttpContext.Current != null)
                //{
                //    configFile =
                //        System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                //}
                //else
                {
                    configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                }
                var settings = configFile.AppSettings.Settings;

                Dictionary<string, string> appSettings = new Dictionary<string, string>();
                appSettings.Add("Phone", defaultDetails.Phone);
                appSettings.Add("EmailIDs", defaultDetails.EmailIdsString);
                appSettings.Add("BeneficiaryIDs", defaultDetails.UserPreference.BeneficiaryIdsString);
                appSettings.Add("SlotPreference", Convert.ToString(defaultDetails.UserPreference.SlotPreference));
                appSettings.Add("PinCodes", defaultDetails.UserPreference.PinCodeString);
                appSettings.Add("MinAgeLimit", Convert.ToString(defaultDetails.UserPreference.AgeCriteria));
                appSettings.Add("Dose", Convert.ToString(defaultDetails.UserPreference.Dose));
                appSettings.Add("AutoPickCenter", Convert.ToString(Convert.ToInt32(defaultDetails.UserPreference.AutoPickCenter)));
                appSettings.Add("IncludePaidService", Convert.ToString(Convert.ToInt32(defaultDetails.UserPreference.IncludePaidService)));
                appSettings.Add("FirstName", defaultDetails.FirstName);
                appSettings.Add("LastName", defaultDetails.LastName);
                appSettings.Add("Vaccine", defaultDetails.UserPreference.Vaccine);
                appSettings.Add("PollingTime", Convert.ToString(defaultDetails.UserPreference.PollingTime));

                bool change = false;
                foreach (var kv in appSettings)
                {
                    var key = kv.Key;
                    var value = kv.Value;
                    if (settings[key] == null)
                    {
                        logger.Warn("While reading the App.config, this line had no key/value attributes modify: " + key);
                        //settings.Add(key, value);
                    }
                    else if (settings[key].Value != value)
                    {
                        change = true;
                        settings[key].Value = value;
                    }
                }
                if (change)
                {
                    configFile.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                    logger.Info("Updated App.config file");
                }
                else
                {
                    logger.Info("No need to update App.config file because appsetting are accurate in App.config file");
                }
            }
            catch (Exception ex)
            {
                logger.Info("Unable to update App.config file" + "\n" + ex);
            }
            finally
            {
                logger.Info("UpdateConfig end");
            }
        }
    }
}
