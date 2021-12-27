using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccineFinder.Enums;

namespace VaccineFinder.Providers
{
    public class APIFacade
    {
        private readonly IConfiguration _configuration;
        public APIFacade(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public APIFacade()
        {

        }

        public IRestResponse Get(string endpoint, Authenticator authenticator, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.GET);

            AddGenericHeaders(request, authenticator, cowinRelatedHeadersToBeUsed);

            IRestResponse response = client.Execute(request);

            return response;
        }

        public Task<IRestResponse> GetAsync(string endpoint, Authenticator authenticator, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.GET);

            AddGenericHeaders(request, authenticator, cowinRelatedHeadersToBeUsed);

            var response = client.ExecuteAsync(request);

            return response;
        }

        private void AddGenericHeaders(IRestRequest request, Authenticator authenticator, bool cowinRelatedHeadersToBeUsed = true)
        {
            request.AddHeader("accept", "application/json");
            request.AddHeader("Accept-Language", "en_US");
            request.AddHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
            
            if (AppConfig.ProtectedAPIToBeUsed && cowinRelatedHeadersToBeUsed)
            {
                request.AddHeader("Origin", AppConfig.CoWIN_RegistrationURL);
                request.AddHeader("Referer", AppConfig.CoWIN_RegistrationURL);
                //request.AddHeader("Authorization", $"Bearer {OTPAuthenticator.BEARER_TOKEN}");
                if (authenticator != null)
                {
                    switch (authenticator.TokenType)
                    {
                        case TokenType.None:
                            break;
                        case TokenType.Bearer:
                            authenticator.ApiKey = authenticator.IsEncode ? APIFacade.Encoding(authenticator.ApiKey) : authenticator.ApiKey;
                            request.AddHeader("Authorization", $"{TokenType.Bearer} {authenticator.ApiKey}");
                            break;
                        case TokenType.Basic:
                            string apikey = $"{authenticator.Username}:{authenticator.Password}";
                            apikey = authenticator.IsEncode ? APIFacade.Encoding(apikey) : apikey;
                            request.AddHeader("Authorization", $"{TokenType.Basic} {apikey}");
                            break;
                    }
                }
                //client.Authenticator = New HTTPBasicAuthentication(encodedAPIKey, "");
            }
        }

        public IRestResponse Post(string endpoint, Authenticator authenticator, string body = null, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.POST);

            AddGenericHeaders(request, authenticator, cowinRelatedHeadersToBeUsed);

            AddPostSpecificParameters(body, request);

            IRestResponse response = client.Execute(request);

            return response;
        }

        public Task<IRestResponse> PostAsync(string endpoint, Authenticator authenticator, string body = null, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.POST);

            AddGenericHeaders(request, authenticator, cowinRelatedHeadersToBeUsed);

            AddPostSpecificParameters(body, request);

            var response = client.ExecuteAsync(request);

            return response;
        }

        public Task<IRestResponse> PutAsync(string endpoint, Authenticator authenticator, string body = null, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.PUT);

            AddGenericHeaders(request, authenticator, cowinRelatedHeadersToBeUsed);

            AddPostSpecificParameters(body, request);

            var response = client.ExecuteAsync(request);

            return response;
        }

        public Task<IRestResponse> DeleteAsync(string endpoint, Authenticator authenticator, string body = null, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.DELETE);

            AddGenericHeaders(request, authenticator, cowinRelatedHeadersToBeUsed);

            AddPostSpecificParameters(body, request);

            var response = client.ExecuteAsync(request);

            return response;
        }

        private static void AddPostSpecificParameters(string body, IRestRequest request)
        {
            request.AddHeader("Content-Type", "application/json");

            if (body != null)
            {
                request.AddParameter("application/json", body, ParameterType.RequestBody);
            }
        }

        private RestClient InitHttpClient(string endpoint, bool cowinRelatedHeadersToBeUsed = true)
        {
            var client = new RestClient(endpoint);
            //client.Timeout = -1;
            //client.Timeout = Convert.ToInt32(_configuration["App:APIRequestTimeout"]) * 1000;
            client.Timeout = AppConfig.APIRequestTimeout * 1000;
            if (AppConfig.ProtectedAPIToBeUsed && cowinRelatedHeadersToBeUsed)
            {
                client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36"/*RandomUserAgent.UserAgent*/;
            }

            //if (Convert.ToBoolean(_configuration["Proxy:IsToBeUsed"]))
            //{
            //    client.Proxy = new WebProxy
            //    {
            //        Address = new Uri(_configuration["Proxy:Address"]),
            //        UseDefaultCredentials = true
            //    };
            //}

            return client;
        }

        //Converts API Key to Base64 
        private static string Encoding(string toEncode)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding(28591).GetBytes(toEncode);
            string toReturn = System.Convert.ToBase64String(bytes);
            return toReturn;
        }
    }
}
