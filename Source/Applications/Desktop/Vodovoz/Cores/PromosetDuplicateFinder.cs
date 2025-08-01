using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using QS.Dialog;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Cores
{
	public class PromosetDuplicateFinder
	{
		private readonly IFreeLoaderChecker _freeLoaderChecker;
		private readonly IInteractiveService _interactiveService;

		public PromosetDuplicateFinder(
			IFreeLoaderChecker freeLoaderChecker,
			IInteractiveService interactiveService)
		{
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public bool RequestDuplicatePromosets(
			IUnitOfWork uow,
			int orderId,
			DeliveryPoint deliveryPoint,
			IEnumerable<string> phoneNumbers)
		{
			var result = _freeLoaderChecker.CheckFreeLoaders(uow, orderId, deliveryPoint, phoneNumbers);

			if(!result)
			{
				return true;
			}

			string message = $"Найдены проданные промонаборы по аналогичному адресу/телефону:{Environment.NewLine}";
			int counter = 1;
			foreach(var r in _freeLoaderChecker.PossibleFreeLoadersByAddress) {
				string date = r.Date.HasValue ? r.Date.Value.ToString("dd.MM.yyyy") + ", " : "";
				message += $"{counter}. {date}{r.Client}, {r.Address}, {r.Phone}{Environment.NewLine}";
				counter++;
			}

			foreach(var r in _freeLoaderChecker.PossibleFreeLoadersByPhones) {
				string date = r.Date.HasValue ? r.Date.Value.ToString("dd.MM.yyyy") + ", " : "";
				message += $"{counter}. {date}{r.Client}, {r.Address}, {r.Phone}{Environment.NewLine}";
				counter++;
			}
			message += $"Продолжить сохранение?";

			return _interactiveService.Question(message, "Найдены проданные промонаборы!");
		}
	}
}
