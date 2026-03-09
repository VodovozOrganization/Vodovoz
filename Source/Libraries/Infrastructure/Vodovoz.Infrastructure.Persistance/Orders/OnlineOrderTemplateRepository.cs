using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;
using VodovozBusiness.EntityRepositories.Orders;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	public sealed class OnlineOrderTemplateRepository : IOnlineOrderTemplateRepository
	{
		public int GetOnlineOrdersTemplatesCount(IUnitOfWork uow, int counterpartyId)
		{
			var templatesCount = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					where template.CounterpartyId == counterpartyId
					select template
				)
				.Count();
			
			return templatesCount;
		}

		public OnlineOrderTemplateInfo GetOnlineOrderTemplateDataByTemplateId(IUnitOfWork uow, int templateId)
		{
			var templates = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					join counterparty in uow.Session.Query<Counterparty>()
						on template.CounterpartyId equals counterparty.Id
					join deliveryPoint in uow.Session.Query<DeliveryPoint>()
						on template.DeliveryPointId equals deliveryPoint.Id into deliveryPoints
					from deliveryPoint in deliveryPoints.DefaultIfEmpty()
					join deliverySchedule in uow.Session.Query<DeliverySchedule>()
						on template.DeliveryScheduleId equals deliverySchedule.Id
					where template.Id == templateId
					select new OnlineOrderTemplateInfo
					{
						Id = template.Id,
						IsActive = template.IsActive,
						RepeatOrder = template.RepeatOrder.ToString(),
						PaymentType = template.PaymentType.ToString(),
						CounterpartyId = template.CounterpartyId,
						DeliveryPointId = template.DeliveryPointId,
						DeliveryAddress = deliveryPoint.ShortAddress,
						DeliverySchedule = $"с {deliverySchedule.From:hh\\:mm} до {deliverySchedule.To:hh\\:mm}"
					}
				)
				.FirstOrDefault();

			return templates;
		}

		public IEnumerable<OnlineOrderTemplateCardForListData> GetOnlineOrdersTemplatesDataByCounterpartyId(IUnitOfWork uow, int counterpartyId, int skip, int take)
		{
			var templates = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					join deliveryPoint in uow.Session.Query<DeliveryPoint>()
						on template.DeliveryPointId equals deliveryPoint.Id into deliveryPoints
					from deliveryPoint in deliveryPoints.DefaultIfEmpty()
					join deliverySchedule in uow.Session.Query<DeliverySchedule>()
						on template.DeliveryScheduleId equals deliverySchedule.Id
					where template.CounterpartyId == counterpartyId
					select new OnlineOrderTemplateCardForListData
					{
						Id = template.Id,
						IsActive = template.IsActive,
						RepeatOrder = template.RepeatOrder.ToString(),
						DeliveryAddress = deliveryPoint.ShortAddress,
						DeliverySchedule = $"с {deliverySchedule.From:hh\\:mm} до {deliverySchedule.To:hh\\:mm}"
					}
				)
				.Skip(skip)
				.Take(take)
				.ToList();
			
			return templates;
		}
		
		public IEnumerable<OnlineOrderTemplateProduct> GetOnlineOrdersTemplatesProductsByTemplateId(IUnitOfWork uow, int templateId)
		{
			var products = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					join product in uow.Session.Query<OnlineOrderTemplateProduct>()
						on template.Id equals product.TemplateId
					where template.Id == templateId
					select product
				)
				.ToList();
			
			return products;
		}
		
		public IEnumerable<OnlineOrderTemplateWeekdayData> GetOnlineOrdersTemplatesWeekdaysData(IUnitOfWork uow, int counterpartyId, int skip, int take)
		{
			var weekdays = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					join weekday in uow.Session.Query<OnlineOrderTemplateWeekday>()
						on template.Id equals weekday.TemplateId
					where template.CounterpartyId == counterpartyId
					select OnlineOrderTemplateWeekdayData.Create(template.Id, weekday.Weekday.ToString())
				)
				.Skip(skip)
				.Take(take)
				.ToList();
			
			return weekdays;
		}

		public IEnumerable<OnlineOrderTemplateWeekdayData> GetOnlineOrdersTemplatesWeekdaysDataByTemplateId(IUnitOfWork uow, int templateId)
		{
			var weekdays = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					join weekday in uow.Session.Query<OnlineOrderTemplateWeekday>()
						on template.Id equals weekday.TemplateId
					where template.Id == templateId
					select OnlineOrderTemplateWeekdayData.Create(template.Id, weekday.Weekday.ToString())
				)
				.ToList();

			return weekdays;
		}

		public IEnumerable<string> GetOnlineOrdersTemplatesWeekdaysByTemplateId(IUnitOfWork uow, int templateId)
		{
			var weekdays = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					join weekday in uow.Session.Query<OnlineOrderTemplateWeekday>()
						on template.Id equals weekday.TemplateId
					where template.Id == templateId
					select weekday.Weekday.ToString()
				)
				.ToList();

			return weekdays;
		}
		
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
							Projections.Property(() => templateAlias.RepeatOrder)))
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
	}
}
