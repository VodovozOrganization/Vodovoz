using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Multi;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.Extensions;
using VodovozBusiness.Nodes;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	public sealed class OnlineOrderTemplateRepository : IOnlineOrderTemplateRepository
	{
		private const string _templateBatchKey = "templateInfo";
		private const string _counterpartyBatchKey = "counterparty";
		private const string _deliveryPointBatchKey = "deliveryPoint";
		private const string _weekdaysBatchKey = "weekdays";
		private const string _productsBatchKey = "products";
		private readonly IOnlineOrderTemplateQueryOverRepository _queryOverRepository;

		public OnlineOrderTemplateRepository(IOnlineOrderTemplateQueryOverRepository queryOverRepository)
		{
			_queryOverRepository = queryOverRepository ?? throw new ArgumentNullException(nameof(queryOverRepository));
		}

		public async Task<OnlineOrdersTemplatesData> GetOnlineOrdersTemplatesDataAsync(IUnitOfWork uow, int[] templatesIds)
		{
			var batch = uow.Session.CreateQueryBatch();

			var templateBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateData(templatesIds);
			//var counterpartyBatch = _onlineOrderTemplateRepository.GetQueryOverOnlineOrderTemplateCounterpartyDataByTemplateId(templateId);
			//var deliveryPointBatch = _onlineOrderTemplateRepository.GetQueryOverOnlineOrderTemplateDeliveryPointDataByTemplateId(templateId);
			var weekdaysBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateWeekdays(templatesIds);
			var templateProductsBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateProducts(templatesIds);

			batch
				.Add<OnlineOrderTemplateData>(_templateBatchKey, templateBatch)
				//.Add<Counterparty>(counterpartyBatchKey, counterpartyBatch)
				//.Add<DeliveryPoint>(deliveryPointBatchKey, deliveryPointBatch)
				.Add<OnlineOrderTemplateWeekday>(_weekdaysBatchKey, weekdaysBatch)
				.Add<OnlineOrderTemplateProduct>(_productsBatchKey, templateProductsBatch)
				;

			await batch.ExecuteAsync();

			var templates = (await batch
					.GetResultAsync<OnlineOrderTemplateData>(_templateBatchKey))
				.ToList();
			//var counterparty = (await batch.GetResultAsync<Counterparty>(counterpartyBatchKey)).FirstOrDefault();
			//var deliveryPoint = (await batch.GetResultAsync<DeliveryPoint>(deliveryPointBatchKey)).FirstOrDefault();
			var weekdays = (await batch
					.GetResultAsync<OnlineOrderTemplateWeekday>(_weekdaysBatchKey))
				.ToLookup(x => x.TemplateId);

			var products = (await batch
					.GetResultAsync<OnlineOrderTemplateProduct>(_productsBatchKey))
				.ToLookup(x => x.TemplateId);

			return OnlineOrdersTemplatesData.Create(templates, weekdays, products);
		}

		public async Task<AggregateOnlineOrderTemplateInfo> GetAggregateOnlineOrderTemplateInfoAsync(IUnitOfWork uow, int templateId)
		{
			var batch = uow.Session.CreateQueryBatch();

			var templateBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateDataByTemplateId(templateId);
			var counterpartyBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateCounterpartyDataByTemplateId(templateId);
			var deliveryPointBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateDeliveryPointDataByTemplateId(templateId);
			var weekdaysBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateWeekdaysByTemplateId(templateId);
			var templateProductsBatch = _queryOverRepository.GetQueryOverOnlineOrderTemplateProductsByTemplateId(templateId);

			batch
				.Add<OnlineOrderTemplateInfo>(_templateBatchKey, templateBatch)
				.Add<Counterparty>(_counterpartyBatchKey, counterpartyBatch)
				.Add<DeliveryPoint>(_deliveryPointBatchKey, deliveryPointBatch)
				.Add<string>(_weekdaysBatchKey, weekdaysBatch)
				.Add<OnlineOrderTemplateProduct>(_productsBatchKey, templateProductsBatch)
				;

			await batch.ExecuteAsync();

			var templateData = (await batch.GetResultAsync<OnlineOrderTemplateInfo>(_templateBatchKey))
				.FirstOrDefault();
			var counterparty = (await batch.GetResultAsync<Counterparty>(_counterpartyBatchKey))
				.FirstOrDefault();
			var deliveryPoint = (await batch.GetResultAsync<DeliveryPoint>(_deliveryPointBatchKey))
				.FirstOrDefault();
			var weekdays = await batch.GetResultAsync<string>(_weekdaysBatchKey);
			var products = await batch.GetResultAsync<OnlineOrderTemplateProduct>(_productsBatchKey);

			return AggregateOnlineOrderTemplateInfo.Create(templateData, counterparty, deliveryPoint, weekdays, products);
		}
		
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

		public IEnumerable<OnlineOrderTemplate> GetActiveOnlineOrdersTemplatesForCreateOrders(
			IUnitOfWork uow, DateTime date)
		{
			//TODO доработать алгоритм подбора шаблонов за день до доставки
			var weekDayFromDate = date.DayOfWeek.ConvertToWeekDayName();
			
			var templates = (
				from template in uow.Session.Query<OnlineOrderTemplate>()
				join weekday in uow.Session.Query<OnlineOrderTemplateWeekday>()
					on template.Id equals weekday.TemplateId
					
				let lastOnlineFromTemplate = (
					from onlineOrder in uow.Session.Query<OnlineOrder>()
					where onlineOrder.TemplateId == template.Id
					orderby onlineOrder.Id descending 
					select onlineOrder
					)
					.FirstOrDefault()

				let needCreateByDay = weekday.Weekday == WeekDayName.Sunday
					? (int)weekday.Weekday - (int)weekDayFromDate == 6
					: (int)weekday.Weekday - (int)weekDayFromDate == 1
					
				let days = template.DeliveryFrequency == OnlineOrderDeliveryFrequency.OnePerWeek
					? 7
					: template.DeliveryFrequency == OnlineOrderDeliveryFrequency.OneEveryTwoWeeks
						? 14
						: template.DeliveryFrequency == OnlineOrderDeliveryFrequency.OneEveryThreeWeeks
							? 21
							: template.DeliveryFrequency == OnlineOrderDeliveryFrequency.OneEveryFourWeeks
								? 28
								: 0

				let needCreateByWeek = lastOnlineFromTemplate != null
					|| (
						lastOnlineFromTemplate.Created.Date != date.Date
							&& lastOnlineFromTemplate.Created.AddDays(days).Date == date.Date
						)
				
				where template.IsActive && needCreateByDay && needCreateByWeek

				select template
				)
				.Distinct()
				.ToList();
			
			return templates;
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
						RepeatOrder = template.DeliveryFrequency.ToString(),
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

		public IEnumerable<OnlineOrderTemplateCardForListData> GetOnlineOrdersTemplatesDataByCounterpartyId(
			IUnitOfWork uow,
			int counterpartyId,
			int skip,
			int take)
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
						RepeatOrder = template.DeliveryFrequency.ToString(),
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
		
		public IEnumerable<OnlineOrderTemplateWeekdayData> GetOnlineOrdersTemplatesWeekdaysData(
			IUnitOfWork uow,
			int counterpartyId,
			int skip,
			int take)
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
	}
}
