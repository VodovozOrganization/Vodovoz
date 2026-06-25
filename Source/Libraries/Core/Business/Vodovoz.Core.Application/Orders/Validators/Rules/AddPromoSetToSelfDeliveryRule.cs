using System;
using System.Linq;
using System.Text;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Errors.PromoSets;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Validators.Rules
{
	public interface IAddPromoSetRule
	{
		Result<string> Validate(IUnitOfWork uow, IAddSaleItemSource source, PromotionalSet proSet);
	}
	
	/// <summary>
	/// Правило добавления более одного промонабора для новых клиентов
	/// </summary>
	public class AddMoreOnePromoSetForNewClientsToOrderRule : IAddPromoSetRule
	{
		public Result<string> Validate(IUnitOfWork uow, IAddSaleItemSource saleItemSource, PromotionalSet proSet)
		{
			var order = saleItemSource.Source as Order;
			
			if(order.PromotionalSets.Any(x => x.PromotionalSetForNewClients && proSet.PromotionalSetForNewClients))
			{
				return Result.Failure<string>(PromoSetErrors.CantAddTwoPromoSetsForNewClients);
			}
			
			return null;
		}
	}
	
	/// <summary>
	/// Правило добавления более одного промонабора для новых клиентов
	/// </summary>
	public class AddMoreOnePromoSetForNewClientsRule : IAddPromoSetRule
	{
		public Result<string> Validate(IUnitOfWork uow, IAddSaleItemSource source, PromotionalSet proSet)
		{
			if(source.Products.Any(x =>
					x.PromoSet is { PromotionalSetForNewClients: true }
					&& proSet.PromotionalSetForNewClients))
			{
				return Result.Failure<string>(PromoSetErrors.CantAddTwoPromoSetsForNewClients);
			}
			
			return null;
		}
	}
	
	public class AddPromoSetToSelfDeliveryRule : IAddPromoSetRule
	{
		public Result<string> Validate(IUnitOfWork uow, IAddSaleItemSource source, PromotionalSet proSet)
		{
			if(source.IsSelfDelivery)
			{
				return Result.Success(string.Empty);
			}

			return null;
		}
	
	}
	
	/// <summary>
	/// Правило добавления более одного промонабора для новых клиентов
	/// </summary>
	public class AddPromoSetForNewClientsWithPreviousShipmentRule : IAddPromoSetRule
	{
		private readonly IFreeLoaderChecker _freeLoaderChecker;

		public AddPromoSetForNewClientsWithPreviousShipmentRule(IFreeLoaderChecker freeLoaderChecker)
		{
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
		}
		
		public Result<string> Validate(IUnitOfWork uow, IAddSaleItemSource source, PromotionalSet proSet)
		{
			if(proSet.PromotionalSetForNewClients
				&& _freeLoaderChecker.CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
					uow,
					source.IsSelfDelivery,
					source.Counterparty,
					source.DeliveryPoint)
				)
			{
				return Result.Failure<string>(PromoSetErrors.HasPreviousShipmentToAnotherIndividualClient);
			}

			return null;
		}
	}
	
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
		
		public Result<string> Validate(IUnitOfWork uow, IAddSaleItemSource saleItemSource, PromotionalSet proSet)
		{
			var questionMessage = string.Empty;
			
			if(!proSet.PromotionalSetForNewClients)
			{
				return Result.Success(questionMessage);
			}
			
			var proSetDict = _promotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(uow, saleItemSource);

			if(!proSetDict.Any())
			{
				return Result.Success(questionMessage);
			}
			
			var address = string.Join(
				", ",
				saleItemSource.DeliveryPoint.City,
				saleItemSource.DeliveryPoint.Street,
				saleItemSource.DeliveryPoint.Building,
				saleItemSource.DeliveryPoint.Room);
			
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
