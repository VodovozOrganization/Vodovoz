using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Project.DB;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.EntityRepositories.Orders;

namespace Vodovoz.Infrastructure.Persistance.Sale
{
	internal sealed class OnlineOrderTemplateQueryOverRepository : IOnlineOrderTemplateQueryOverRepository
	{
		/*public IQueryOver GetQueryOverOnlineOrderTemplateDataByTemplateId(int templateId)
		{
			OnlineOrderTemplate templateAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			OnlineOrderTemplateInfo resultAlias = null;

			var query = QueryOver.Of(() => templateAlias)
				.JoinEntityAlias(
					() => counterpartyAlias,
					() => templateAlias.CounterpartyId == counterpartyAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(
					() => deliveryPointAlias,
					() => templateAlias.DeliveryPointId == deliveryPointAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(
					() => deliveryScheduleAlias,
					() => templateAlias.DeliveryScheduleId == deliveryScheduleAlias.Id,
					JoinType.LeftOuterJoin)
				.Where(() => templateAlias.Id == templateId)
				.SelectList(list => list
					.Select(() => templateAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => templateAlias.IsActive).WithAlias(() => resultAlias.IsActive)
					.Select(
						Projections.Cast(
							NHibernateUtil.String,
							Projections.Property(() => templateAlias.DeliveryFrequency)))
						.WithAlias(() => resultAlias.RepeatOrder)
					.Select(
						Projections.Cast(
							NHibernateUtil.String,
							Projections.Property(() => templateAlias.PaymentType)))
						.WithAlias(() => resultAlias.PaymentType)
					.Select(() => templateAlias.CounterpartyId).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => templateAlias.DeliveryPointId).WithAlias(() => resultAlias.DeliveryPointId)
					.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.DeliveryAddress)
					.Select(
						CustomProjections.Concat(
							Projections.Constant("с "),
							//TODO: зарегистрировать в основных функциях
							Projections.SqlFunction(
								new SQLFunctionTemplate(NHibernateUtil.String, "TIME_FORMAT(?1, ?2)"),
								NHibernateUtil.String,
								Projections.Property(() => deliveryScheduleAlias.From),
								Projections.Constant("%H:%i")),
							Projections.Constant(" до "),
							Projections.SqlFunction(
								new SQLFunctionTemplate(NHibernateUtil.String, "TIME_FORMAT(?1, ?2)"),
								NHibernateUtil.String,
								Projections.Property(() => deliveryScheduleAlias.To),
								Projections.Constant("%H:%i"))))
						.WithAlias(() => resultAlias.DeliverySchedule)
				)
				.TransformUsing(Transformers.AliasToBean<OnlineOrderTemplateInfo>());

			return query;
		}*/
		
		public IQueryOver GetQueryOverOnlineOrderTemplates(IEnumerable<int> templatesIds)
		{
			var query = QueryOver.Of<OnlineOrderTemplate>()
				.WhereRestrictionOn(t => t.Id).IsInG(templatesIds)
				.TransformUsing(Transformers.RootEntity);

			return query;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateCounterpartyDataByTemplateId(int templateId)
		{
			OnlineOrderTemplate templateAlias = null;
			Counterparty counterpartyAlias = null;
			
			return QueryOver.Of(() => counterpartyAlias)
				.JoinEntityAlias(() => templateAlias, () => counterpartyAlias.Id == templateAlias.CounterpartyId)
				.Where(c => templateAlias.Id == templateId)
				.Select(Projections.RootEntity());
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateDeliveryPointDataByTemplateId(int templateId)
		{
			OnlineOrderTemplate templateAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			
			return QueryOver.Of(() => deliveryPointAlias)
				.JoinEntityAlias(() => templateAlias, () => deliveryPointAlias.Id == templateAlias.DeliveryPointId)
				.Where(c => templateAlias.Id == templateId)
				.Select(Projections.RootEntity());
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateWeekdaysByTemplateId(int templateId)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateWeekday weekdayAlias = null;
			
			return QueryOver.Of(() => weekdayAlias)
					.JoinEntityAlias(() => templateAlias, () => weekdayAlias.TemplateId == templateAlias.Id)
					.Where(() => templateAlias.Id == templateId)
					.SelectList(list => list
						.Select(
							Projections.Cast(
								NHibernateUtil.String,
								Projections.Property(() => weekdayAlias.Weekday))
						)
					)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateWeekdays(IEnumerable<int> templatesIds)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateWeekday weekdayAlias = null;
			
			return QueryOver.Of(() => weekdayAlias)
					.JoinEntityAlias(() => templateAlias, () => weekdayAlias.TemplateId == templateAlias.Id)
					.WhereRestrictionOn(() => templateAlias.Id).IsInG(templatesIds)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateSaleItemsByTemplateId(int templateId)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateSaleItem templateSaleItemAlias = null;
			
			return QueryOver.Of(() => templateSaleItemAlias)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.Id == templateSaleItemAlias.TemplateId,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.Where(() => templateAlias.Id == templateId)
					.Fetch(SelectMode.Fetch, x => x.Nomenclature)
					.Fetch(SelectMode.Fetch, x => x.PromoSet)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateSaleItems(IEnumerable<int> templatesIds)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateSaleItem templateSaleItemAlias = null;
			
			return QueryOver.Of(() => templateSaleItemAlias)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.Id == templateSaleItemAlias.TemplateId,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.WhereRestrictionOn(() => templateAlias.Id).IsInG(templatesIds)
					.Fetch(SelectMode.Fetch, x => x.Nomenclature)
					.Fetch(SelectMode.Fetch, x => x.PromoSet)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplatePromoSets(IEnumerable<int> templatesIds)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplatePromoSet templatePromoSetAlias = null;
			
			return QueryOver.Of(() => templatePromoSetAlias)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.Id == templatePromoSetAlias.TemplateId,
						JoinType.LeftOuterJoin)
					.WhereRestrictionOn(() => templateAlias.Id).IsInG(templatesIds)
					.Fetch(SelectMode.Fetch, x => x.PromoSet)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateFreeRentPackages(IEnumerable<int> templatesIds)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateFreeRentPackage templateFreeRentPackage = null;
			
			return QueryOver.Of(() => templateFreeRentPackage)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.Id == templateFreeRentPackage.TemplateId,
						JoinType.LeftOuterJoin)
					.WhereRestrictionOn(() => templateAlias.Id).IsInG(templatesIds)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateDeliveryPointFixedPrices(IEnumerable<int> templatesIds)
		{
			NomenclatureFixedPrice fixedPrice = null;
			OnlineOrderTemplate templateAlias = null;
			
			return QueryOver.Of(() => fixedPrice)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.DeliveryPointId == fixedPrice.DeliveryPoint.Id,
						JoinType.LeftOuterJoin)
					.WhereRestrictionOn(() => templateAlias.Id).IsInG(templatesIds)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateCounterpartyFixedPrices(IEnumerable<int> templatesIds)
		{
			NomenclatureFixedPrice fixedPrice = null;
			OnlineOrderTemplate templateAlias = null;
			
			return QueryOver.Of(() => fixedPrice)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.CounterpartyId == fixedPrice.Counterparty.Id,
						JoinType.LeftOuterJoin)
					.WhereRestrictionOn(() => templateAlias.Id).IsInG(templatesIds)
					.TransformUsing(Transformers.RootEntity)
				;
		}
	}
}
