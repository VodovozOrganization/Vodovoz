using System;
using System.Linq;
using CustomerOrders.Contracts.V8.Carts;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Factories.DeliveryConditions;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <inheritdoc/>
	public class DeliveryRulesConditionsCreator : IDeliveryRulesConditionsCreator
	{
		private readonly IAdditionalConditionsFactory _additionalConditionsFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;

		public DeliveryRulesConditionsCreator(
			IAdditionalConditionsFactory additionalConditionsFactory,
			IOrderRepository orderRepository,
			IGenericRepository<Counterparty> counterpartyRepository
			)
		{
			_additionalConditionsFactory = additionalConditionsFactory ?? throw new ArgumentNullException(nameof(additionalConditionsFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
		}
		
		/// <inheritdoc/>
		public DeliveryRulesConditions Create(IUnitOfWork uow, OrderConditionsRequest request)
		{
			var client = request.ErpCounterpartyId.HasValue
				? _counterpartyRepository.Get(uow, c => c.Id == request.ErpCounterpartyId.Value).FirstOrDefault()
				: null;
			
			var hasRealOrder = _orderRepository.HasCounterpartyFirstRealOrder(uow, client);

			if(!hasRealOrder)
			{
				return DeliveryRulesConditions.Create(
					_additionalConditionsFactory.CreateForNewClient(),
					request.IsSelfDelivery);
			}

			return DeliveryRulesConditions.Create(
				_additionalConditionsFactory.CreateDefault(),
				request.IsSelfDelivery);
		}
	}
}
