using MassTransit;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TrueMark.Api.Extensions;
using TrueMark.Contracts;
using TrueMark.Contracts.Requests;
using TrueMark.Contracts.Responses;

namespace TrueMark.ProductInstanceInfoCheck.Worker;
internal class ProductInstanceInfoRequestConsumer : IConsumer<Batch<ProductInstanceInfoRequest>>
{
	private const string _uri = "cises/info";
	private const int _codePortionLimit = 100;
	private readonly ILogger<ProductInstanceInfoRequestConsumer> _logger;
	private readonly IOptionsMonitor<TrueMarkProductInstanceInfoCheckOptions> _optionsMonitor;
	private readonly HttpClient _httpClient;

	public ProductInstanceInfoRequestConsumer(
		ILogger<ProductInstanceInfoRequestConsumer> logger,
		IOptionsMonitor<TrueMarkProductInstanceInfoCheckOptions> optionsMonitor,
		HttpClient httpClient)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	}

	public async Task Consume(ConsumeContext<Batch<ProductInstanceInfoRequest>> context)
	{
		var grouped = context.Message.GroupBy(m => m.Message.Bearer);

		foreach(var group in grouped)
		{
			await ProcessBearerGroup(group);
		}
	}

	private async Task ProcessBearerGroup(IGrouping<string, ConsumeContext<ProductInstanceInfoRequest>> group)
	{
		var codes = group.Select(m => m.Message.ProductCode).ToList();

		if(!codes.Any())
		{
			return;
		}

		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", group.Key);

		var productInstanceStatuses = new List<ProductInstanceStatus>();

		while(codes.Any())
		{
			var codesPortion = codes.Take(_codePortionLimit).ToArray();

			var errorMessage = new StringBuilder();
			errorMessage.AppendLine("Не удалось получить данные о статусах экземпляров товаров.");

			var response = await _httpClient.PostAsJsonAsync<IEnumerable<string>>(_uri, codesPortion); // try catch, настроить клиент, таймаут 60 сек

			var currentPortionContexts = group.Where(g => codesPortion.Contains(g.Message.ProductCode));

			if(!response.IsSuccessStatusCode)
			{
				errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}");

				await RespondError(errorMessage, currentPortionContexts);

				codes.RemoveRange(0, _codePortionLimit);

				continue;
			}

			string responseBody = await response.Content.ReadAsStringAsync();
			var cisesInformations = JsonSerializer.Deserialize<IEnumerable<CisInfoRoot>>(responseBody); // try catch
			_logger.LogInformation("responseBody: {ResponseBody}", responseBody);

			if(cisesInformations is null)
			{
				errorMessage.AppendLine("Не удалось получить ожидаемый ответ от сервера");

				await RespondError(errorMessage, currentPortionContexts);

				codes.RemoveRange(0, _codePortionLimit);

				continue;
			}

			foreach(var cisesInformation in cisesInformations)
			{
				var matchingGroup = group.Where(g => g.Message.ProductCode == cisesInformation.CisInfo.RequestedCis).FirstOrDefault();

				matchingGroup?.Respond(new ProductInstanceInfoResponse
				{
					InstanceStatus = new ProductInstanceStatus
					{
						IdentificationCode = cisesInformation.CisInfo.RequestedCis,
						Status = cisesInformation.CisInfo.Status.ToProductInstanceStatusEnum(),
						OwnerInn = cisesInformation.CisInfo.OwnerInn,
						OwnerName = cisesInformation.CisInfo.OwnerName
					}
				});
			}

			codes.RemoveRange(0, _codePortionLimit);

			await Task.Delay(_optionsMonitor.CurrentValue.RequestsDelay);
		}
	}

	private async Task RespondError(StringBuilder errorMessage, IEnumerable<ConsumeContext<ProductInstanceInfoRequest>> currentPortionContexts)
	{
		foreach(var context in currentPortionContexts)
		{
			context.Respond(new ProductInstanceInfoResponse
			{
				ErrorMessage = errorMessage.ToString()
			});
		}

		await Task.Delay(_optionsMonitor.CurrentValue.RequestsDelay);
	}
}
