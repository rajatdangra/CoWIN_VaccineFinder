using NLog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder
{
    class OTPAuthenticator
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static string BEARER_TOKEN;

        public OTPAuthenticator()
        {
        }

        public void ValidateUser(string phone)
        {
            if (!string.IsNullOrEmpty(BEARER_TOKEN))
            {
                // Check if user already has a valid bearer token, if yes use it.
                if (IsValidBearerToken(BEARER_TOKEN))
                {
                    ConsoleMethods.PrintSuccess(string.Format("Token is valid. Resuming Session for phone: {0} at {1}", phone, DateTime.Now.ToDetailString()));
                    return;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARNING] Invalid/Expired Bearer Token provided in config file. Re-generating OTP to establish session!");
                    Console.ResetColor();
                }
            }

            var GenerateMobileOTPResponse = GenerateMobileOTP(phone);
            if (GenerateMobileOTPResponse == null)
                return;

            string inputMessage = "Please Enter OTP:";
            Console.WriteLine(inputMessage);
            var otp = Console.ReadLine();
            var ValidateMobileOTPResponse = ValidateMobileOTP(otp, GenerateMobileOTPResponse.txnId);
            while (ValidateMobileOTPResponse == null)
            {
                string stInfo = "Invalid OTP. Please Retry.";
                logger.Info(stInfo + ": " + otp);
                //Console.WriteLine(stInfo);
                Console.WriteLine(inputMessage);
                otp = Console.ReadLine();
                ValidateMobileOTPResponse = ValidateMobileOTP(otp, GenerateMobileOTPResponse.txnId);
            }
        }

        public GenerateMobileOTPResponse GenerateMobileOTP(string phone)
        {
            string stInfo = "GenerateMobileOTP Call Started for Phone: " + phone;
            GenerateMobileOTPResponse response = null;
            try
            {
                logger.Info(stInfo);
                Console.WriteLine(stInfo);

                response = APIs.GenerateMobileOTP(phone);

                if (response != null)
                {
                    stInfo = "OTP Sent Successfully to Phone: " + phone;
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);
                }
                else
                {
                    stInfo = "Not able to send OTP";
                    //Console.WriteLine(stInfo);
                    logger.Info(stInfo);
                }
                return response;
            }
            catch (Exception ex)
            {
                stInfo = "Error in GenerateMobileOTP:\n" + ex;
                logger.Error(stInfo);
                ConsoleMethods.PrintError(stInfo);
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
                    BEARER_TOKEN = response.token;
                    stInfo = string.Format("OTP Verified. Bearer Token Generated Successfully at {0}. It is valid for only 15 minutes and will expire on {1}", DateTime.Now.ToDetailString(), GetTokenExpiryDateTime(BEARER_TOKEN).ToDetailString());
                    ConsoleMethods.PrintSuccess(stInfo);
                    logger.Info(stInfo);
                }
                else
                {
                    stInfo = "Unable to verifiy OTP: " + otp;
                    logger.Info(stInfo);
                    ConsoleMethods.PrintError(stInfo);
                }
                return response;
            }
            catch (Exception ex)
            {
                stInfo = "Error in GenerateMobileOTP:\n" + ex;
                logger.Error(stInfo);
                ConsoleMethods.PrintError(stInfo);
                return response;
            }
        }

        private bool IsValidBearerToken(string bearerToken)
        {
            DateTime expiryDateTime = GetTokenExpiryDateTime(bearerToken);
            DateTime currentDateTime = DateTime.Now;
            return currentDateTime.CompareTo(expiryDateTime) < 0;
        }

        private static DateTime GetTokenExpiryDateTime(string bearerToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(bearerToken);

            var expiryTime = Convert.ToInt64(jsonToken.Claims.First(claim => claim.Type == "exp").Value);

            DateTime expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryTime).LocalDateTime;
            return expiryDateTime;
        }
    }
}
