using EmailDebtNotificationWorker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace EmailDebtNotificationWorker.Services
{
	public class EmailClaimLettersService : IEmailClaimLettersService
	{
		private readonly ILogger<EmailClaimLettersService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IOptionsMonitor<EmailClaimLettersOptions> _emailClaimLettersOptions;
		private readonly OrderStatus[] _orderStatuses =
			new[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

		private readonly RevenueStatus[] _excludeCounterpartyRevenueStatuses =
			new[] { RevenueStatus.Liquidating, RevenueStatus.Liquidated, RevenueStatus.Reorganizing, RevenueStatus.Bankrupt };

		public EmailClaimLettersService(
			ILogger<EmailClaimLettersService> logger,
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository,
			IOptionsMonitor<EmailClaimLettersOptions> emailClaimLettersOptions)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new System.ArgumentNullException(nameof(orderRepository));
			_emailClaimLettersOptions = emailClaimLettersOptions ?? throw new System.ArgumentNullException(nameof(emailClaimLettersOptions));
		}

		public async Task SendClaimLetters(CancellationToken cancellationToken)
		{
			using var uow = _uowFactory.CreateWithoutRoot("Получение списка должников в воркере по рассылке писем о претензиях");

			var overdueDebitorsDebtData = await _orderRepository.GetCounterpartyOverdueDebtorDebtData(
				uow,
				_emailClaimLettersOptions.CurrentValue.OverdueDebtorDebtExpiredDaysAgo,
				_orderStatuses,
				_excludeCounterpartyRevenueStatuses,
				cancellationToken);

			if(overdueDebitorsDebtData.Count == 0)
			{
				_logger.LogDebug("Нет писем для массовой рассылки");
				return;
			}

			await Task.CompletedTask;
		}
	}
}
