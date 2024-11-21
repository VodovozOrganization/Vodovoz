using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Nodes;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class FreeLoaderChecker : IFreeLoaderChecker
	{
		private readonly IPromotionalSetRepository _promotionalSetRepository;
		private readonly IFreeLoaderRepository _freeLoaderRepository;

		public FreeLoaderChecker(
			IPromotionalSetRepository promotionalSetRepository,
			IFreeLoaderRepository freeLoaderRepository)
		{
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_freeLoaderRepository = freeLoaderRepository ?? throw new ArgumentNullException(nameof(freeLoaderRepository));
		}
		
		public IEnumerable<FreeLoaderInfoNode> PossibleFreeLoadersByAddress { get; private set; }
		public IEnumerable<FreeLoaderInfoNode> PossibleFreeLoadersByPhones { get; private set; }
		
		public virtual bool CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
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
	}
}
