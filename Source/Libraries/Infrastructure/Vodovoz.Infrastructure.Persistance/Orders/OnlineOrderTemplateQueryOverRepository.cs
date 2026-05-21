using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Project.DB;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;
using VodovozBusiness.Nodes;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	internal sealed class OnlineOrderTemplateQueryOverRepository : IOnlineOrderTemplateQueryOverRepository
	{
		public IQueryOver GetQueryOverOnlineOrderTemplateDataByTemplateId(int templateId)
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
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateData(int[] templatesIds)
		{
			OnlineOrderTemplate templateAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;
			OnlineOrderTemplateData resultAlias = null;

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
				.WhereRestrictionOn(() => templateAlias.Id).IsIn(templatesIds)
				.SelectList(list => list
					.Select(() => templateAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => templateAlias.Source).WithAlias(() => resultAlias.Source)
					.Select(() => templateAlias.ExternalCounterpartyId).WithAlias(() => resultAlias.ExternalCounterpartyId)
					.Select(() => templateAlias.IsActive).WithAlias(() => resultAlias.IsActive)
					.Select(() => templateAlias.DeliveryFrequency).WithAlias(() => resultAlias.DeliveryFrequency)
					.Select(() => templateAlias.PaymentType).WithAlias(() => resultAlias.PaymentType)
					.Select(() => templateAlias.IsSelfDelivery).WithAlias(() => resultAlias.IsSelfDelivery)
					.Select(() => templateAlias.IsFastDelivery).WithAlias(() => resultAlias.IsFastDelivery)
					.Select(() => templateAlias.BottlesReturn).WithAlias(() => resultAlias.BottlesReturn)
					.Select(() => templateAlias.CallBeforeArrivalMinutes).WithAlias(() => resultAlias.CallBeforeArrivalMinutes)
					.Select(() => templateAlias.ContactPhone).WithAlias(() => resultAlias.ContactPhone)
					.Select(() => templateAlias.DontArriveBeforeInterval).WithAlias(() => resultAlias.DontArriveBeforeInterval)
					.Select(() => templateAlias.IsNeedConfirmationByCall).WithAlias(() => resultAlias.IsNeedConfirmationByCall)
					.Select(() => templateAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => deliveryPointAlias).WithAlias(() => resultAlias.DeliveryPoint)
					.Select(() => counterpartyAlias).WithAlias(() => resultAlias.Counterparty)
					.Select(() => deliveryScheduleAlias).WithAlias(() => resultAlias.DeliverySchedule)
				)
				.TransformUsing(Transformers.AliasToBean<OnlineOrderTemplateData>());

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
		
		public IQueryOver GetQueryOverOnlineOrderTemplateWeekdays(int[] templatesIds)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateWeekday weekdayAlias = null;
			
			return QueryOver.Of(() => weekdayAlias)
					.JoinEntityAlias(() => templateAlias, () => weekdayAlias.TemplateId == templateAlias.Id)
					.WhereRestrictionOn(() => templateAlias.Id).IsIn(templatesIds)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateProductsByTemplateId(int templateId)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateProduct templateProductAlias = null;
			
			return QueryOver.Of(() => templateProductAlias)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.Id == templateProductAlias.TemplateId,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.Where(() => templateAlias.Id == templateId)
					.Fetch(SelectMode.Fetch, x => x.Nomenclature)
					.Fetch(SelectMode.Fetch, x => x.PromoSet)
					.TransformUsing(Transformers.RootEntity)
				;
		}
		
		public IQueryOver GetQueryOverOnlineOrderTemplateProducts(int[] templatesIds)
		{
			OnlineOrderTemplate templateAlias = null;
			OnlineOrderTemplateProduct templateProductAlias = null;
			
			return QueryOver.Of(() => templateProductAlias)
					.JoinEntityAlias(
						() => templateAlias,
						() => templateAlias.Id == templateProductAlias.TemplateId,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.WhereRestrictionOn(() => templateAlias.Id).IsIn(templatesIds)
					.Fetch(SelectMode.Fetch, x => x.Nomenclature)
					.Fetch(SelectMode.Fetch, x => x.PromoSet)
					.TransformUsing(Transformers.RootEntity)
				;
		}
	}
}
