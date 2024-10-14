using MassTransit;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TrueMark.Contracts;
using TrueMark.Contracts.Requests;
using TrueMark.Contracts.Responses;
using TrueMark.Contracts.Extensions;

namespace TrueMark.ProductInstanceInfoCheck.Worker;
internal class ProductInstanceInfoRequestConsumer : IConsumer<Batch<ProductInstanceInfoRequest>>
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
	}

	public async Task Consume(ConsumeContext<Batch<ProductInstanceInfoRequest>> context)
	{
		var grouped = context.Message.GroupBy(m => m.Message.Bearer);

		foreach(var group in grouped)
		{
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", group.First().Message.Bearer);

			var codes = group.SelectMany(m => m.Message.ProductCodes).ToList();

			ConsumeContext<ProductInstanceInfoRequest> currentContext = null!;

			List<ProductInstanceStatus> productInstanceStatuses = new List<ProductInstanceStatus>();

			if(!codes.Any())
			{
				continue;
			}

			while(codes.Any())
			{
				var codesPortion = codes.Take(100).ToArray();

				var errorMessage = new StringBuilder();
				errorMessage.AppendLine("Не удалось получить данные о статусах экземпляров товаров.");

				string content = JsonSerializer.Serialize(codesPortion);
				HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(_uri, httpContent);

				currentContext = group.Where(g => g.Message.ProductCodes.All(pc => codesPortion.Contains(pc))).First();
				
				if(!response.IsSuccessStatusCode)
				{
					currentContext.Respond(new ProductInstancesInfoResponse
					{
						ErrorMessage = errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}").ToString()
					});

					break;
				}

				string responseBody = await response.Content.ReadAsStringAsync();
				var cisesInformation = JsonSerializer.Deserialize<IList<CisInfoRoot>>(responseBody);
				_logger.LogInformation("responseBody: {ResponseBody}", responseBody);

				productInstanceStatuses.AddRange(cisesInformation.Select(x =>
					new ProductInstanceStatus
					{
						IdentificationCode = x.CisInfo.RequestedCis,
						Status = x.CisInfo.Status.ToProductInstanceStatusEnum(),
						OwnerInn = x.CisInfo.OwnerInn,
						OwnerName = x.CisInfo.OwnerName
					}));

				codes.RemoveRange(0, 100);

				await Task.Delay(_optionsMonitor.CurrentValue.RequestsDelay);
			}

			currentContext.Respond(new ProductInstancesInfoResponse
			{
				InstanceStatuses = new List<ProductInstanceStatus>(productInstanceStatuses)
			});
		}
	}
}
