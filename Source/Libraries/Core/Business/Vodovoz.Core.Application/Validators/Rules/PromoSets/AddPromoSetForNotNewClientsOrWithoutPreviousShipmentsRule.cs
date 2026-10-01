using System;
using System.Linq;
using System.Text;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Rules;

namespace Vodovoz.Core.Application.Validators.Rules.PromoSets
{
	/// <summary>
	/// Правило добавления более одного промонабора для новых клиентов
	/// </summary>
	public class AddPromoSetForNotNewClientsOrWithoutPreviousShipmentsRule : IAddPromoSetRule
	{
		private readonly IPromotionalSetRepository _promotionalSetRepository;

		public AddPromoSetForNotNewClientsOrWithoutPreviousShipmentsRule(IPromotionalSetRepository promotionalSetRepository)
		{
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
		}
		
		public Result<string> Apply(IUnitOfWork uow, ISaleSource source, PromotionalSet proSet)
		{
			var questionMessage = string.Empty;
			
			if(!proSet.PromotionalSetForNewClients)
			{
				return Result.Success(questionMessage);
			}
			
			var proSetDict =
				_promotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(uow, source);

			if(!proSetDict.Any())
			{
				return Result.Success(questionMessage);
			}
			
			var address = string.Join(
				", ",
				source.DeliveryPoint.City,
				source.DeliveryPoint.Street,
				source.DeliveryPoint.Building,
				source.DeliveryPoint.Room);
			
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
			questionMessage = sb.ToString();

			return Result.Success(questionMessage);
		}
	}
}
