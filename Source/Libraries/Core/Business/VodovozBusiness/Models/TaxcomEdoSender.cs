using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

		public async Task<string> SendUpdByOrderAsync(int orderId)
		{
			var uri = $"SendUpdByOrder?orderId={orderId}";
			var responseTimeoutInSeconds = 2;

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
							return $"Ошибка отправки. Сервер вернул код \"{response.StatusCode}\"";
						}

						return "Успешно выполнена повторная отправка";
					}
					catch(OperationCanceledException e)
					{
						return "Ошибка отправки. Сервер не обработал запрос за выделенное время";
					}
					catch (Exception e)
					{
						return $"Ошибка отправки. Текст ошибки: {e.Message}";
					}
				}
			}
		}
	}
}
