using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VaccineFinder.Models;

namespace VaccineFinder
{
    class APIs
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool isIPThrottled = false;

        public static GenerateMobileOTPResponse GenerateMobileOTP(string phone)
        {
            string stInfo = string.Empty;
            logger.Info("GenerateMobileOTP API call started for Phone: " + phone);
            GenerateMobileOTPResponse apiResponse = null;
            try
            {
                #region RestClient
                var client = new RestClient(AppConfig.GenerateOTPUrl);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                //var jsonBody = "{\r\n    \"mobile\": \"" + phone + "\",\r\n    \"secret\": \"" + AppConfig.Secret + "\"\r\n}";
                var requestObj = new GenerateMobileOTPRequest() { secret = AppConfig.Secret, mobile = phone };
                var serRequestObj = JsonConvert.SerializeObject(requestObj);
                request.AddParameter("application/json", serRequestObj /*jsonBody*/, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                var responseString = response.Content;
                //Console.WriteLine(responseString);
                logger.Info("Response from GenerateMobileOTP API: " + responseString);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse = JsonConvert.DeserializeObject<GenerateMobileOTPResponse>(responseString);

                    stInfo = string.Format("OTP Sent Successfully to Phone: {0} at {1}", phone, DateTime.Now.ToDetailString());
                    logger.Info(stInfo);
                    ConsoleMethods.PrintSuccess(stInfo);
                }
                else
                {
                    stInfo = "Could not generate Otp";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError("Please check your Internet Connection\n" + stInfo);
                }
                #endregion

                #region using WebRequest
                //// Create a request using a URL that can receive a post.
                //WebRequest request = WebRequest.Create(AppConfig.GenerateOTPUrl);
                //// Set the Method property of the request to POST.
                //request.Method = "POST";

                //// Create POST data and convert it to a byte array.
                //var req = new GenerateMobileOTPRequest() { secret = AppConfig.Secret, mobile = phone };
                //string postData = JsonConvert.SerializeObject(req);
                //byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                //// Set the ContentType property of the WebRequest.
                //request.ContentType = "application/Json";
                //// Set the ContentLength property of the WebRequest.
                //request.ContentLength = byteArray.Length;

                ////using (var stream = request.GetRequestStream())
                ////{
                ////    stream.Write(byteArray, 0, byteArray.Length);
                ////}

                //// Get the request stream.
                //Stream dataStream = request.GetRequestStream();
                //// Write the data to the request stream.
                //dataStream.Write(byteArray, 0, byteArray.Length);
                //// Close the Stream object.
                //dataStream.Close();

                //// Get the response.
                //WebResponse response = request.GetResponse();
                //// Display the status.
                //Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                //// Get the stream containing content returned by the server.
                //// The using block ensures the stream is automatically closed.
                //using (dataStream = response.GetResponseStream())
                //{
                //    // Open the stream using a StreamReader for easy access.
                //    StreamReader reader = new StreamReader(dataStream);
                //    // Read the content.
                //    string responseFromServer = reader.ReadToEnd();
                //    // Display the content.
                //    Console.WriteLine(responseFromServer);
                //    logger.Info("Response from GenerateMobileOTP API: " + responseFromServer);
                //    apiResponse = JsonConvert.DeserializeObject<GenerateMobileOTPResponse>(responseFromServer);
                //}

                //// Close the response.
                //response.Close();
                #endregion

                return apiResponse;
            }
            catch (Exception ex)
            {
                stInfo = "Error in GenerateMobileOTP:\n" + ex;
                Console.WriteLine("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return apiResponse;
            }
            finally
            {
                logger.Info("GenerateMobileOTP API call end for Phone: " + phone);
            }
        }

        public static ValidateMobileOTPResponse ValidateMobileOTP(string otp, string txnId)
        {
            string stInfo = string.Empty;
            logger.Info("ValidateMobileOTP API call started.");
            try
            {
                ValidateMobileOTPResponse apiResponse = null;
                var client = new RestClient(AppConfig.ConfirmOTPUrl);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                var requestObj = new ValidateMobileOTPRequest() { otp = otp, txnId = txnId };
                var serRequestObj = JsonConvert.SerializeObject(requestObj);
                request.AddParameter("application/json", serRequestObj, ParameterType.RequestBody);
                request.AddHeader("Origin", AppConfig.CoWIN_RegistrationURL);
                IRestResponse response = client.Execute(request);
                var responseString = response.Content;
                //Console.WriteLine(responseString);
                logger.Info("Response from ValidateMobileOTP API: " + responseString);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse = JsonConvert.DeserializeObject<ValidateMobileOTPResponse>(responseString);

                    stInfo = "OTP Verified. Bearer Token Generated Successfully at " + DateTime.Now.ToDetailString();
                    logger.Info(stInfo);
                    //ConsoleMethods.PrintSuccess(stInfo);
                }
                else
                {
                    stInfo = "Invalid OTP.";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError(stInfo);
                }
                return apiResponse;
            }
            catch (Exception ex)
            {
                stInfo = "Error in ValidateMobileOTP:\n" + ex;
                ConsoleMethods.PrintError("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return null;
            }
            finally
            {
                logger.Info("ValidateMobileOTP API call end.");
            }
        }

        public static GetBeneficiariesResponse GetBeneficiaries(string phone)
        {
            string stInfo = string.Empty;
            logger.Info("GetBeneficiaries API call started.");
            try
            {
                GetBeneficiariesResponse apiResponse = null;
                var client = new RestClient(AppConfig.GetBeneficiariesUrl);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + OTPAuthenticator.BEARER_TOKEN);
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36";
                IRestResponse response = client.Execute(request);
                var responseString = response.Content;
                //Console.WriteLine(responseString);
                logger.Info("Response from GetBeneficiaries API: " + responseString);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse = JsonConvert.DeserializeObject<GetBeneficiariesResponse>(responseString);
                    stInfo = "Beneficiaries fetched Successfully!";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintSuccess(stInfo);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    stInfo = $"[WARNING] Session Expired : Regenerating Auth Token.";
                    logger.Warn(stInfo);
                    ConsoleMethods.PrintProgress(stInfo);
                    new OTPAuthenticator().ValidateUser(phone);
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    stInfo = $"[FATAL] Response From Server: {responseString}\nToo many hits from your IP address from the same Session, hence request has been blocked.";
                    logger.Warn(stInfo);
                    ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkYellow);
                    if (AppConfig.IsThrottlingToBeUsed)
                    {
                        isIPThrottled = false;
                        new Thread(new ThreadStart(IPThrottledNotifier)).Start();
                        stInfo = $"[FATAL] You can try following options:\n1.(By Default) Wait for {AppConfig.ThrottlingRefreshTime} seconds, the Application will Automatically resume working.\n2.Switch to a different network which will change your current IP address.\n3.Close the application and try again with IP Throttling Disabled.";
                        logger.Warn(stInfo);
                        ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkYellow);
                        Thread.Sleep(AppConfig.ThrottlingRefreshTime * 1000);
                        isIPThrottled = true;
                    }
                    else
                    {
                        stInfo = "IP Throttling is Disabled in 'appsettings.json'. Regenerating Auth Token.";
                        logger.Warn(stInfo);
                        ConsoleMethods.PrintProgress(stInfo);
                        new OTPAuthenticator().ValidateUser(phone, forceGenerateToken: true);
                    }
                }
                else
                {
                    stInfo = "Unable to Fetch Beneficiaries.\nResponse Code: " + response.StatusCode + ", Response: " + responseString;
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError(stInfo);
                }
                return apiResponse;
            }
            catch (Exception ex)
            {
                stInfo = "Error in GetBeneficiaries:\n" + ex;
                ConsoleMethods.PrintError("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return null;
            }
            finally
            {
                logger.Info("GetBeneficiaries API call end.");
            }
        }

        public static AvailabilityStatusAPIResponse CheckCalendarByPin(string pinCode, DateTime date, string phone)
        {
            logger.Info("CheckCalendarByPin API call started.");
            AvailabilityStatusAPIResponse apiResponse = null;
            string stInfo = string.Empty;
            try
            {
                #region Generate Random String to ignore Caching
                string randomString = StringMethods.GenerateRandomString();
                #endregion

                var postData = "?";
                postData += "pincode=" + pinCode;
                postData += "&date=" + date.ToString("dd-MM-yyyy");
                postData += "&random=" + randomString;

                //var request = (HttpWebRequest)WebRequest.Create(AppConfig.CalendarByPinUrl + postData);
                //request.PreAuthenticate = true;
                //request.Headers.Add("Authorization", "Bearer " + OTPAuthenticator.BEARER_TOKEN);
                //request.Method = "GET";
                //var response = (HttpWebResponse)request.GetResponse();

                //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //logger.Info("Response from status API: " + responseString);

                var client = new RestClient(AppConfig.CalendarByPinUrl + postData);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + OTPAuthenticator.BEARER_TOKEN);
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36";

                IRestResponse response = client.Execute(request);
                var responseString = response.Content;
                //Console.WriteLine(responseString);
                logger.Info("Response from CheckCalendarByPin API: " + responseString);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse = JsonConvert.DeserializeObject<AvailabilityStatusAPIResponse>(responseString);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    apiResponse = new AvailabilityStatusAPIResponse();
                    apiResponse.SessionRelatedError = true;

                    stInfo = $"[WARNING] Session Expired : Regenerating Auth Token.";
                    logger.Warn(stInfo);
                    ConsoleMethods.PrintProgress(stInfo);
                    new OTPAuthenticator().ValidateUser(phone);
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    apiResponse = new AvailabilityStatusAPIResponse();
                    apiResponse.SessionRelatedError = true;

                    stInfo = $"[FATAL] Response From Server: {responseString}\nToo many hits from your IP address from the same Session, hence request has been blocked.";
                    logger.Warn(stInfo);
                    ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkYellow);
                    if (AppConfig.IsThrottlingToBeUsed)
                    {
                        isIPThrottled = false;
                        new Thread(new ThreadStart(IPThrottledNotifier)).Start();
                        stInfo = $"[FATAL] You can try following options:\n1.(By Default) Wait for {AppConfig.ThrottlingRefreshTime} seconds, the Application will Automatically resume working.\n2.Switch to a different network which will change your current IP address.\n3.Close the application and try again with IP Throttling Disabled.";
                        logger.Warn(stInfo);
                        ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkYellow);
                        Thread.Sleep(AppConfig.ThrottlingRefreshTime * 1000);
                        isIPThrottled = true;
                    }
                    else
                    {
                        stInfo = "IP Throttling is Disabled in 'appsettings.json'. Regenerating Auth Token.";
                        logger.Warn(stInfo);
                        ConsoleMethods.PrintProgress(stInfo);
                        new OTPAuthenticator().ValidateUser(phone, forceGenerateToken: true);
                    }
                }
                else
                {
                    stInfo = $"Unable to Fetch slots availability.\nResponse Code: {response.StatusCode}, Response: {responseString}";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError(stInfo);
                }

                return apiResponse;
            }

            catch (Exception ex)
            {
                stInfo = "Error in CheckCalendarByPin:\n" + ex;
                ConsoleMethods.PrintError("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return apiResponse;
            }
            finally
            {
                logger.Info("CheckCalendarByPin API call end.");
            }
        }

        //public static APIResponse CheckCalendarByDistrict(UserDetails details, DateTime date)
        //{
        //    try
        //    {
        //        #region Generate Random String to ignore Caching
        //        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //        var stringChars = new char[16];
        //        var random = new Random();
        //        for (int i = 0; i < stringChars.Length; i++)
        //        {
        //            stringChars[i] = chars[random.Next(chars.Length)];
        //        }
        //        string randomString = new String(stringChars);
        //        #endregion

        //        var postData = "?";
        //        postData += "pincode=" + details.UserPreference.District;
        //        postData += "&date=" + date.ToString("dd-MM-yyyy");
        //        postData += "&random=" + randomString;

        //        var request = (HttpWebRequest)WebRequest.Create(AppConfig.CalendarByDistrictUrl + postData);

        //        request.Method = "GET";
        //        var response = (HttpWebResponse)request.GetResponse();

        //        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //        logger.Info("Response from status API: " + responseString);

        //        APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(responseString);
        //        return apiResponse;
        //    }

        //    catch (Exception ex)
        //    {
        //        var stInfo = "Error in CheckCalendarByPin:\n" + ex;
        //        Console.WriteLine("Please check your Internet Connection\n" + stInfo);
        //        logger.Error(stInfo);
        //        return null;
        //    }
        //}

        public static SlotBookingResponse BookSlot(List<string> beneficiaryIds, string sessionId, string slot, int dose, DateTime date, string phone)
        {
            logger.Info("BookSlot API call started.");
            string stInfo = string.Format("Trying to book Vaccination slot for Date: {0}, Slot: {1}, Session Id: {2}.", date.ToString("dd-MM-yyyy"), slot, sessionId);
            Console.WriteLine("\n" + stInfo);
            logger.Info(stInfo);
            try
            {
                SlotBookingResponse apiResponse = null;

                var client = new RestClient(AppConfig.ScheduleAppointmentUrl);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + OTPAuthenticator.BEARER_TOKEN);
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36";

                var requestObj = new SlotBookingRequest() { dose = dose, beneficiaries = beneficiaryIds.ToList(), session_id = sessionId, slot = slot };
                var serRequestObj = JsonConvert.SerializeObject(requestObj);
                request.AddParameter("application/json", serRequestObj, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                var responseString = response.Content;
                //Console.WriteLine(responseString);
                logger.Info("Response from BookSlot API: " + responseString);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse = JsonConvert.DeserializeObject<SlotBookingResponse>(responseString);
                    stInfo = "Vaccination slot has been booked Successfully!" + " - Confirmation number: " + apiResponse.appointment_confirmation_no;
                    logger.Info(stInfo);
                    ConsoleMethods.PrintSuccess(stInfo);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    stInfo = $"[WARNING] Session Expired : Regenerating Auth Token.";
                    logger.Warn(stInfo);
                    ConsoleMethods.PrintProgress(stInfo);
                    new OTPAuthenticator().ValidateUser(phone);
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    stInfo = $"[FATAL] Response From Server: {responseString}\nToo many hits from your IP address from the same Session, hence request has been blocked.";
                    logger.Warn(stInfo);
                    ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkYellow);
                    if (AppConfig.IsThrottlingToBeUsed)
                    {
                        isIPThrottled = false;
                        new Thread(new ThreadStart(IPThrottledNotifier)).Start();
                        stInfo = $"[FATAL] You can try following options:\n1.(By Default) Wait for {AppConfig.ThrottlingRefreshTime} seconds, the Application will Automatically resume working.\n2.Switch to a different network which will change your current IP address.\n3.Close the application and try again with IP Throttling Disabled.";
                        logger.Warn(stInfo);
                        ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkYellow);
                        Thread.Sleep(AppConfig.ThrottlingRefreshTime * 1000);
                        isIPThrottled = true;
                    }
                    else
                    {
                        stInfo = "IP Throttling is Disabled in 'appsettings.json'. Regenerating Auth Token.";
                        logger.Warn(stInfo);
                        ConsoleMethods.PrintProgress(stInfo);
                        new OTPAuthenticator().ValidateUser(phone, forceGenerateToken: true);
                    }
                }
                else
                {
                    stInfo = string.Format("Unable to book Vaccination slot for Date: {0}, Slot: {1}, Session Id: {2}.\nResponse Code: {3}, Response: {4}", date.ToString("dd-MM-yyyy"), slot, sessionId, response.StatusCode, responseString);
                    logger.Info(stInfo);
                    ConsoleMethods.PrintProgress(stInfo);
                }
                return apiResponse;
            }
            catch (Exception ex)
            {
                stInfo = "Error in BookSlot:\n" + ex;
                ConsoleMethods.PrintError("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return null;
            }
            finally
            {
                logger.Info("BookSlot API call end.");
            }
        }

        private static void IPThrottledNotifier()
        {
            while (!isIPThrottled)
            {
                Sound.PlayBeep(1, 800, 100); // Default Frequency: 800 Hz, Default Duration of Beep: 200 ms
                Thread.Sleep(AppConfig.ThrottlingRefreshTime * 1000 / 5);
            }
        }

        public static VersionModel FetchLatestAppVersion()
        {
            string stInfo = "FetchLatestAppVersion from GITHUB API call started.";
            logger.Info(stInfo);
            ConsoleMethods.PrintProgress(stInfo);
            try
            {
                VersionModel apiResponse = null;
                var client = new RestClient(AppConfig.FetchVersionUrl);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);

                IRestResponse response = client.Execute(request);
                var responseString = response.Content;
                //Console.WriteLine(responseString);
                logger.Info("Response from FetchLatestAppVersion API: " + responseString);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse = JsonConvert.DeserializeObject<VersionModel>(responseString);
                    stInfo = "Version fetched Successfully!";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintSuccess(stInfo);
                }
                else
                {
                    stInfo = $"Unable to Fetch Latest Version from GITHUB.\nResponse Code: {response.StatusCode}, Response: {responseString}";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError(stInfo);
                }
                return apiResponse;
            }
            catch (Exception ex)
            {
                stInfo = "Error in FetchLatestAppVersion:\n" + ex;
                ConsoleMethods.PrintError("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return null;
            }
            finally
            {
                logger.Info("FetchLatestAppVersion API call end.");
            }
        }
    }
}
