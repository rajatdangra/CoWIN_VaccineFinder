using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder.Providers
{
    public class APIFacade
    {
        public APIFacade()
        {
        }
        public IRestResponse Get(string endpoint, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.GET);

            AddGenericHeaders(request, cowinRelatedHeadersToBeUsed);

            IRestResponse response = client.Execute(request);

            return response;
        }

        private void AddGenericHeaders(IRestRequest request, bool cowinRelatedHeadersToBeUsed = true)
        {
            request.AddHeader("accept", "application/json");
            request.AddHeader("Accept-Language", "en_US");
            request.AddHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");

            if (AppConfig.ProtectedAPIToBeUsed && cowinRelatedHeadersToBeUsed)
            {
                request.AddHeader("Origin", AppConfig.CoWIN_RegistrationURL);
                request.AddHeader("Referer", AppConfig.CoWIN_RegistrationURL);
                request.AddHeader("Authorization", $"Bearer {OTPAuthenticator.BEARER_TOKEN}");
            }
        }

        public IRestResponse Post(string endpoint, string body = null, bool cowinRelatedHeadersToBeUsed = true)
        {
            RestClient client = InitHttpClient(endpoint, cowinRelatedHeadersToBeUsed);

            IRestRequest request = new RestRequest(Method.POST);

            AddGenericHeaders(request, cowinRelatedHeadersToBeUsed);

            AddPostSpecificParameters(body, request);

            IRestResponse response = client.Execute(request);

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
            client.Timeout = -1;
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
    }
}
