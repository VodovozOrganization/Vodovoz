using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Multi;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.EntityRepositories.Orders;
using VodovozBusiness.EntityRepositories.Sale;
using VodovozBusiness.Nodes;

namespace Vodovoz.Infrastructure.Persistance.Sale
{
	public class OnlineOrderTemplateRepository : IOnlineOrderTemplateRepository
	{
		private const string _templatesBatchKey = "templatesKey";
		private const string _saleItemsBatchKey = "saleItemsKey";
		private const string _promoSetsBatchKey = "promoSetsKey";
		private const string _rentPackagesBatchKey = "rentPackagesKey";
		private const string _deliveryPointFixedPricesBatchKey = "deliveryPointFixedPricesKey";
		private const string _counterpartyFixedPricesBatchKey = "counterpartyFixedPricesKey";

		private readonly IOnlineOrderTemplateQueryOverRepository _templateQueryOverRepository;

		public OnlineOrderTemplateRepository(IOnlineOrderTemplateQueryOverRepository templateQueryOverRepository)
		{
			_templateQueryOverRepository = templateQueryOverRepository ?? throw new ArgumentNullException(nameof(templateQueryOverRepository));
		}

		/// <inheritdoc/>
		public int GetActiveOnlineOrderTemplatesCount(IUnitOfWork uow, int counterpartyId)
		{
			return (
				from template in uow.Session.Query<OnlineOrderTemplate>()
				where template.CounterpartyId == counterpartyId
					&& template.Status != OnlineOrderTemplateStatus.Archived
				select template.Id
				)
				.Count();
		}
		
		/// <inheritdoc/>
		public bool NotArchiveTemplateExists(IUnitOfWork uow, int templateId)
		{
			return (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					where template.Id == templateId
						&& template.Status != OnlineOrderTemplateStatus.Archived
					select template.Id
				)
				.Any();
		}

		/// <inheritdoc/>
		public bool OnlineOrderTemplateFromCounterparty(IUnitOfWork uow, int templateId, int? counterpartyId)
		{
			return (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					where template.Id == templateId
						&& template.CounterpartyId == counterpartyId
					select template.Id
				)
				.Any();
		}

		[Obsolete]
		public IEnumerable<OnlineOrderTemplate> GetActiveOnlineOrdersTemplatesForCreateOrders(IUnitOfWork uow, DateTime dateTime)
		{
			//TODO-6121 переделать алгоритм под новые условия создания онлайн заказа
			var weekDayFromDate = dateTime.DayOfWeek.ConvertToWeekDayName();
			
			var templates = (
				from template in uow.Session.Query<OnlineOrderTemplate>()
				join weekday in uow.Session.Query<OnlineOrderTemplateWeekday>()
					on template.Id equals weekday.TemplateId
					
				let lastOnlineFromTemplate = (
					from onlineOrder in uow.Session.Query<OnlineOrder>()
					where onlineOrder.Template.Id == template.Id
					orderby onlineOrder.Id descending 
					select onlineOrder
					)
					.FirstOrDefault()

				let lastOnlineOrder = (
						from onlineOrder in uow.Session.Query<OnlineOrder>()
						where onlineOrder.Counterparty.Id == template.CounterpartyId
							&& onlineOrder.DeliveryPoint.Id == template.DeliveryPointId
							&& onlineOrder.DeliverySchedule.Id == template.Schedule.DeliveryScheduleId
							&& onlineOrder.DeliveryDate == dateTime.AddDays(1).Date
						orderby onlineOrder.Id descending 
						select onlineOrder
					)
					.FirstOrDefault()

				let needCreateByDay = weekDayFromDate == WeekDayName.Sunday
					? (byte)weekDayFromDate - weekday.DayNumber == 6
					: weekday.DayNumber - (byte)weekDayFromDate == 1

				where template.IsActive && needCreateByDay

				//добавить в валидатор доп условие по уже созданному онлайну(не автозаказу) на эту дату, от этого клиента, ТД и время
				select new ValueTuple<OnlineOrderTemplate, OnlineOrder>(template, lastOnlineFromTemplate)
				)
				.Distinct()
				.ToList();

			return (
				from template in templates

				let days = 0

				//не нашел решения, как заставить работать AddDays(days) в трансляторе HQL, с подзапросом
				//поэтому вынес после sql
				/* т.к. помимо дней у нас есть еще периодичность доставки, то необходимость создания будет определяться как
				 * Онлайн заказа по этому шаблону нет
				 * ИЛИ
				 * Дата доставки онлайн заказа не равна дате плюс один день(создаем за день до доставки), т.е. мы не создали уже свежий
				 * И
				 * Либо день доставки онлайна не равен дню от даты плюс один день
				 * Либо дата плюс один день равна дате доставки онлайна плюс количество дней по периодичности доставки
				 */
				let needCreateByWeek = template.Item2 == null
					|| template.Item2.DeliveryDate.Date != dateTime.AddDays(1).Date
					&& (template.Item2.DeliveryDate.DayOfWeek != dateTime.AddDays(1).DayOfWeek
						|| dateTime.AddDays(1).Date == template.Item2.DeliveryDate.AddDays(days))

				where needCreateByWeek
				select template.Item1)
				.ToList();
		}

