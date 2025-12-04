using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Nodes;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class FreeLoaderChecker : IFreeLoaderChecker
	{
		private readonly ILogger<FreeLoaderChecker> _logger;
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IFreeLoaderRepository _freeLoaderRepository;

		public FreeLoaderChecker(
			ILogger<FreeLoaderChecker> logger,
			IPromotionalSetRepository promotionalSetRepository,
			IOrderRepository orderRepository,
			IFreeLoaderRepository freeLoaderRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
			IEnumerable<string> phoneNumbers,
			bool? promoSetForNewClients = null)
		{
			if(phoneNumbers == null)
			{
				throw new ArgumentNullException(nameof(phoneNumbers));
			}
			
			PossibleFreeLoadersByAddress =
				deliveryPoint != null ?
					_freeLoaderRepository.GetPossibleFreeLoadersByAddress(uow, orderId, deliveryPoint, promoSetForNewClients)
					: Array.Empty<FreeLoaderInfoNode>();

			var phoneResultByCounterparty =
				_freeLoaderRepository.GetPossibleFreeLoadersInfoByCounterpartyPhones(uow, orderId, phoneNumbers);
			
			var excludeOrderIds =
				phoneResultByCounterparty.Select(x => x.OrderId)
					.Concat(new[] { orderId });

			var phoneResultByDeliveryPoint =
				_freeLoaderRepository.GetPossibleFreeLoadersInfoByDeliveryPointPhones(uow, excludeOrderIds, phoneNumbers);
			
			PossibleFreeLoadersByPhones = phoneResultByCounterparty.Concat(phoneResultByDeliveryPoint);

			return PossibleFreeLoadersByAddress.Any() || PossibleFreeLoadersByPhones.Any();
		}
		
		public Result CanOrderPromoSetForNewClientsFromOnline(
			IUnitOfWork uow,
			bool isSelfDelivery,
			int? counterpartyId,
			int? deliveryPointId,
			string digitsNumber = null)
		{
			if(isSelfDelivery)
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsFromSelfDelivery");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSetForNewClientsFromSelfDelivery);
			}
			
			var counterparty = uow.GetById<Counterparty>(counterpartyId ?? 0);
			var deliveryPoint = uow.GetById<DeliveryPoint>(deliveryPointId ?? 0);

			if(counterparty is null || deliveryPoint is null)
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsToUnknownClientOrDeliveryPoint");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSetForNewClientsToUnknownClientOrDeliveryPoint);
			}
			
			if(_orderRepository.HasCounterpartyFirstRealOrder(uow, counterparty))
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsToCounterpartyWithRealOrder");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSet);
			}

			if(!CanOrderPromoSetForNewClientsByBuildingFiasGuid(
				uow, isSelfDelivery, deliveryPoint.BuildingFiasGuid, deliveryPoint.Room))
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsByBuildingFiasGuid");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSet);
			}
			
			if(CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(uow, isSelfDelivery, counterparty, deliveryPoint))
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsByNaturalClientToOfficeOrStore");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSet);
			}

			var phones = new List<string>();
			phones.AddRange(counterparty.Phones.Select(x => x.DigitsNumber));
			phones.AddRange(deliveryPoint.Phones.Select(x => x.DigitsNumber));

			if(!string.IsNullOrWhiteSpace(digitsNumber))
			{
				phones.Add(digitsNumber);
			}

			if(CheckFreeLoaders(uow, 0, deliveryPoint, phones, true))
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsCheckingByPhones");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSet);
			}
			
			if(HasOnlineOrderWithPromoSetForNewClient(uow, deliveryPoint.Id))
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsToClientWithOnlineOrderWithThem");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSet);
			}

			return Result.Success();
		}

		private bool HasOnlineOrderWithPromoSetForNewClient(
			IUnitOfWork uow,
			int deliveryPointId)
		{
			return _freeLoaderRepository.HasOnlineOrderWithPromoSetForNewClients(uow, deliveryPointId);
		}

		private bool CanOrderPromoSetForNewClientsByBuildingFiasGuid(
			IUnitOfWork uow,
			bool isSelfDelivery,
			Guid? buildingFiasGuid,
			string room,
			bool promoSetForNewClients = true)
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
				.GetPossibleFreeLoadersInfoByBuildingFiasGuid(uow, buildingFiasGuid.Value, room, 0, promoSetForNewClients)
				.Any();
		}
	}
}
