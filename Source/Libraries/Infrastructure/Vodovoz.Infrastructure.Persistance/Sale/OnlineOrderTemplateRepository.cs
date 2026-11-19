using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Multi;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.NHibernate.NhibernateExtensions.HqlGenerators;
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

		public async Task<IEnumerable<int>> GetActiveOnlineOrdersTemplatesForCreateOrdersAsync(
			IUnitOfWork uow,
			DateTime dateTime,
			CancellationToken cancellationToken)
		{
			var targetWeekday = dateTime.DayOfWeek.ConvertToWeekDayName();
			// Последний день предыдущего месяца — для проверки сдвига 29/30/31-го числа
			var prevMonthLastDay = new DateTime(dateTime.Year, dateTime.Month, 1).AddDays(-1);

			var templateIds = uow.Session.Query<OnlineOrderTemplate>()
				.Where(t => !uow.Session.Query<OnlineOrder>()
					.Any(o => o.Template.Id == t.Id && o.DeliveryDate == dateTime))
				.Where(t => t.IsActive)
				.Where(t =>
					// Недельный шаблон
					(t.Schedule.ScheduleType == TemplateDeliveryScheduleType.Weekly
						&& t.Schedule.Weekday == targetWeekday
						&& t.Schedule.FirstDeliveryDate != null
						&& t.Schedule.FirstDeliveryDate.Value <= dateTime
						/*&& SqlDateFunctions.DateDiffDays(t.Schedule.FirstDeliveryDate.Value, dateTime)
							% (7 * t.Schedule.RepeatEveryWeeks.Value) == 0*/)
					||
					// Месячный шаблон: либо число совпало, либо 29/30/31 уехали на 1-е число следующего месяца
					(t.Schedule.ScheduleType == TemplateDeliveryScheduleType.Monthly
						&& (t.Schedule.DayOfMonth == dateTime.Day
							|| (dateTime.Day == 1 && t.Schedule.DayOfMonth > prevMonthLastDay.Day))
					)
				)
				.Select(t => t.Id);

			return await templateIds.ToListAsync(cancellationToken);
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
