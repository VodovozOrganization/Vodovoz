using Dadata.Model;
using QS.DomainModel.UoW;
using RevenueService.Client;
using RevenueService.Client.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Application.Services
{
	public class CounterpartyService : ICounterpartyService
	{
		private readonly IRevenueServiceClient _revenueServiceClient;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public CounterpartyService(IRevenueServiceClient revenueServiceClient, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_revenueServiceClient = revenueServiceClient ?? throw new System.ArgumentNullException(nameof(revenueServiceClient));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new System.ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public async Task StopShipmentsIfNeeded(Counterparty counterparty, Employee employee, CancellationToken cancellationToken)
		{
			if(counterparty.IsDeliveriesClosed)
			{
				return;
			}

			if(counterparty.PersonType != PersonType.legal)
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(counterparty.INN))
			{
				return;
			}

			var status = await _revenueServiceClient.GetCounterpartyStatus(counterparty.INN, cancellationToken);

			if(status != PartyStatus.ACTIVE)
			{
				counterparty.ToggleDeliveryOption(employee);
				counterparty.AddCloseDeliveryComment($"Автоматическое закрытие поставок: контрагент в статусе \"{status.GetUserFriendlyName()}\". Оформление заказа невозможно", employee);
				counterparty.IsLiquidating = true;
			}
		}

		public async Task StopShipmentsIfNeeded(int counterpartyId, int employeeId, CancellationToken cancellationToken)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateForRoot<Counterparty>(counterpartyId, "Автоматическое закрытие поставок: контрагент в статусе ликвидации"))
			{
				var employee = unitOfWork.GetById<Employee>(employeeId);

				await StopShipmentsIfNeeded(unitOfWork.Root, employee, cancellationToken);

				unitOfWork.Commit();
			}
		}
	}
}
