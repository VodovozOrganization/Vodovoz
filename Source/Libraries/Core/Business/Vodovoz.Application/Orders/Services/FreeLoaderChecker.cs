using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors;
using Vodovoz.Nodes;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class FreeLoaderChecker : IFreeLoaderChecker
	{
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IFreeLoaderRepository _freeLoaderRepository;

		public FreeLoaderChecker(
			IPromotionalSetRepository promotionalSetRepository,
			IOrderRepository orderRepository,
			IFreeLoaderRepository freeLoaderRepository)
		{
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_freeLoaderRepository = freeLoaderRepository ?? throw new ArgumentNullException(nameof(freeLoaderRepository));
		}
		
		public IEnumerable<FreeLoaderInfoNode> PossibleFreeLoadersByAddress { get; private set; }
		public IEnumerable<FreeLoaderInfoNode> PossibleFreeLoadersByPhones { get; private set; }
		
		public bool CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
			IUnitOfWork uow,
			bool isSelfDelivery,
			Counterparty client,
			DeliveryPoint deliveryPoint)
		{
			if(isSelfDelivery || client is null || deliveryPoint is null)
			{
				return false;
			}
			
			return client.PersonType == PersonType.natural
				&& (deliveryPoint.RoomType == RoomType.Office || deliveryPoint.RoomType == RoomType.Store)
				&& _promotionalSetRepository.AddressHasAlreadyBeenUsedForPromoForNewClients(uow, deliveryPoint);
		}

		public bool CheckFreeLoaders(
			IUnitOfWork uow,
			int orderId,
			DeliveryPoint deliveryPoint,
			IEnumerable<Phone> phones)
		{
			if(phones == null)
			{
				throw new ArgumentNullException(nameof(phones));
			}
			
			PossibleFreeLoadersByAddress =
				deliveryPoint != null ?
					_freeLoaderRepository.GetPossibleFreeLoadersByAddress(uow, orderId, deliveryPoint)
					: Array.Empty<FreeLoaderInfoNode>();

			var phoneResultByCounterparty =
				_freeLoaderRepository.GetPossibleFreeLoadersInfoByCounterpartyPhones(uow, orderId, phones);
			
			var excludeOrderIds =
				phoneResultByCounterparty.Select(x => x.OrderId)
					.Concat(new[] { orderId });

			var phoneResultByDeliveryPoint =
				_freeLoaderRepository.GetPossibleFreeLoadersInfoByDeliveryPointPhones(uow, excludeOrderIds, phones);
			
			PossibleFreeLoadersByPhones = phoneResultByCounterparty.Concat(phoneResultByDeliveryPoint);

			return PossibleFreeLoadersByAddress.Any() || PossibleFreeLoadersByPhones.Any();
		}
		
		public Result CanOrderPromoSetForNewClientsFromOnline(
			IUnitOfWork uow,
			bool isSelfDelivery,
			int? counterpartyId,
			int? deliveryPointId)
		{
			if(isSelfDelivery)
			{
				return Result.Failure(Vodovoz.Errors.Orders.Order.UnableToShipPromoSetForNewClientsFromSelfDelivery);
			}
			
			var counterparty = uow.GetById<Counterparty>(counterpartyId ?? 0);
			var deliveryPoint = uow.GetById<DeliveryPoint>(deliveryPointId ?? 0);

			if(counterparty is null || deliveryPoint is null)
			{
				return Result.Failure(Vodovoz.Errors.Orders.Order.UnableToShipPromoSetForNewClientsToUnknownClientOrDeliveryPoint);
			}
			
			if(_orderRepository.HasCounterpartyFirstRealOrder(uow, counterparty))
			{
				return Result.Failure(Vodovoz.Errors.Orders.Order.UnableToShipPromoSet);
			}

			if(!CanOrderPromoSetForNewClientsByBuildingFiasGuid(
				   uow, isSelfDelivery, deliveryPoint.BuildingFiasGuid, deliveryPoint.Room))
			{
				return Result.Failure(Vodovoz.Errors.Orders.Order.UnableToShipPromoSet);
			}
			
			if(CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(uow, isSelfDelivery, counterparty, deliveryPoint))
			{
				return Result.Failure(Vodovoz.Errors.Orders.Order.UnableToShipPromoSet);
			}

			var phones = new List<Phone>();
			phones.AddRange(counterparty.Phones);
			phones.AddRange(deliveryPoint.Phones);

			if(CheckFreeLoaders(uow, 0, deliveryPoint, phones))
			{
				return Result.Failure(Vodovoz.Errors.Orders.Order.UnableToShipPromoSet);
			}

			return Result.Success();
		}
		
		private bool CanOrderPromoSetForNewClientsByBuildingFiasGuid(
			IUnitOfWork uow,
			bool isSelfDelivery,
			Guid? buildingFiasGuid,
			string room)
		{
			if(isSelfDelivery)
			{
				return false;
			}

			if(buildingFiasGuid is null)
			{
				return true;
			}
			
			return !_freeLoaderRepository
				.GetPossibleFreeLoadersInfoByBuildingFiasGuid(uow, buildingFiasGuid.Value, room, 0)
				.Any();
		}
	}
}
