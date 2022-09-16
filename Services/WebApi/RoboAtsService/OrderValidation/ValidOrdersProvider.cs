using NHibernate;
using QS.ErrorReporting;
using RoboAtsService.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Services;

namespace RoboAtsService.OrderValidation
{
	public class ValidOrdersProvider
	{
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly RoboatsCallRegistrator _roboatsCallRegistrator;

		public ValidOrdersProvider(INomenclatureParametersProvider nomenclatureParametersProvider, IRoboatsRepository roboatsRepository, RoboatsCallRegistrator roboatsCallRegistrator)
		{
			_nomenclatureParametersProvider = nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new ArgumentNullException(nameof(roboatsCallRegistrator));
		}

		public IEnumerable<int> GetLastDeliveryPointIds(string clientPhone, int counterpartyId, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			var lastOrders = _roboatsRepository.GetLastOrders(counterpartyId).OrderByDescending(x => x.Id);
			var validOrders = GetValidLastOrders(clientPhone, counterpartyId, lastOrders, roboatsCallFailType, roboatsCallOperation);
			var deliveryPointIds = validOrders.Where(x => x.DeliveryPoint != null).Select(o => o.DeliveryPoint.Id);
			return deliveryPointIds;
		}

		public Order GetLastOrder(string clientPhone, int counterpartyId, int deliveryPointId, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			var lastOrder = _roboatsRepository.GetLastOrder(counterpartyId, deliveryPointId);

			var validOrders = GetValidLastOrders(clientPhone, counterpartyId, new[] { lastOrder }, roboatsCallFailType, roboatsCallOperation);
			return validOrders.FirstOrDefault();
		}

		private IEnumerable<Order> GetValidLastOrders(string clientPhone, int counterpartyId, IEnumerable<Order> orders, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			try
			{
				return InvokeGetValidLastOrders(clientPhone, counterpartyId, orders, roboatsCallFailType, roboatsCallOperation);
			}
			catch (Exception ex)
			{
				var lazyEx = ExceptionHelper.FindExceptionTypeInInner<LazyInitializationException>(ex);
				var message = $"При обращении к не инициализированным полям в сущностях из переданной коллекции {nameof(orders)} " +
					$"возникло исключение {nameof(LazyInitializationException)}. В сущностях передаваемых в {nameof(ValidOrdersProvider)} " +
					$"должны быть уже загружены данные используемые валидаторами, как в примере в {nameof(RoboatsRepository)}.{nameof(RoboatsRepository.GetLastOrders)}.";
				throw new LazyInitializationException(message, lazyEx);
			}
		}

		private IEnumerable<Order> InvokeGetValidLastOrders(string clientPhone, int counterpartyId, IEnumerable<Order> orders, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			if(!orders.Any())
			{
				_roboatsCallRegistrator.RegisterFail(clientPhone, roboatsCallFailType, roboatsCallOperation,
					$"У контрагента {counterpartyId} нет заказов");
			}
			else
			{
				var multiValidator = new OrderMultiValidator();
				multiValidator.AddValidator(new StatusOrderValidator());
				multiValidator.AddValidator(new BottleStockOrderValidator());
				multiValidator.AddValidator(new DateOrderValidator());
				multiValidator.AddValidator(new PromosetOrderValidator());
				multiValidator.AddValidator(new FiasStreetOrderValidator(_roboatsRepository));
				multiValidator.AddValidator(new ApartmentOrderValidator());
				multiValidator.AddValidator(new OnlyWaterOrderValidator(_nomenclatureParametersProvider));
				multiValidator.AddValidator(new RoboatsWaterOrderValidator(_roboatsRepository));
				multiValidator.AddValidator(new WaterRowDuplicateOrderValidator());


				var result = multiValidator.ValidateOrders(orders);
				if(result.HasValidOrders)
				{
					return result.ValidOrders;
				}
				else
				{
					foreach(var problemMessage in result.ProblemMessages)
					{
						_roboatsCallRegistrator.RegisterFail(clientPhone, roboatsCallFailType, roboatsCallOperation, problemMessage);
					}
				}
			}

			_roboatsCallRegistrator.AbortCall(clientPhone);
			return Enumerable.Empty<Order>();
		}
	}
}
