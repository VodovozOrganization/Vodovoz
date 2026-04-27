using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Orders;

namespace EmailDebtNotificationWorker.Services
{
	public class EmailClaimLettersService : IEmailClaimLettersService
	{
		private readonly ILogger<EmailClaimLettersService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;

		public EmailClaimLettersService(
			ILogger<EmailClaimLettersService> logger,
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new System.ArgumentNullException(nameof(orderRepository));
		}

		public async Task SendClaimLetters(CancellationToken cancellationToken)
		{
			using var uow = _uowFactory.CreateWithoutRoot("Получение списка должников в воркере по рассылке писем о претензиях");

			var overdueDebitorsDebtData = await _orderRepository.GetCounterpartyOverdueDebtorDebtData(
				uow,
				1,
				9,
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
