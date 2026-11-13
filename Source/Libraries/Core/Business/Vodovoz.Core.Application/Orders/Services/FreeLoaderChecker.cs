using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Nodes;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class FreeLoaderChecker : IFreeLoaderChecker
	{
		private readonly ILogger<FreeLoaderChecker> _logger;
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IFreeLoaderRepository _freeLoaderRepository;
		private readonly IBottlesRepository _bottlesRepository;

		public FreeLoaderChecker(
			ILogger<FreeLoaderChecker> logger,
			IPromotionalSetRepository promotionalSetRepository,
			IFreeLoaderRepository freeLoaderRepository,
			IBottlesRepository bottlesRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_freeLoaderRepository = freeLoaderRepository ?? throw new ArgumentNullException(nameof(freeLoaderRepository));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
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

			var phones = new List<string>();

			if(counterparty != null)
			{
				phones.AddRange(counterparty.Phones.Select(x => x.DigitsNumber));
			}

			if(deliveryPoint != null)
			{
				phones.AddRange(deliveryPoint.Phones.Select(x => x.DigitsNumber));
			}

			if(!string.IsNullOrWhiteSpace(digitsNumber))
			{
				phones.Add(digitsNumber);
			}

			phones = phones
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToList();
			
			// 2.6: ФИО и телефон найдены, адрес доставки не найден, есть долги по таре
			if(IsFreeLoaderByFioAndPhoneWithoutAddressWithBottlesDebt(uow, counterparty, deliveryPoint, phones))
			{
				_logger.LogInformation("UnableToShipPromoSetForNewClientsByFioAndPhoneWithoutAddressWithBottlesDebt");
				return Result.Failure(Vodovoz.Errors.Orders.OrderErrors.UnableToShipPromoSet);
			}

			return Result.Success();
		}

		/// <summary>
		/// Проверка условия 2.6: ФИО и телефон найдены в системе, адрес доставки не найден,
		/// по клиенту есть долги по таре - промонаборы недоступны
		/// </summary>
		private bool IsFreeLoaderByFioAndPhoneWithoutAddressWithBottlesDebt(
			IUnitOfWork uow,
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			IEnumerable<string> phoneNumbers)
		{
			if(!IsCounterpartyFioFilled(counterparty))
			{
				return false;
			}

			if(deliveryPoint != null)
			{
				return false;
			}

			if(!phoneNumbers.Any(phone => _freeLoaderRepository.PhoneExists(uow, phone)))
			{
				return false;
			}

			return _bottlesRepository.GetBottlesDebtAtCounterparty(uow, counterparty.Id) > 0;
		}

		private static bool IsCounterpartyFioFilled(Counterparty counterparty)
		{
			if(counterparty is null || counterparty.PersonType != PersonType.natural)
			{
				return false;
			}

			return !string.IsNullOrWhiteSpace(counterparty.Name)
				|| !string.IsNullOrWhiteSpace(counterparty.Surname)
				|| !string.IsNullOrWhiteSpace(counterparty.FirstName);
		}

	}
}
