using Edo.Withdrawal.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using RTools_NTS.Util;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Withdrawal
{
	public class WithdrawalTaskCreatedHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<WithdrawalTaskCreatedHandler> _logger;
		private readonly IOptions<TrueMarkSettings> _trueMarkSettitnsOptions;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public WithdrawalTaskCreatedHandler(
			ILogger<WithdrawalTaskCreatedHandler> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<TrueMarkSettings> trueMarkSettitnsOptions,
			ITrueMarkSett)
		{
			_logger =
				logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkSettitnsOptions = trueMarkSettitnsOptions ?? throw new ArgumentNullException(nameof(trueMarkSettitnsOptions));
			_uow =
				(unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot(nameof(WithdrawalTaskCreatedHandler));
		}

		public async Task HandleWithdrawal(int withdrawalEdoTaskId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var withdrawalEdoTask = await _uow.Session.GetAsync<WithdrawalEdoTask>(withdrawalEdoTaskId, cancellationToken);

			if(withdrawalEdoTask is null)
			{
				throw new InvalidOperationException($"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} не найдена");
			}

			if(withdrawalEdoTask.Status == EdoTaskStatus.Completed)
			{
				throw new InvalidOperationException($"Задача {nameof(WithdrawalEdoTask)} с Id {withdrawalEdoTaskId} уже завершена");
			}

			var codes = withdrawalEdoTask.Items.Select(x => x.ProductCode).ToList();

			using(var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				httpClient.BaseAddress = new Uri(_trueMarkSettitnsOptions.Value.ExternalTrueMarkBaseUrl);
			}

				withdrawalEdoTask.Status = EdoTaskStatus.Completed;
			withdrawalEdoTask.EndTime = DateTime.Now;

			await _uow.SaveAsync(withdrawalEdoTask, cancellationToken: cancellationToken);
			_uow.Commit();
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
