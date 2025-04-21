using Edo.Withdrawal.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Withdrawal
{
	public class WithdrawalTaskCreatedHandler
	{
		private readonly ILogger<WithdrawalTaskCreatedHandler> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly IOptions<TrueMarkOptions> _options;

		public WithdrawalTaskCreatedHandler(
			ILogger<WithdrawalTaskCreatedHandler> logger,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory uowFactory,
			ITrueMarkApiClient trueMarkApiClient,
			IOptions<TrueMarkOptions> trueMarkOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_options = trueMarkOptions;
		}

		public async Task HandleWithdrawal(int withdrawalEdoTaskId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var withdrawalEdoTask = await uow.Session.GetAsync<WithdrawalEdoTask>(withdrawalEdoTaskId, cancellationToken);

				if(withdrawalEdoTask is null)
				{
					throw new InvalidOperationException($"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} не найдена");
				}

				if(withdrawalEdoTask.Status == EdoTaskStatus.Completed)
				{
					throw new InvalidOperationException($"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} уже завершена");
				}

				var codes = withdrawalEdoTask.Items.Select(x => x.ProductCode).ToList();

				var firstOrganizationData = _options.Value.OrganizationCertificates.FirstOrDefault();

				var crptToken = _trueMarkApiClient.GetCrptTokenAsync(firstOrganizationData.CertificateThumbPrint, firstOrganizationData.Inn, cancellationToken);

				withdrawalEdoTask.Status = EdoTaskStatus.Completed;
				withdrawalEdoTask.EndTime = DateTime.Now;

				await uow.SaveAsync(withdrawalEdoTask, cancellationToken: cancellationToken);
				uow.Commit();
			}
		}
	}
}
