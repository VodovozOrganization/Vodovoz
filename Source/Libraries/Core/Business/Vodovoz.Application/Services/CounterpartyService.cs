using Dadata.Model;
using QS.DomainModel.UoW;
using RevenueService.Client;
using RevenueService.Client.Extensions;
using System;
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
			_revenueServiceClient = revenueServiceClient ?? throw new ArgumentNullException(nameof(revenueServiceClient));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public async Task StopShipmentsIfNeeded(Counterparty counterparty, Employee employee, CancellationToken cancellationToken)
		{
			if(counterparty.IsLiquidating
				|| counterparty.IsDeliveriesClosed
				|| counterparty.PersonType != PersonType.legal
				|| string.IsNullOrWhiteSpace(counterparty.INN))
			{
				return;
			}

			var status = await _revenueServiceClient.GetCounterpartyStatus(counterparty.INN, cancellationToken);

			if(status != PartyStatus.ACTIVE)
			{
				counterparty.ToggleDeliveryOption(employee);
				counterparty.AddCloseDeliveryComment($"Автоматическое закрытие поставок: контрагент в статусе \"{status.GetUserFriendlyName()}\" в ФНС. Оформление заказа невозможно", employee);
				counterparty.IsLiquidating = true;
			}
		}

		public async Task StopShipmentsIfNeeded(int counterpartyId, int employeeId, CancellationToken cancellationToken)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateForRoot<Counterparty>(counterpartyId, "Автоматическое закрытие поставок: контрагент в статусе ликвидации"))
			{
				var employee = unitOfWork.GetById<Employee>(employeeId);

				await StopShipmentsIfNeeded(unitOfWork.Root, employee, cancellationToken);

				unitOfWork.Save();
			}
		}
	}
}
