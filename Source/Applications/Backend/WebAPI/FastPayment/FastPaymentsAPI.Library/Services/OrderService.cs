using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FastPaymentsApi.Contracts.Responses;
using Microsoft.Extensions.Configuration;

namespace FastPaymentsAPI.Library.Services
{
	public class OrderService : IOrderService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public OrderService(HttpClient client, IConfiguration configuration)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task<OrderRegistrationResponseDTO> RegisterOrderAsync(string xmlStringOrderRegistrationRequestDTO)
		{
			var httpContent =
				new StringContent($"xml={xmlStringOrderRegistrationRequestDTO}", Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await _httpClient.PostAsync(
				_configuration.GetSection("OrderService").GetValue<string>("RegisterOrderEndpointURI"), httpContent);

			await using var responseStream = await response.Content.ReadAsStreamAsync();
			return (OrderRegistrationResponseDTO)new XmlSerializer(typeof(OrderRegistrationResponseDTO)).Deserialize(responseStream);
		}
		
		public async Task<OrderInfoResponseDTO> GetOrderInfoAsync(string xmlStringOrderInfoDTO)
		{
			var httpContent = new StringContent($"xml={xmlStringOrderInfoDTO}", Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await _httpClient.PostAsync(
				_configuration.GetSection("OrderService").GetValue<string>("GetOrderInfoEndpointURI"), httpContent);
			
			await using var responseStream = await response.Content.ReadAsStreamAsync();
			return (OrderInfoResponseDTO)new XmlSerializer(typeof(OrderInfoResponseDTO)).Deserialize(responseStream);
		}
		
		public async Task<CancelPaymentResponseDTO> CancelPaymentAsync(string xmlStringFromCancelPaymentRequestDTO)
		{
			var httpContent = new StringContent(
				$"xml={xmlStringFromCancelPaymentRequestDTO}",
				Encoding.UTF8,
				"application/x-www-form-urlencoded");
			var response = await _httpClient.PostAsync(
				_configuration.GetSection("OrderService").GetValue<string>("CancelPaymentEndpointURI"), httpContent);
			
			await using var responseStream = await response.Content.ReadAsStreamAsync();
			return (CancelPaymentResponseDTO)new XmlSerializer(typeof(CancelPaymentResponseDTO)).Deserialize(responseStream);
		}
	}
}
