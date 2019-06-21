using System;
using NHibernate;
using QS.DomainModel.Config;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Repository;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class OrderJournalViewModel : SingleEntityJournalViewModelBase<VodovozOrder, OrderDlg, OrderJournalNode, OrderJournalFilterViewModel>
	{
		public OrderJournalViewModel(OrderJournalFilterViewModel filterViewModel, IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал заказов";
			SetOrder<OrderJournalNode>(x => x.DeliveryTime, true);

			RegisterAliasPropertiesToSearch(
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.City,
				() => deliveryPointAlias.Street,
				() => deliveryPointAlias.Building,
				() => authorAlias.Name,
				() => authorAlias.LastName,
				() => authorAlias.Patronymic,
				() => lastEditorAlias.Name,
				() => lastEditorAlias.LastName,
				() => lastEditorAlias.Patronymic,
				() => orderAlias.LastEditedTime,
				() => orderAlias.DriverCallId,
				() => orderAlias.Id
			);
		}

		OrderJournalNode resultAlias = null;
		VodovozOrder orderAlias = null;
		Nomenclature nomenclatureAlias = null;
		OrderItem orderItemAlias = null;
		Counterparty counterpartyAlias = null;
		DeliveryPoint deliveryPointAlias = null;
		DeliverySchedule deliveryScheduleAlias = null;
		Employee authorAlias = null;
		Employee lastEditorAlias = null;
		ScheduleRestrictedDistrict districtAlias = null;

		protected override Func<IQueryOver<VodovozOrder>> ItemsSourceQueryFunction => () => {

			Nomenclature sanitizationNomenclature = NomenclatureRepository.GetSanitisationNomenclature(UoW);

			var query = UoW.Session.QueryOver<VodovozOrder>(() => orderAlias);

			if(FilterViewModel.RestrictStatus != null) {
				query.Where(o => o.OrderStatus == FilterViewModel.RestrictStatus);
			}

			if(FilterViewModel.RestrictPaymentType != null) {
				query.Where(o => o.PaymentType == FilterViewModel.RestrictPaymentType);
			}

			if(FilterViewModel.HideStatuses != null) {
				query.WhereRestrictionOn(o => o.OrderStatus).Not.IsIn(FilterViewModel.HideStatuses);
			}

			if(FilterViewModel.RestrictOnlySelfDelivery != null) {
				query.Where(o => o.SelfDelivery == FilterViewModel.RestrictOnlySelfDelivery);
			}

			if(FilterViewModel.RestrictWithoutSelfDelivery != null) {
				query.Where(o => o.SelfDelivery != FilterViewModel.RestrictWithoutSelfDelivery);
			}

			if(FilterViewModel.RestrictCounterparty != null) {
				query.Where(o => o.Client == FilterViewModel.RestrictCounterparty);
			}

			if(FilterViewModel.RestrictDeliveryPoint != null) {
				query.Where(o => o.DeliveryPoint == FilterViewModel.RestrictDeliveryPoint);
			}

			if(FilterViewModel.RestrictStartDate != null) {
				query.Where(o => o.DeliveryDate >= FilterViewModel.RestrictStartDate);
			}

			if(FilterViewModel.RestrictEndDate != null) {
				query.Where(o => o.DeliveryDate <= FilterViewModel.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.RestrictOnlyWithoutCoodinates) {
				query.Where(() => deliveryPointAlias.Longitude == null && deliveryPointAlias.Latitude == null);
			}

			if(FilterViewModel.RestrictLessThreeHours == true) {
				query.Where(Restrictions
							.GtProperty(Projections.SqlFunction(
											new SQLFunctionTemplate(NHibernateUtil.Time, "ADDTIME(?1, ?2)"),
											NHibernateUtil.Time,
											Projections.Property(() => deliveryScheduleAlias.From),
											Projections.Constant("3:0:0")),
											Projections.Property(() => deliveryScheduleAlias.To)));
			}

			if(FilterViewModel.RestrictHideService != null) {
				query.Where(o => o.IsService != FilterViewModel.RestrictHideService);
			}

			if(FilterViewModel.RestrictOnlyService != null) {
				query.Where(o => o.IsService == FilterViewModel.RestrictOnlyService);
			}

			var bottleCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var sanitisationCountSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
													 .Where(() => orderAlias.Id == orderItemAlias.Order.Id)
													 .Where(() => orderItemAlias.Nomenclature.Id == sanitizationNomenclature.Id)
													 .Select(Projections.Sum(() => orderItemAlias.Count));

			var orderSumSubquery = QueryOver.Of<OrderItem>(() => orderItemAlias)
											.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
											.Select(
												Projections.Sum(
													Projections.SqlFunction(
														new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 * ?2 - ?3"),
														NHibernateUtil.Decimal,
														Projections.Property<OrderItem>(x => x.Count),
														Projections.Property<OrderItem>(x => x.Price),
														Projections.Property<OrderItem>(x => x.DiscountMoney)
													   )
												   )
											   );

			query.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.DeliverySchedule, () => deliveryScheduleAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.Client, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .JoinAlias(o => o.LastEditor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				 .Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias);

			var resultQuery = query
				.SelectList(list => list
				   .Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				   .Select(() => deliveryScheduleAlias.Name).WithAlias(() => resultAlias.DeliveryTime)
				   .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => orderAlias.Address1c).WithAlias(() => resultAlias.Address1c)
				   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
				   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				   .Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorLastName)
				   .Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
				   .Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
				   .Select(() => orderAlias.LastEditedTime).WithAlias(() => resultAlias.LastEditedTime)
				   .Select(() => orderAlias.DriverCallId).WithAlias(() => resultAlias.DriverCallId)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
				   .Select(() => deliveryPointAlias.City).WithAlias(() => resultAlias.City)
				   .Select(() => deliveryPointAlias.Street).WithAlias(() => resultAlias.Street)
				   .Select(() => deliveryPointAlias.Building).WithAlias(() => resultAlias.Building)
				   .Select(() => deliveryPointAlias.Latitude).WithAlias(() => resultAlias.Latitude)
				   .Select(() => deliveryPointAlias.Longitude).WithAlias(() => resultAlias.Longitude)
				   .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.Sum)
				   .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.BottleAmount)
				   .SelectSubQuery(sanitisationCountSubquery).WithAlias(() => resultAlias.SanitisationAmount)
				)
				.OrderBy(x => x.DeliveryDate).Desc
				.TransformUsing(Transformers.AliasToBean<OrderJournalNode>());

			return resultQuery;
		};

		protected override Func<OrderDlg> CreateDialogFunction => () => {
			return new OrderDlg();
		};

		protected override Func<OrderJournalNode, OrderDlg> OpenDialogFunction => (node) => {
			return new OrderDlg(node.Id);
		};
	}
}
