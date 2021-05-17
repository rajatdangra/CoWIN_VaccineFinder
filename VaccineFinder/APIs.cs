using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder
{
    class APIs
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static APIResponse CheckCalendarByPin(UserDetails details, DateTime date)
        {
            try
            {
                var postData = "?";
                postData += "pincode=" + details.PinCode;
                postData += "&date=" + date.ToString("dd-MM-yyyy");

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
                var stInfo = "Error in CheckCalendarByPin:\n" + ex;
                Console.WriteLine("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return null;
            }
        }
    }
}
