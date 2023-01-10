using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InstantSmsService;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;
using VodovozInfrastructure.Utils;

namespace Vodovoz.Additions
{
	public class SmsSender
	{
		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
        private readonly IInstantSmsService _instantSmsService;
		private HttpClient _httpClient;
		
        public SmsSender(
			IFastPaymentParametersProvider fastPaymentParametersProvider,
			IInstantSmsService service)
        {
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
            _instantSmsService = service ?? throw new ArgumentNullException(nameof(service));
        }

		public async Task<ResultMessage> SendFastPaymentUrlAsync(Order order, string phoneNumber, bool isQr)
		{
			var orderId = order.Id;
			if(_instantSmsService == null)
			{
				return new ResultMessage { ErrorDescription = "Сервис отправки Sms не работает, обратитесь в РПО." };
			}

			var realPhoneNumber = PhoneUtils.RemoveNonDigit(phoneNumber);
			var response = await GetFastPaymentResponseDtoAsync(orderId, realPhoneNumber, isQr);

			if(response == null)
			{
				return new ResultMessage { ErrorDescription = "Не удалось получить ответ от банка, обратитесь в РПО" };
			}

			if(response.ErrorMessage != null)
			{
				return new ResultMessage { ErrorDescription = response.ErrorMessage };
			}

			if(response.FastPaymentStatus.HasValue && response.FastPaymentStatus == FastPaymentStatus.Performed)
			{
				var resultMessage = new ResultMessage
				{
					ErrorDescription = $"Заказ №{orderId} оплачен",
					IsPaidStatus = true
				};
				return resultMessage;
			}

			var smsMessage = new InstantSmsMessage
			{
				MessageText = $"Ссылка на оплату заказа №{orderId}\n" +
					$"{_fastPaymentParametersProvider.GetVodovozFastPayBaseUrl}/{response.FastPaymentGuid}",
				MobilePhone = phoneNumber,
				ExpiredTime = DateTime.Now.AddMinutes(10)
			};

			return _instantSmsService.SendSms(smsMessage);
		}

		private async Task<FastPaymentResponseDTO> GetFastPaymentResponseDtoAsync(int orderid, string phoneNumber, bool isQr)
		{
			using(_httpClient = HttpClientFactory.Create())
			{
				_httpClient.BaseAddress = new Uri(_fastPaymentParametersProvider.GetFastPaymentApiBaseUrl);
				_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				var responseTask = await _httpClient.PostAsJsonAsync("/api/RegisterOrder", new { orderid, phoneNumber, isQr });

				return await responseTask.Content.ReadAsAsync<FastPaymentResponseDTO>();
			}
		}
	}

	public class FastPaymentResponseDTO
	{
		public string Ticket { get; set; }
		public Guid FastPaymentGuid { get; set; }
		public string ErrorMessage { get; set; }
		public FastPaymentStatus? FastPaymentStatus { get; set; }
	}
}