		/// <inheritdoc/>
		public async Task<OnlineOrdersTemplatesData> GetOnlineOrdersTemplatesDataAsync(IUnitOfWork uow, IEnumerable<int> templatesIds)
		{
			var batch = uow.Session.CreateQueryBatch();

			var templatesBatch = _templateQueryOverRepository.GetQueryOverOnlineOrderTemplates(templatesIds);
			var deliveryPointFixedPrices = _templateQueryOverRepository.GetQueryOverOnlineOrderTemplateDeliveryPointFixedPrices(templatesIds);
			var counterpartyFixedPrices = _templateQueryOverRepository.GetQueryOverOnlineOrderTemplateCounterpartyFixedPrices(templatesIds);
			var templatesSaleItemsBatch = _templateQueryOverRepository.GetQueryOverOnlineOrderTemplateSaleItems(templatesIds);
			var templatesPromoSetsBatch = _templateQueryOverRepository.GetQueryOverOnlineOrderTemplatePromoSets(templatesIds);
			var templatesRentPackagesBatch = _templateQueryOverRepository.GetQueryOverOnlineOrderTemplateFreeRentPackages(templatesIds);

			batch
				.Add<OnlineOrderTemplate>(_templatesBatchKey, templatesBatch)
				.Add<NomenclatureFixedPrice>(_deliveryPointFixedPricesBatchKey, deliveryPointFixedPrices)
				.Add<NomenclatureFixedPrice>(_counterpartyFixedPricesBatchKey, counterpartyFixedPrices)
				.Add<OnlineOrderTemplateSaleItem>(_saleItemsBatchKey, templatesSaleItemsBatch)
				.Add<OnlineOrderTemplatePromoSet>(_promoSetsBatchKey, templatesPromoSetsBatch)
				.Add<OnlineOrderTemplateFreeRentPackage>(_rentPackagesBatchKey, templatesRentPackagesBatch)
				;

			await batch.ExecuteAsync();

			var templates = (await batch
					.GetResultAsync<OnlineOrderTemplate>(_templatesBatchKey))
				.ToList();

			var deliveryPointFixedPricesLookup = (await batch
					.GetResultAsync<NomenclatureFixedPrice>(_deliveryPointFixedPricesBatchKey))
				.ToLookup(x => x.DeliveryPoint.Id);

			var counterpartyFixedPricesLookup = (await batch
					.GetResultAsync<NomenclatureFixedPrice>(_counterpartyFixedPricesBatchKey))
				.ToLookup(x => x.Counterparty.Id);
			
			var saleItemsLookup = (await batch
					.GetResultAsync<OnlineOrderTemplateSaleItem>(_saleItemsBatchKey))
				.ToLookup(x => x.TemplateId);

			var promoSetsLookup = (await batch
					.GetResultAsync<OnlineOrderTemplatePromoSet>(_promoSetsBatchKey))
				.ToLookup(x => x.TemplateId);

			var rentPackagesLookup = (await batch
					.GetResultAsync<OnlineOrderTemplateFreeRentPackage>(_rentPackagesBatchKey))
				.ToLookup(x => x.TemplateId);

			return OnlineOrdersTemplatesData.Create(
				templates,
				deliveryPointFixedPricesLookup,
				counterpartyFixedPricesLookup,
				saleItemsLookup,
				promoSetsLookup,
				rentPackagesLookup);
		}
	}
}
