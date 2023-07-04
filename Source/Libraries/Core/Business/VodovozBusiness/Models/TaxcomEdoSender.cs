using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using Vodovoz.Settings.Edo;

namespace Vodovoz.Models
{
	public class TaxcomEdoSender
	{
		private readonly IEdoSettings _edoSettings;

		public TaxcomEdoSender(IEdoSettings edoSettings)
		{
			_edoSettings = edoSettings ?? throw new System.ArgumentNullException(nameof(edoSettings));
		}

		public async Task<EdoSendResult> SendUpdByOrderAsync(int orderId)
		{
			var uri = $"SendUpdByOrder?orderId={orderId}";
			var responseTimeoutInSeconds = 2;
			var sendResult = new EdoSendResult();

			using(var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(responseTimeoutInSeconds)))
			{
				using(var httpClient = HttpClientFactory.Create())
				{
					httpClient.BaseAddress = new Uri(_edoSettings.TaxcomBaseAddressUri);

					try
					{
						var response = await httpClient.GetAsync(uri, cancel.Token);

						if(!response.IsSuccessStatusCode)
						{
							sendResult.IsSuccess = false;
							sendResult.Message = $"Ошибка отправки. Сервер вернул код \"{response.StatusCode}\"";
							return sendResult;
						}

						sendResult.IsSuccess = true;
						sendResult.Message = "Успешно выполнена повторная отправка";
						return sendResult;
					}
					catch(OperationCanceledException e)
					{
						sendResult.IsSuccess = false;
						sendResult.Message = "Ошибка отправки. Сервер не обработал запрос за выделенное время";
						return sendResult;
					}
					catch (Exception e)
					{
						sendResult.IsSuccess = false;
						sendResult.Message = $"Ошибка отправки. Текст ошибки: {e.Message}";
						return sendResult;
					}
				}
			}
		}

		public class EdoSendResult
		{
			public bool IsSuccess { get; set; } = false;
			public string Message { get; set; } = string.Empty;
		}
	}
}
