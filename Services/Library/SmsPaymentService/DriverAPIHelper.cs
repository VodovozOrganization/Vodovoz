using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SmsPaymentService
{
    public class DriverAPIHelper : ISmsPaymentStatusNotificationReciever
    {
        private string notifyOfSmsPaymentStatusChangedURI;
        private HttpClient _apiClient;

        public DriverAPIHelper(IConfiguration configuration)
        {
            InitializeClient(configuration);
        }

        private void InitializeClient(IConfiguration configuration)
        {
            var apiConfiguration = configuration.GetSection("DriverAPI");

            _apiClient = new HttpClient();
            _apiClient.BaseAddress = new Uri(apiConfiguration["ApiBase"]);
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            notifyOfSmsPaymentStatusChangedURI = apiConfiguration["NotifyOfSmsPaymentStatusChangedURI"];
        }

        public async Task NotifyOfSmsPaymentStatusChanged(int orderId)
        {
            using (HttpResponseMessage response = await _apiClient.PostAsJsonAsync(notifyOfSmsPaymentStatusChangedURI, orderId))
            {
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
                else
                {
                    throw new DriverAPIHelperException(response.ReasonPhrase);
                }
            }
        }
    }

    public class DriverAPIHelperException : Exception
    {
        public DriverAPIHelperException(string message) : base(message) { }
    }
}
