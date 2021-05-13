using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
    public class SmsPaymentServiceAPIHelper : ISmsPaymentServiceAPIHelper
    {
        private string sendPaymentEndpointURI = "SendPayment";
        private HttpClient _apiClient;

        public SmsPaymentServiceAPIHelper(IConfiguration configuration)
        {
            InitializeClient(configuration);
        }

        public async Task SendPayment(int orderId, string phoneNumber)
        {
            using (HttpResponseMessage response = await _apiClient.PostAsJsonAsync(sendPaymentEndpointURI, new { OrderId = orderId, PhoneNumber = phoneNumber }))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<SendPaymentResponseModel>();

                    if (result.Status == SendPaymentResponseModelMessageStatus.Ok)
                    {
                        return;
                    }

                    throw new SmsPaymentServiceAPIHelperException(result.ErrorDescription);
                }
                else
                {
                    throw new SmsPaymentServiceAPIHelperException(response.ReasonPhrase);
                }
            }
        }

        private void InitializeClient(IConfiguration configuration)
        {
            var apiConfiguration = configuration.GetSection("SmsPaymentServiceAPI") as IConfigurationSection;

            _apiClient = new HttpClient();
            _apiClient.BaseAddress = new Uri(apiConfiguration["ApiBase"]);
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            sendPaymentEndpointURI = apiConfiguration["SendPaymentEndpointURI"];
        }
    }
}
