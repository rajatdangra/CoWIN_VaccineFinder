using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VaccineFinder.Enums;
using VaccineFinder.Models;
using VaccineFinder.Providers;

namespace VaccineFinder
{
    class APIs
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool isIPThrottled = false;

        private static IRestResponse GetRequest(string endpoint, bool cowinRelatedHeadersToBeUsed = true)
        {
            Authenticator authenticator = new Authenticator()
            {
                TokenType = TokenType.Bearer,
                ApiKey = OTPAuthenticator.BEARER_TOKEN,
                IsEncode = false
            };

            IRestResponse response = new APIFacade().Get(endpoint, authenticator, cowinRelatedHeadersToBeUsed);
            return response;
        }
        private static IRestResponse PostRequest(string endpoint, string requestBody, bool cowinRelatedHeadersToBeUsed = true)
        {
            Authenticator authenticator = new Authenticator()
            {
                TokenType = TokenType.Bearer,
                ApiKey = OTPAuthenticator.BEARER_TOKEN,
                IsEncode = false
            };

            IRestResponse response = new APIFacade().Post(endpoint, authenticator, requestBody, cowinRelatedHeadersToBeUsed);
            return response;
        }

        public static GenerateMobileOTPResponse GenerateMobileOTP(string phone)
        {
            string stInfo = string.Empty;
            logger.Info("GenerateMobileOTP API call started for Phone: " + phone);
            GenerateMobileOTPResponse apiResponse = null;
            try
            {
                #region RestClient
                //var client = new RestClient(AppConfig.GenerateOTPUrl);
                //client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                ////var jsonBody = "{\r\n    \"mobile\": \"" + phone + "\",\r\n    \"secret\": \"" + AppConfig.Secret + "\"\r\n}";
                var requestObj = new GenerateMobileOTPRequest() { secret = AppConfig.Secret, mobile = phone };
                var serRequestObj = JsonConvert.SerializeObject(requestObj);
                //request.AddParameter("application/json", serRequestObj /*jsonBody*/, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);

                IRestResponse response = PostRequest(AppConfig.GenerateOTPUrl, serRequestObj);
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
                var requestObj = new ValidateMobileOTPRequest() { otp = otp, txnId = txnId };
                var serRequestObj = JsonConvert.SerializeObject(requestObj);

                IRestResponse response = PostRequest(AppConfig.ConfirmOTPUrl, serRequestObj);
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

                IRestResponse response = GetRequest(AppConfig.GetBeneficiariesUrl);
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

        public static AvailabilityStatusAPIResponse CheckCalendarByPin(string pinCode, DateTime date, bool isPrecautionDose, string phone)
        {
            string stInfo = string.Empty;
            logger.Info("CheckCalendarByPin API call started.");

            AvailabilityStatusAPIResponse apiResponse = null;
            try
            {
                IRestResponse response = FetchSlotsByPINCode(pinCode, date.ToString("dd-MM-yyyy"), isPrecautionDose, string.Empty, generateRandomString: false);
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

        private static IRestResponse FetchSlotsByPINCode(string pinCode, string searchDate, bool isPrecautionDose, string vaccineType, bool generateRandomString = false)
        {
            UriBuilder builder;
            builder = new UriBuilder(AppConfig.CalendarByPinUrl);
            NameValueCollection queryString = HttpUtility.ParseQueryString(builder.Query);
            if (AppConfig.ProtectedAPIToBeUsed)
            {
                if (!string.IsNullOrEmpty(vaccineType))
                {
                    queryString["vaccine"] = vaccineType;
                }
            }
            else
            {
                string stInfo = $"[WARNING] Using Public API (Data could be OUTDATED), since ProtectedAPIToBeUsed is Disabled in appsettings.json";
                logger.Info(stInfo);
                ConsoleMethods.PrintInfo(stInfo, color: ConsoleColor.DarkYellow);
            }

            queryString["pincode"] = pinCode;
            queryString["precaution_flag"] = isPrecautionDose ? "true" : "false";
            queryString["date"] = searchDate;
            if (generateRandomString)
            {
                #region Generate Random String to ignore Caching
                string randomString = StringMethods.GenerateRandomString();
                #endregion
                queryString["random"] = randomString;
            }
            builder.Query = queryString.ToString();
            string endpoint = builder.ToString();

            IRestResponse response = GetRequest(endpoint);
            return response;
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

        public static SlotBookingResponse BookSlot(List<string> beneficiaryIds, int centerId, string sessionId, string slot, int dose, bool isPrecautionDose, DateTime date, string phone)
        {
            logger.Info("BookSlot API call started.");
            string stInfo = string.Format("Trying to book Vaccination slot for Date: {0}, Slot: {1}, Center Id: {2}, Session Id: {3}.", date.ToString("dd-MM-yyyy"), slot, centerId, sessionId);
            Console.WriteLine("\n" + stInfo);
            logger.Info(stInfo);
            try
            {
                SlotBookingResponse apiResponse = null;

                var requestObj = new SlotBookingRequest() { dose = dose, is_precaution = isPrecautionDose, beneficiaries = beneficiaryIds.ToList(), center_id = centerId, session_id = sessionId, slot = slot };
                var serRequestObj = JsonConvert.SerializeObject(requestObj);
                IRestResponse response = PostRequest(AppConfig.ScheduleAppointmentUrl, serRequestObj);
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
                    stInfo = string.Format("Unable to book Vaccination slot for Date: {0}, Slot: {1}, Center Id: {2}, Session Id: {3}.\nResponse Code: {4}, Response: {5}", date.ToString("dd-MM-yyyy"), slot, centerId, sessionId, response.StatusCode, responseString);
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

        public static void DownloadAppointmentSlip(string appointmentConfirmationNumber, string phone)
        {
            string stInfo = string.Empty;
            try
            {
                UriBuilder builder;
                NameValueCollection queryString;
                var fileName = "Co-WIN Appointment_No_" + appointmentConfirmationNumber + ".pdf";
                builder = new UriBuilder(AppConfig.AppointmentSlipUrl);
                queryString = HttpUtility.ParseQueryString(builder.Query);
                queryString["appointment_id"] = appointmentConfirmationNumber;
                builder.Query = queryString.ToString();

                string endpoint = builder.ToString();

                IRestResponse response = GetRequest(endpoint);
                var responseString = response.Content;

                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), fileName), response.RawBytes);
                    stInfo = $"Appointment Slip Successfully Downloaded for Confirmation number: {appointmentConfirmationNumber}. Saved Path: {Path.Combine(Directory.GetCurrentDirectory(), fileName)}";
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
                else
                {
                    stInfo = $"Unable to Download Appointment Slip for Confirmation number: {appointmentConfirmationNumber}.\nResponse Code: {response.StatusDescription}, Response: {responseString}";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError(stInfo);
                }
            }
            catch (Exception e)
            {
                stInfo = $"Error while downloading Appointment Slip, please download from Co-WIN PORTAL! Details: {e}";
                logger.Info(stInfo);
                ConsoleMethods.PrintError(stInfo);
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
            string stInfo = "FetchLatestAppVersion API call started.";
            logger.Info(stInfo);
            ConsoleMethods.PrintProgress(stInfo);
            try
            {
                VersionModel apiResponse = null;

                IRestResponse response = GetRequest(AppConfig.FetchVersionUrl, cowinRelatedHeadersToBeUsed: false);
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
                    stInfo = $"Unable to Fetch Latest Version.\nResponse Code: {response.StatusCode}, Response: {responseString}";
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

        public static TelegramBotUpdatesModel FetchUpdatesFromTelegramBot()
        {
            string stInfo = "FetchUpdatesFromTelegramBot API call started.";
            logger.Info(stInfo);
            ConsoleMethods.PrintProgress(stInfo);
            try
            {
                TelegramBotUpdatesModel apiResponse = null;
                string endPoint = AppConfig.TelegramFetchBotUpdatesUrl.Replace("<token>", Private.PrivateData.TelegramBotToken);
                IRestResponse response = GetRequest(endPoint, cowinRelatedHeadersToBeUsed: false);
                var responseString = response.Content;
                //Console.WriteLine(responseString);
                logger.Info("Response from FetchUpdatesFromTelegramBot API: " + responseString);
                if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                {
                    apiResponse = JsonConvert.DeserializeObject<TelegramBotUpdatesModel>(responseString);
                    stInfo = "Updates fetched Successfully!";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintSuccess(stInfo);
                }
                else
                {
                    stInfo = $"Unable to Fetch Updates.\nResponse Code: {response.StatusCode}, Response: {responseString}";
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError(stInfo);
                }
                return apiResponse;
            }
            catch (Exception ex)
            {
                stInfo = "Error in FetchUpdatesFromTelegramBot:\n" + ex;
                ConsoleMethods.PrintError("Please check your Internet Connection\n" + stInfo);
                logger.Error(stInfo);
                return null;
            }
            finally
            {
                logger.Info("FetchUpdatesFromTelegramBot API call end.");
            }
        }
    }
}
