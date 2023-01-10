﻿using Google.Protobuf.WellKnownTypes;
using Sms.Internal;
using Sms.Internal.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;
using Vodovoz.Settings.Sms;
using VodovozInfrastructure.Utils;

namespace Vodovoz.Models
{
    public class FastPaymentSender
    {
        private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		private readonly SmsClientChannelFactory _smsClientFactory;
		private readonly ISmsSettings _smsSettings;
		private HttpClient _httpClient;
		
        public FastPaymentSender(
			IFastPaymentParametersProvider fastPaymentParametersProvider,
			SmsClientChannelFactory smsClientFactory,
			ISmsSettings smsSettings)
        {
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
			_smsClientFactory = smsClientFactory ?? throw new ArgumentNullException(nameof(smsClientFactory));
			_smsSettings = smsSettings ?? throw new ArgumentNullException(nameof(smsSettings));
		}

		public async Task<FastPaymentResult> SendFastPaymentUrlAsync(Order order, string phoneNumber, bool isQr)
		{
			var orderId = order.Id;

			if(!_smsSettings.SmsSendingAllowed)
			{
				var resultMessage = GetErrorResult("Отправка смс сообщений не разрешена настройками приложения. Обратитесь в техподдержку.");
				return resultMessage;
			}

			using(var smsChannel = _smsClientFactory.OpenChannel())
			{
				var realPhoneNumber = PhoneUtils.RemoveNonDigit(phoneNumber);
				var response = await GetFastPaymentResponseDtoAsync(orderId, realPhoneNumber, isQr);

				if(response == null)
				{
					var resultMessage = GetErrorResult("Не удалось получить ответ от банка, обратитесь в РПО");
					return resultMessage;
				}

				if(response.ErrorMessage != null)
				{
					var resultMessage = GetErrorResult(response.ErrorMessage);
					return resultMessage;
				}

				if(response.FastPaymentStatus.HasValue && response.FastPaymentStatus == FastPaymentStatus.Performed)
				{
					var resultMessage = GetErrorResult($"Заказ №{orderId} оплачен");
					resultMessage.OrderAlreadyPaied = true;
					return resultMessage;
				}

				var smsMessage = new SmsMessage
				{
					MessageText = $"Ссылка на оплату заказа №{orderId}\n" +
						$"{_fastPaymentParametersProvider.GetVodovozFastPayBaseUrl}/{response.FastPaymentGuid}",
					MobilePhone = phoneNumber,
					ExpiredTime = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(10))
				};

				var result = await smsChannel.Client.SendAsync(smsMessage);

				return new FastPaymentResult
				{
					Status = result.Status,
					ErrorMessage = result.ErrorDescription
				};
			}
		}

		private FastPaymentResult GetErrorResult(string message)
		{
			var resultMessage = new FastPaymentResult
			{
				Status = ResultStatus.Error,
				ErrorMessage = message
			};
			return resultMessage;
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

	public class FastPaymentResult
	{
		public ResultStatus Status { get; set; }
		public string ErrorMessage { get; set; }
		public bool OrderAlreadyPaied { get; set; }
	}

	public class FastPaymentResponseDTO
	{
		public string Ticket { get; set; }
		public Guid FastPaymentGuid { get; set; }
		public string ErrorMessage { get; set; }
		public FastPaymentStatus? FastPaymentStatus { get; set; }
	}
}
