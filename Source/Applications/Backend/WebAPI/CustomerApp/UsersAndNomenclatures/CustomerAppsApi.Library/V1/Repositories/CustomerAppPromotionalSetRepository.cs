using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.V1.Dto.Goods;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V1.Repositories
{
	/// <summary>
	/// Репозиторий получения данных по промонаборам
	/// </summary>
	public class CustomerAppPromotionalSetRepository : ICustomerAppPromotionalSetRepository
	{
		/// <inheritdoc/>
		public IEnumerable<PromotionalSetOnlineParametersDto> GetActivePromotionalSetsOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			return (
				from onlineParameters in uow.Session.Query<PromotionalSetOnlineParameters>()
				join promoSet in uow.Session.Query<PromotionalSet>()
					on onlineParameters.PromotionalSet.Id equals promoSet.Id
				where onlineParameters.PromotionalSetOnlineAvailability != null
					&& onlineParameters.Type == parameterType
					&& !promoSet.IsArchive
				select new PromotionalSetOnlineParametersDto
				{
					Id = onlineParameters.Id,
					PromotionalSetId =  promoSet.Id,
					PromotionalSetOnlineName = promoSet.OnlineName,
					PromotionalSetForNewClients = promoSet.PromotionalSetForNewClients,
					BottlesCountForCalculatingDeliveryPrice = promoSet.BottlesCountForCalculatingDeliveryPrice,
					AvailableForSale = onlineParameters.PromotionalSetOnlineAvailability
				})
				.ToList();
		}

		/// <inheritdoc/>
		public IEnumerable<PromotionalSetItemBalanceDto> GetPromotionalSetsItemsWithBalanceForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouses)
		{
			PromotionalSet promotionalSetAlias = null;
			PromotionalSetItem promotionalSetItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclature2Alias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			PromotionalSetItemBalanceDto resultAlias = null;

			var discountProjection = Projections.Conditional(
				Restrictions.Where(() => promotionalSetItemAlias.IsDiscountInMoney),
				Projections.Property(() => promotionalSetItemAlias.DiscountMoney),
				Projections.Property(() => promotionalSetItemAlias.Discount));

			var balanceSubQuery = QueryOver.Of(() => nomenclature2Alias)
				.JoinEntityAlias(
					() => operationAlias,
					() => nomenclature2Alias.Id == operationAlias.Nomenclature.Id,
					JoinType.LeftOuterJoin)
				.Where(() => nomenclatureAlias.Id == nomenclature2Alias.Id)
				.AndRestrictionOn(() => operationAlias.Warehouse).IsInG(warehouses)
				.Select(Projections.Sum(() => operationAlias.Amount));

			return uow.Session.QueryOver<PromotionalSetOnlineParameters>()
				.Left.JoinAlias(p => p.PromotionalSet, () => promotionalSetAlias)
				.Left.JoinAlias(() => promotionalSetAlias.PromotionalSetItems, () => promotionalSetItemAlias)
				.Left.JoinAlias(() => promotionalSetItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(p => p.Type == parameterType)
				.And(p => p.PromotionalSetOnlineAvailability != null)
				.And(() => !promotionalSetAlias.IsArchive)
				.SelectList(list => list
					.Select(() => promotionalSetAlias.Id).WithAlias(() => resultAlias.PromotionalSetId)
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => promotionalSetItemAlias.Count).WithAlias(() => resultAlias.Count)
					.Select(discountProjection).WithAlias(() => resultAlias.Discount)
					.Select(() => promotionalSetItemAlias.IsDiscountInMoney).WithAlias(() => resultAlias.IsDiscountMoney)
					.SelectSubQuery(balanceSubQuery).WithAlias(() => resultAlias.Stock)
				)
				.TransformUsing(Transformers.AliasToBean<PromotionalSetItemBalanceDto>())
				.List<PromotionalSetItemBalanceDto>();
		}
	}
}
