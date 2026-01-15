using CashReceiptApi.Options;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.CashReceipt;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace CashReceiptApi.HealthChecks
{
	public class CashReceiptApiHealthChecks : VodovozHealthCheckBase
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger;
		private readonly IOptions<ServiceOptions> _serviceOptions;
		private readonly ICashReceiptSettings _cashReceiptSettings;

		public CashReceiptApiHealthChecks(
			ILogger<VodovozHealthCheckBase> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<ServiceOptions> serviceOptions,
			ICashReceiptSettings cashReceiptSettings)
			: base(logger, unitOfWorkFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceOptions = serviceOptions ?? throw new ArgumentNullException(nameof(serviceOptions));
			_cashReceiptSettings = cashReceiptSettings ?? throw new ArgumentNullException(nameof(cashReceiptSettings));
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Поступил запрос на информацию о работоспособности.");

			var handler = new GrpcWebHandler(new HttpClientHandler());

			try
			{
				using var httpClient = new HttpClient(handler);

				httpClient.DefaultRequestHeaders.Add("ApiKey", _cashReceiptSettings.CashReceiptApiKey);

				var options = new GrpcChannelOptions();
				options.HttpClient = httpClient;

				using var channel = GrpcChannel.ForAddress(_cashReceiptSettings.CashReceiptApiUrl, options);

				var grpcChannel = channel;
				var client =
					new CashReceiptApiHealthCheck.CashReceiptServiceGrpc.CashReceiptServiceGrpcClient(channel);

				var request = new CashReceiptApiHealthCheck.RefreshReceiptRequest
				{
					CashReceiptId = _serviceOptions.Value.HealthCheckCashReceiptId
				};

				var response = client.RefreshFiscalDocument(request);

				_logger.LogInformation("Проверка работоспособности выполнена успешно");

				return await Task.FromResult(new VodovozHealthResultDto { IsHealthy = response.IsSuccess });
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, "Непредвиденная ошибка при проверке доступности службы");

				return new VodovozHealthResultDto { IsHealthy = false };
			}
		}
	}
}
