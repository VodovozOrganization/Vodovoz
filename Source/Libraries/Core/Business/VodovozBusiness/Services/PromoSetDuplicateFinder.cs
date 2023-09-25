using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods.PromotionalSets;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Services
{
	public class PromoSetDuplicateFinder : IPromoSetDuplicateFinder
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IPromotionalSetRepository _promotionalSetRepository;

		public PromoSetDuplicateFinder(
			IInteractiveService interactiveService,
			IPromotionalSetRepository promotionalSetRepository)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
		}

		public bool CheckDuplicatePromoSets(IUnitOfWork uow, DeliveryPoint deliveryPoint, IEnumerable<Phone> phones)
		{
			if(phones == null)
			{
				throw new ArgumentNullException(nameof(phones));
			}

			IEnumerable<PromoSetDuplicateInfoNode> deliveryPointResult = new List<PromoSetDuplicateInfoNode>();
			if(deliveryPoint != null)
			{
				deliveryPointResult = GetDeliveryPointResult(uow, deliveryPoint);
			}
			var counterpartyPhoneResult = GetPhonesResultByCounterparty(uow, phones);
			var deliveryPointPhoneResult = GetPhonesResultByDeliveryPoint(uow, phones);
			var phoneResult =
				counterpartyPhoneResult.Except(deliveryPointPhoneResult, new PromoSetDuplicateInfoComparer());

			if(!deliveryPointResult.Any() && !phoneResult.Any())
			{
				return true;
			}

			var message = $"Найдены проданные промо-наборы по аналогичному адресу/телефону:{Environment.NewLine}";
			var counter = 1;
			foreach(var r in deliveryPointResult)
			{
				var date = r.Date.HasValue ? r.Date.Value.ToString("dd.MM.yyyy") + ", " : "";
				message += $"{counter}. {date}{r.Client}, {r.Address}, {r.Phone}{Environment.NewLine}";
				counter++;
			}

			foreach(var r in phoneResult)
			{
				var date = r.Date.HasValue ? r.Date.Value.ToString("dd.MM.yyyy") + ", " : "";
				message += $"{counter}. {date}{r.Client}, {r.Address}, {r.Phone}{Environment.NewLine}";
				counter++;
			}
			
			foreach(var r in phoneResult)
			{
				var date = r.Date.HasValue ? r.Date.Value.ToString("dd.MM.yyyy") + ", " : "";
				message += $"{counter}. {date}{r.Client}, {r.Address}, {r.Phone}{Environment.NewLine}";
				counter++;
			}
			message += "Продолжить сохранение?";

			return _interactiveService.Question(message, "Найдены проданные промонаборы!");
		}

		private IEnumerable<PromoSetDuplicateInfoNode> GetDeliveryPointResult(IUnitOfWork uow, DeliveryPoint deliveryPoint)
			=> _promotionalSetRepository.GetPromoSetDuplicateInfoByAddress(uow, deliveryPoint);

		private IEnumerable<PromoSetDuplicateInfoNode> GetPhonesResultByCounterparty(IUnitOfWork uow, IEnumerable<Phone> phones)
			=> _promotionalSetRepository.GetPromoSetDuplicateInfoByCounterpartyPhones(uow, phones);

		private IEnumerable<PromoSetDuplicateInfoNode> GetPhonesResultByDeliveryPoint(IUnitOfWork uow, IEnumerable<Phone> phones)
			=> _promotionalSetRepository.GetPromoSetDuplicateInfoByDeliveryPointPhones(uow, phones);
	}
}
