using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
    public class FCMAPIHelper : IFCMAPIHelper
    {
        private string sendPaymentEndpointURI;
        private HttpClient _apiClient;

        public FCMAPIHelper(IConfiguration configuration)
        {
            InitializeClient(configuration);
        }

        public async Task SendPushNotification(string pushNotificationClientToken, string sender, string message)
        {
            var request = new FCMSendPushRequestModel() 
            { 
                to = pushNotificationClientToken,
                data = new FCMSendPushMessageModel()
                {
                    notificationType = "orderPaymentStatusChange",
                    sender = sender,
                    message = message
                }
            };

            using (HttpResponseMessage response = await _apiClient.PostAsJsonAsync(sendPaymentEndpointURI, request))
            {
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else
                {
                    throw new FCMException(response.ReasonPhrase);
                }
            }
        }

        private void InitializeClient(IConfiguration configuration)
        {
            var apiConfiguration = configuration.GetSection("FCMAPI");

            _apiClient = new HttpClient();
            _apiClient.BaseAddress = new Uri(apiConfiguration["ApiBase"]);
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _apiClient.DefaultRequestHeaders.Add("Authorization", $"key={apiConfiguration["AccessToken"]}");
            _apiClient.DefaultRequestHeaders.Add("Sender", $"id={apiConfiguration["AppId"]}");

            sendPaymentEndpointURI = apiConfiguration["SendPaymentEndpointURI"];
        }
    }

    public class FCMException : Exception
    {
        public FCMException(string message) : base(message){ }
    }

    public class FCMSendPushRequestModel
    {
        public string to { get; set; }
        public FCMSendPushMessageModel data { get; set; }
    }

    public class FCMSendPushMessageModel
    {
        public string notificationType { get; set; }
        public string message { get; set; }
        public string sender { get; set; }
    }
}
