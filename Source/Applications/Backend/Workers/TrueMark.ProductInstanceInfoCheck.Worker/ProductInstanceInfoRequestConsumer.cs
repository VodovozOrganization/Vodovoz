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
public class ProductInstanceInfoRequestConsumer : IConsumer<Batch<ProductInstanceInfoRequest>>
{
	private const string _uri = "cises/info";
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

		_logger.LogInformation("Consumer succesfully created");
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
		var currentCodesPreRequestLimit = _optionsMonitor.CurrentValue.CodesPerRequestLimit;
		var currentRequestDelay = _optionsMonitor.CurrentValue.RequestsDelay;
		
		using var apiRequestCancellationTokenSource = new CancellationTokenSource(_optionsMonitor.CurrentValue.RequestsTimeOut);

		var codes = group.Select(m => m.Message.ProductCode).ToList();

		if(!codes.Any())
		{
			return;
		}

		_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", group.Key);

		var productInstanceStatuses = new List<ProductInstanceStatus>();

		while(codes.Any())
		{
			var codesPortion = codes.Take(currentCodesPreRequestLimit).ToArray();

			var errorMessage = new StringBuilder();

			const string baseErrorMessage = "Не удалось получить данные о статусах экземпляров товаров.";

			HttpResponseMessage? response = null;

			var currentPortionContexts = group.Where(g => codesPortion.Contains(g.Message.ProductCode));

			try
			{
				response = await _httpClient.PostAsJsonAsync<IEnumerable<string>>(_uri, codesPortion, apiRequestCancellationTokenSource.Token);
			}
			catch(Exception ex)
			{
				errorMessage.AppendLine(baseErrorMessage);
				errorMessage.AppendLine($"Ошибка при получении информации из Честного знака: {ex.Message}");
				RespondError(errorMessage, currentPortionContexts);

				if(codes.Count > 0)
				{
					codes.RemoveRange(0, Math.Min(codes.Count, currentCodesPreRequestLimit));
				}

				await Task.Delay(currentRequestDelay);

				continue;
			}

			if(response is null || !response.IsSuccessStatusCode)
			{
				errorMessage.AppendLine(baseErrorMessage);
				errorMessage.AppendLine($"{response?.StatusCode} {response?.ReasonPhrase}");

				RespondError(errorMessage, currentPortionContexts);

				if(codes.Count > 0)
				{
					codes.RemoveRange(0, Math.Min(codes.Count, currentCodesPreRequestLimit));
				}

				await Task.Delay(currentRequestDelay);

				continue;
			}

			string responseBody = await response.Content.ReadAsStringAsync();

			IEnumerable<CisInfoRoot> cisesInformations = Enumerable.Empty<CisInfoRoot>();

			try
			{
				cisesInformations = JsonSerializer.Deserialize<IEnumerable<CisInfoRoot>>(responseBody) ?? Enumerable.Empty<CisInfoRoot>();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Message: {ExceptionMessage}", ex.Message);

				errorMessage.AppendLine(baseErrorMessage);
				errorMessage.AppendLine("Не удалось получить ожидаемый ответ от сервера");

				RespondError(errorMessage, currentPortionContexts);

				if(codes.Count > 0)
				{
					codes.RemoveRange(0, Math.Min(codes.Count, currentCodesPreRequestLimit));
				}

				await Task.Delay(currentRequestDelay);

				continue;
			}

			_logger.LogInformation("responseBody: {ResponseBody}", responseBody);

			if(cisesInformations is null)
			{
				errorMessage.AppendLine(baseErrorMessage);
				errorMessage.AppendLine("Не удалось получить ожидаемый ответ от сервера");

				RespondError(errorMessage, currentPortionContexts);

				if(codes.Count > 0)
				{
					codes.RemoveRange(0, Math.Min(codes.Count, currentCodesPreRequestLimit));
				}

				await Task.Delay(currentRequestDelay);

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
						Gtin = cisesInformation.CisInfo.Gtin,
						Status = cisesInformation.CisInfo.Status.ToProductInstanceStatusEnum(),
						GeneralPackageType = Enum.TryParse<GeneralPackageType>(cisesInformation.CisInfo.GeneralPackageType, true, out var generalPackageType) ? generalPackageType : null,
						PackageType = Enum.TryParse<PackageType>(cisesInformation.CisInfo.PackageType, true, out var packageType) ? packageType : null,
						Childs = cisesInformation.CisInfo.Childs ?? Enumerable.Empty<string>(),
						ParentId = cisesInformation.CisInfo.Parent,
						OwnerInn = cisesInformation.CisInfo.OwnerInn,
						OwnerName = cisesInformation.CisInfo.OwnerName
					}
				});
			}

			if(codes.Count > 0)
			{
				codes.RemoveRange(0, Math.Min(codes.Count, currentCodesPreRequestLimit));
			}

			await Task.Delay(currentRequestDelay);
		}
	}

	private static void RespondError(StringBuilder errorMessage, IEnumerable<ConsumeContext<ProductInstanceInfoRequest>> currentPortionContexts)
	{
		foreach(var context in currentPortionContexts)
		{
			context.Respond(new ProductInstanceInfoResponse
			{
				ErrorMessage = errorMessage.ToString()
			});
		}
	}
}
