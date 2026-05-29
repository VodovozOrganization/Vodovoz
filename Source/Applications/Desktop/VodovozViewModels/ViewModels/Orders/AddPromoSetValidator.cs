using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Errors.PromoSets;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class AddPromoSetValidator
	{
		private readonly IFreeLoaderChecker _freeLoaderChecker;
		private readonly IPromotionalSetRepository _promotionalSetRepository;

		public AddPromoSetValidator(
			IFreeLoaderChecker freeLoaderChecker,
			IPromotionalSetRepository promotionalSetRepository
			)
		{
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
		}
		
		public virtual Result Validate(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			PromotionalSet proSet)
		{
			/*if(PromotionalSets.Any(x => x.PromotionalSetForNewClients && proSet.PromotionalSetForNewClients))
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"В заказ нельзя добавить два промо-набора для новых клиентов");
				return false;
			}*/

			if(addProductSource.IsSelfDelivery)
			{
				return Result.Success();
			}

			var deliveryPoint = addProductSource.DeliveryPoint;
			
			if(proSet.PromotionalSetForNewClients
				&& _freeLoaderChecker.CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
					uow,
					addProductSource.IsSelfDelivery,
					addProductSource.Counterparty,
					deliveryPoint))
			{
				return Result.Failure(PromoSetErrors.HasPreviousShipmentToAnotherIndividualClient);
			}

			var proSetDict = _promotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(uow);

			if(!proSet.PromotionalSetForNewClients | !proSetDict.Any())
			{
				return Result.Success();
			}

			var address = string.Join(", ", deliveryPoint.City, deliveryPoint.Street, deliveryPoint.Building, deliveryPoint.Room);
			var sb = new StringBuilder(
				$"Для адреса \"{address}\", найдены схожие точки доставки, на которые уже создавались заказы с промо-наборами:\n");
		
			foreach(var d in proSetDict)
			{
				var proSetTitle = uow.GetById<PromotionalSet>(d.Key).ShortTitle;
				var orders = string.Join(
					" ,",
					uow.GetById<Order>(d.Value).Select(o => o.Title)
				);
				sb.AppendLine($"– {proSetTitle}: {orders}");
			}
		
			sb.AppendLine($"Вы уверены, что хотите добавить \"{proSet.Title}\"");
		
			return _commonServices.InteractiveService.Question(sb.ToString());
		}
	}
}
