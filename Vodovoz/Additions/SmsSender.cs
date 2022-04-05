using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InstantSmsService;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Services;
using VodovozInfrastructure.Utils;

namespace Vodovoz.Additions
{
    public class SmsSender
    {
        private readonly ISmsNotifierParametersProvider _smsNotifierParametersProvider;
        private readonly IInstantSmsService _instantSmsService;
		private HttpClient _httpClient;
		
        public SmsSender(
			ISmsNotifierParametersProvider smsNotifierParametersProvider,
			IInstantSmsService service)
        {
            _smsNotifierParametersProvider = smsNotifierParametersProvider ?? 
				throw new ArgumentNullException(nameof(smsNotifierParametersProvider));
            _instantSmsService = service ?? throw new ArgumentNullException(nameof(service));
        }
        
        public ResultMessage SendPassword(string phone, string login, string password)
        {
            #region Формирование
            
            if(!_smsNotifierParametersProvider.IsSmsNotificationsEnabled) {
                return new ResultMessage { ErrorDescription = "Sms уведомления выключены" };
            }
            if(String.IsNullOrWhiteSpace(password)) {
                return new ResultMessage { ErrorDescription = "Был передан неверный пароль" };
            }

            string messageText = $"Логин: {login}\nПароль: {password}";
            
            if(_instantSmsService == null) {
                return new ResultMessage { ErrorDescription = "Сервис отправки Sms не работает, обратитесь в РПО." };
            }
            
            var smsNotification = new InstantSmsMessage {
                MessageText = messageText,
                MobilePhone = phone,
                ExpiredTime = DateTime.Now.AddMinutes(10)
            };

            #endregion
            
            return _instantSmsService.SendSms(smsNotification);
        }

		public async Task<ResultMessage> SendFastPaymentUrlAsync(int orderId, string phoneNumber)
		{
			if(_instantSmsService == null)
			{
				return new ResultMessage { ErrorDescription = "Сервис отправки Sms не работает, обратитесь в РПО." };
			}

			var realPhoneNumber = PhoneUtils.RemoveNonDigit(phoneNumber);
			var response = await GetFastPaymentResponseDtoAsync(orderId, realPhoneNumber);

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
					$"{_smsNotifierParametersProvider.GetAvangardFastPayBaseUrl()}?ticket={response.Ticket}",
				MobilePhone = phoneNumber,
				ExpiredTime = DateTime.Now.AddMinutes(10)
			};

			return _instantSmsService.SendSms(smsMessage);
		}

		private async Task<FastPaymentResponseDTO> GetFastPaymentResponseDtoAsync(int orderid, string phoneNumber)
		{
			using(_httpClient = HttpClientFactory.Create())
			{
				_httpClient.BaseAddress = new Uri(_smsNotifierParametersProvider.GetFastPaymentApiBaseUrl());
				_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				var responseTask = await _httpClient.PostAsJsonAsync("/api/RegisterOrder", new { orderid, phoneNumber });

				return await responseTask.Content.ReadAsAsync<FastPaymentResponseDTO>();
			}
		}
	}

	public class FastPaymentResponseDTO
	{
		public string Ticket { get; set; }
		public string ErrorMessage { get; set; }
		public FastPaymentStatus? FastPaymentStatus { get; set; }
	}

	public class CancelFastPaymentDTO
	{
		public int ResponseCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}
