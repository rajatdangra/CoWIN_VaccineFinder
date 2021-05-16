using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Co_WIN_Status
{
    class APIs
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static APIResponse CheckCalendarByPin(UserDetails details)
        {
            try
            {
                var postData = "?";
                postData += "pincode=" + details.PinCode;
                postData += "&date=" + DateTime.Now.ToString("dd-MM-yyyy");

                var request = (HttpWebRequest)WebRequest.Create(AppConfig.CoWIN_URL + postData);

                request.Method = "GET";
                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                logger.Info("Response from status API: " + responseString);

                APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseString);
                return apiResponse;
            }

            catch (Exception ex)
            {
                logger.Error("Error in CheckCalendarByPin:\n" + ex);
                return null;
            }
        }
    }
}
