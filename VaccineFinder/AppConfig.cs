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

        internal static string EmailIDs => Convert.ToString(ConfigurationManager.AppSettings["EmailIDs"]);
        internal static string PinCode => Convert.ToString(ConfigurationManager.AppSettings["PinCode"]);
        internal static int MinAgeLimit => Convert.ToInt32(ConfigurationManager.AppSettings["MinAgeLimit"]);
        internal static string Phone => Convert.ToString(ConfigurationManager.AppSettings["Phone"]);
        internal static string FirstName => Convert.ToString(ConfigurationManager.AppSettings["FirstName"]);
        internal static string LastName => Convert.ToString(ConfigurationManager.AppSettings["LastName"]);
        internal static string FullName => FirstName + " " + LastName;
        internal static bool SaveUserDetails => String.Equals(ConfigurationManager.AppSettings["SaveUserDetails"], "1");
        internal static int PollingTime => Convert.ToInt32(ConfigurationManager.AppSettings["PollingTime"]);
        internal static string CoWIN_URL => Convert.ToString(ConfigurationManager.AppSettings["CoWIN_URL"]);
        internal static string CoWIN_BookingURL => Convert.ToString(ConfigurationManager.AppSettings["CoWIN_BookingURL"]);
        internal static string Mail_Subject => Convert.ToString(ConfigurationManager.AppSettings["Mail_Subject"]);
        //internal static int Retry_Count => Convert.ToInt32(ConfigurationManager.AppSettings["Retry_Count"]);

        public static void UpdateConfig(UserDetails defaultDetails, int PollingTime)
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
                appSettings.Add("EmailIDs", defaultDetails.EmailIdsString);
                appSettings.Add("PinCode", defaultDetails.PinCode);
                appSettings.Add("Phone", defaultDetails.Phone);
                appSettings.Add("MinAgeLimit", Convert.ToString(defaultDetails.AgeCriteria));
                appSettings.Add("FirstName", defaultDetails.FirstName);
                appSettings.Add("LastName", defaultDetails.LastName);
                appSettings.Add("PollingTime", Convert.ToString(PollingTime));

                bool change = false;
                foreach (var kv in appSettings)
                {
                    var key = kv.Key;
                    var value = kv.Value;
                    if (settings[key] == null)
                    {
                        logger.Warn("While reading the app.config, this line had no key/value attributes modify: " + key);
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
                    logger.Info("Updated app.config file");
                }
                else
                {
                    logger.Info("No need to update app.config file because appsetting are accurate in app.config file");
                }
            }
            catch (Exception ex)
            {
                logger.Info("Unable to update app.config file" + "\n" + ex);
            }
            finally
            {
                logger.Info("UpdateConfig end");
            }
        }
    }
}
