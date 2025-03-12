using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Withdrawal
{
	public class WithdrawalTaskCreatedHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<WithdrawalTaskCreatedHandler> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public WithdrawalTaskCreatedHandler(
			ILogger<WithdrawalTaskCreatedHandler> logger,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger =
				logger ?? throw new ArgumentNullException(nameof(logger));
			_uow =
				(unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot(nameof(WithdrawalTaskCreatedHandler));
		}

		public async Task HandleWithdrawal(int withdrawalEdoTaskId, CancellationToken cancellationToken)
		{
			var withdrawalEdoTask = await _uow.Session.GetAsync<WithdrawalEdoTask>(withdrawalEdoTaskId, cancellationToken);
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
