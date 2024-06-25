using NHibernate;
using QS.DomainModel.UoW;
using QS.Utilities.Debug;
using RoboatsService.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Roboats;

namespace RoboatsService.OrderValidation
{
	public class ValidOrdersProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly IRoboatsSettings _roboatsSettings;
		private readonly RoboatsCallBatchRegistrator _roboatsCallRegistrator;

		public ValidOrdersProvider(
			IUnitOfWorkFactory uowFactory,
			INomenclatureSettings nomenclatureSettings,
			IRoboatsRepository roboatsRepository,
			IRoboatsSettings roboatsSettings,
			RoboatsCallBatchRegistrator roboatsCallRegistrator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_roboatsSettings = roboatsSettings ?? throw new ArgumentNullException(nameof(roboatsSettings));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new ArgumentNullException(nameof(roboatsCallRegistrator));
		}

		public IEnumerable<int> GetLastDeliveryPointIds(string clientPhone, Guid callGuid, int counterpartyId, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			var lastOrders = _roboatsRepository.GetLastOrders(counterpartyId).OrderByDescending(x => x.Id);
			var validOrders = GetValidLastOrders(clientPhone, callGuid, counterpartyId, lastOrders, roboatsCallFailType, roboatsCallOperation);
			var deliveryPointIds = validOrders.Where(x => x.DeliveryPoint != null).Select(o => o.DeliveryPoint.Id);
			return deliveryPointIds;
		}

		public Order GetLastOrder(string clientPhone, Guid callGuid, int counterpartyId, int deliveryPointId, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			var lastOrder = _roboatsRepository.GetLastOrder(counterpartyId, deliveryPointId);

			var validOrders = GetValidLastOrders(clientPhone, callGuid, counterpartyId, new[] { lastOrder }, roboatsCallFailType, roboatsCallOperation);
			return validOrders.FirstOrDefault();
		}

		private IEnumerable<Order> GetValidLastOrders(string clientPhone, Guid callGuid, int counterpartyId, IEnumerable<Order> orders, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			try
			{
				return InvokeGetValidLastOrders(clientPhone, callGuid, counterpartyId, orders, roboatsCallFailType, roboatsCallOperation);
			}
			catch (Exception ex)
			{
				var lazyEx = ExceptionHelper.FindExceptionTypeInInner<LazyInitializationException>(ex);
				var message = $"При обращении к не инициализированным полям в сущностях из переданной коллекции {nameof(orders)} " +
					$"возникло исключение {nameof(LazyInitializationException)}. В сущностях передаваемых в {nameof(ValidOrdersProvider)} " +
					$"должны быть уже загружены данные используемые валидаторами, как в примере в {nameof(IRoboatsRepository)}.{nameof(IRoboatsRepository.GetLastOrders)}.";
				throw new LazyInitializationException(message, lazyEx);
			}
		}

		private IEnumerable<Order> InvokeGetValidLastOrders(string clientPhone, Guid callGuid, int counterpartyId, IEnumerable<Order> orders, RoboatsCallFailType roboatsCallFailType, RoboatsCallOperation roboatsCallOperation)
		{
			using var uow = _uowFactory.CreateWithoutRoot();
			if(!orders.Any())
			{
				_roboatsCallRegistrator.RegisterFail(uow, clientPhone, callGuid, roboatsCallFailType, roboatsCallOperation,
					$"У контрагента {counterpartyId} нет заказов");
			}
			else
			{
				var multiValidator = new OrderMultiValidator();
				multiValidator.AddValidator(new StatusOrderValidator());
				multiValidator.AddValidator(new BottleStockOrderValidator());
				multiValidator.AddValidator(new DateOrderValidator(_roboatsSettings));
				multiValidator.AddValidator(new PromosetOrderValidator());
				multiValidator.AddValidator(new FiasStreetOrderValidator(_roboatsRepository));
				multiValidator.AddValidator(new OnlyWaterOrderValidator(_nomenclatureSettings));
				multiValidator.AddValidator(new RoboatsWaterOrderValidator(_roboatsRepository));
				multiValidator.AddValidator(new WaterRowDuplicateOrderValidator());
				multiValidator.AddValidator(new ReasonForLeavingValidator());
				multiValidator.AddValidator(new ActiveDeliveryPointOrderValidator());

				var result = multiValidator.ValidateOrders(orders);
				if(result.HasValidOrders)
				{
					return result.ValidOrders;
				}
				else
				{
					foreach(var problemMessage in result.ProblemMessages)
					{
						_roboatsCallRegistrator.RegisterFail(uow, clientPhone, callGuid, roboatsCallFailType, roboatsCallOperation, problemMessage);
					}
				}
			}

			_roboatsCallRegistrator.AbortCall(uow, clientPhone, callGuid);
			uow.Commit();

			return Enumerable.Empty<Order>();
		}
	}
}
