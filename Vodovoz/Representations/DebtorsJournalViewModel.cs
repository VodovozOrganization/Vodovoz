using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QSReport;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Order = Vodovoz.Domain.Orders.Order;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Filters.ViewModels;
using System.Threading.Tasks;
using System.Threading;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.Views;

namespace Vodovoz.Representations
{
	public class DebtorsJournalViewModel : FilterableSingleEntityJournalViewModelBase<Order, CallTaskDlg, DebtorJournalNode, DebtorsJournalFilterViewModel>
	{
		private readonly IDebtorsParameters _debtorsParameters;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IEmailParametersProvider _emailParametersProvider;
		private readonly IAttachmentsViewModelFactory _attachmentsViewModelFactory;
		private readonly IEmailRepository _emailRepository;
		private readonly Employee _currentEmployee;
		private readonly bool _canSendBulkEmails;

		public DebtorsJournalViewModel(DebtorsJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, 
			IEmployeeRepository employeeRepository, IGtkTabsOpener gtkTabsOpener, IDebtorsParameters debtorsParameters, IEmailParametersProvider emailParametersProvider,
			IAttachmentsViewModelFactory attachmentsViewModelFactory, IEmailRepository emailRepository) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
			_attachmentsViewModelFactory = attachmentsViewModelFactory ?? throw new ArgumentNullException(nameof(attachmentsViewModelFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			TabName = "Журнал задолженности";
			SelectionMode = JournalSelectionMode.Multiple;
			this.employeeRepository = employeeRepository;
			DataLoader.ItemsListUpdated += UpdateFooterInfo;
			_debtorsParameters = debtorsParameters;
			_gtkTabsOpener = gtkTabsOpener;
			_currentEmployee = employeeRepository.GetEmployeeForCurrentUser(UoW);
			_canSendBulkEmails = commonServices.CurrentPermissionService.ValidatePresetPermission("can_send_bulk_emails");
		}

		IEmployeeRepository employeeRepository { get; set; }

		Task newTask;
		CancellationTokenSource cts = new CancellationTokenSource();

		private void UpdateFooterInfo(object sender, EventArgs e)
		{
			if(newTask?.Status == TaskStatus.Running) {
				cts.Cancel();
				cts = new CancellationTokenSource();
			}

			newTask = Task.Run(() => SetInfo(cts.Token));
		}

		protected void SetInfo(CancellationToken token)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				FooterInfo = $"Идёт загрузка данных...";
				var result = CountQueryFunction.Invoke(uow);
				if(token.IsCancellationRequested)
					return;

				FooterInfo = $"Сумма всех долгов по таре (по адресам): {result}  |  " + base.FooterInfo;
			}
		}

		private string footerInfo = "Идёт загрузка данных...";

		public override string FooterInfo {
			get => footerInfo;
			set => SetField(ref footerInfo, value);
		}

		protected override Func<IUnitOfWork, IQueryOver<Order>> ItemsSourceQueryFunction => (uow) => {
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			BottlesMovementOperation bottleMovementOperationAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			DebtorJournalNode resultAlias = null;
			Residue residueAlias = null;
			CallTask taskAlias = null;
			Order orderAlias = null;
			Order lastOrderAlias = null;
			Order orderCountAlias = null;
			OrderItem orderItemAlias = null;
			OrderItem orderItemsSubQueryAlias = null;
			DiscountReason discountReasonAlias = null;
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclatureSubQueryAlias = null;
			Order orderFromAnotherDPAlias = null;
			Email emailAlias = null;

			int hideSuspendedCounterpartyId = _debtorsParameters.GetSuspendedCounterpartyId;
			int hideCancellationCounterpartyId = _debtorsParameters.GetCancellationCounterpartyId;

			var ordersQuery = uow.Session.QueryOver(() => orderAlias);

			var bottleDebtByAddressQuery = QueryOver.Of(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.And(new Disjunction().Add(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
											.Add(Restrictions.On(() => deliveryPointAlias.Id).IsNull
														&& Restrictions.On(() => bottlesMovementAlias.DeliveryPoint.Id).IsNull
														&& Restrictions.On(() => bottlesMovementAlias.Order.Id).IsNotNull))
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
					Projections.Sum(() => bottlesMovementAlias.Returned),
					Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var residueQuery = QueryOver.Of(() => residueAlias)
			.Where(() => residueAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "IF(?1 IS NOT NULL,'есть', 'нет')"),
				NHibernateUtil.String,
				Projections.Property(() => residueAlias.Id)))
			.Take(1);

			var bottleDebtByClientQuery = QueryOver.Of(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));


			var TaskExistQuery = QueryOver.Of(() => taskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPointAlias.Id)
				.And(() => taskAlias.IsTaskComplete == false)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "IF(?1 IS NOT NULL,'grey', 'black')"),
					NHibernateUtil.String,
					Projections.Property(() => taskAlias.Id)))
				.Take(1);

			var countDeliveryPoint = QueryOver.Of(() => deliveryPointAlias)
				.Where(x => x.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Count(Projections.Id()));

			var counterpartyContactEmailsSubQuery = QueryOver.Of(() => emailAlias)
				.Where(() => emailAlias.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Property(() => emailAlias.Id));

			#region LastOrder

			var LastOrderIdQuery = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null) || (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.Id).Desc
				.Take(1);
			
			var olderLastOrderIdQueryWithDate = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => lastOrderAlias.DeliveryDate > FilterViewModel.EndDate.Value)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null) || (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Order>(p => p.Id));
			
			var LastOrderIdQueryWithDate = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => lastOrderAlias.DeliveryDate >= FilterViewModel.StartDate.Value
					&& lastOrderAlias.DeliveryDate <= FilterViewModel.EndDate.Value)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null)
					|| (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.WithSubquery.WhereNotExists(olderLastOrderIdQueryWithDate)
				.Select(Projections.Property<Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.Id).Desc;

			var LastOrderNomenclatures = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => nomenclatureAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => FilterViewModel.LastOrderNomenclature.Id == nomenclatureAlias.Id);

			var LastOrderDiscount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.DiscountReason, () => discountReasonAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => discountReasonAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => FilterViewModel.DiscountReason.Id == discountReasonAlias.Id);

			var orderFromAnotherDP = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.Where(() => orderFromAnotherDPAlias.Client.Id == counterpartyAlias.Id)
				.And(() => orderFromAnotherDPAlias.OrderStatus == OrderStatus.Closed)
				.And(() => orderFromAnotherDPAlias.DeliveryDate >= orderAlias.DeliveryDate)
				.And(new Disjunction().Add(() => orderFromAnotherDPAlias.DeliveryPoint.Id != deliveryPointAlias.Id)
						.Add(() => orderFromAnotherDPAlias.SelfDelivery && !orderAlias.SelfDelivery)
						.Add(() => !orderFromAnotherDPAlias.SelfDelivery && orderAlias.SelfDelivery)
				);

			var orderFromSuspended = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellation = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);
			
			
			var orderFromSuspendedWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellationWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);
			
			OrderStatus[] statusOptions = {OrderStatus.Canceled, OrderStatus.NotDelivered, OrderStatus.DeliveryCanceled};

			var subQuerryOrdersCount = QueryOver.Of(() => orderCountAlias)
				.Left.JoinAlias(() => orderCountAlias.OrderItems, () => orderItemsSubQueryAlias)
				.Left.JoinAlias(() => orderItemsSubQueryAlias.Nomenclature, () => nomenclatureSubQueryAlias)
				.Where(() => nomenclatureSubQueryAlias.Category == NomenclatureCategory.water)
				.Where(() => orderCountAlias.Client.Id == counterpartyAlias.Id)
				.Where(
					Restrictions.Not(Restrictions.In(Projections.Property<Order>(x => x.OrderStatus), statusOptions)))
				.Select(Projections.GroupProperty(
					Projections.Property<Order>(o => o.Client.Id))
				)
				.Where(Restrictions.Gt(Projections.CountDistinct(() => orderCountAlias.Id), 1));

			#endregion LastOrder

			if(FilterViewModel != null && FilterViewModel.EndDate != null)
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(LastOrderIdQueryWithDate.Take(1));
			else
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(LastOrderIdQuery);

			ordersQuery.JoinAlias(c => c.Client, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			#region Filter

			if(FilterViewModel != null) {
				if(FilterViewModel.Client != null)
					ordersQuery = ordersQuery.Where((arg) => arg.Client.Id == FilterViewModel.Client.Id);
				if(FilterViewModel.Address != null)
					ordersQuery = ordersQuery.Where((arg) => arg.DeliveryPoint.Id == FilterViewModel.Address.Id);
				if(FilterViewModel.OPF != null)
					ordersQuery = ordersQuery.Where(() => counterpartyAlias.PersonType == FilterViewModel.OPF.Value);
				if(FilterViewModel.LastOrderBottlesFrom != null)
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered >= FilterViewModel.LastOrderBottlesFrom.Value);
				if(FilterViewModel.LastOrderBottlesTo != null)
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered <= FilterViewModel.LastOrderBottlesTo.Value);
				if(FilterViewModel.StartDate != null)
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate >= FilterViewModel.StartDate.Value);
				if(FilterViewModel.EndDate != null)
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate <= FilterViewModel.EndDate.Value);
				if(FilterViewModel.EndDate != null && FilterViewModel.HideActiveCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereNotExists(orderFromAnotherDP);

				if(FilterViewModel.HideWithOneOrder) {
					ordersQuery.WithSubquery
						.WhereProperty(() => counterpartyAlias.Id)
						.In(subQuerryOrdersCount);
				}

				if(FilterViewModel.LastOrderNomenclature != null)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderNomenclatures);
				if(FilterViewModel.DiscountReason != null)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderDiscount);
				if(FilterViewModel.DebtBottlesFrom != null)
					ordersQuery = ordersQuery.WithSubquery.WhereValue(FilterViewModel.DebtBottlesFrom.Value).Le(bottleDebtByAddressQuery);
				if(FilterViewModel.DebtBottlesTo != null)
					ordersQuery = ordersQuery.WithSubquery.WhereValue(FilterViewModel.DebtBottlesTo.Value).Ge(bottleDebtByAddressQuery);
				if(!FilterViewModel.EndDate.HasValue && FilterViewModel.ShowSuspendedCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspendedWithoutDate);
				if(!FilterViewModel.EndDate.HasValue && FilterViewModel.ShowCancellationCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellationWithoutDate);
				if(FilterViewModel.EndDate.HasValue && FilterViewModel.ShowSuspendedCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspended);
				if(FilterViewModel.EndDate.HasValue && FilterViewModel.ShowCancellationCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellation);
				if(FilterViewModel.HideWithoutEmail)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(counterpartyContactEmailsSubQuery);
				}
			}

			#endregion Filter

			ordersQuery.Where(GetSearchCriterion(
					() => deliveryPointAlias.Id,
					() => deliveryPointAlias.CompiledAddress,
					() => counterpartyAlias.Id,
					() => counterpartyAlias.Name
			));

			var resultQuery = ordersQuery
				.JoinAlias(c => c.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(c => c.BottlesMovementOperation, () => bottleMovementOperationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.ClientId)
				   .Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.AddressId)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressName)
				   .Select(() => deliveryPointAlias.BottleReserv).WithAlias(() => resultAlias.Reserve)
				   .Select(() => counterpartyAlias.PersonType).WithAlias(() => resultAlias.OPF)
				   .Select(() => bottleMovementOperationAlias.Delivered).WithAlias(() => resultAlias.LastOrderBottles)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.LastOrderDate)
				   .SelectSubQuery(residueQuery).WithAlias(() => resultAlias.IsResidueExist)
				   .SelectSubQuery(bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery(bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				   .SelectSubQuery(TaskExistQuery).WithAlias(() => resultAlias.RowColor)
				   .SelectSubQuery(countDeliveryPoint).WithAlias(() => resultAlias.CountOfDeliveryPoint))
				.SetTimeout(180)
				.TransformUsing(Transformers.AliasToBean<DebtorJournalNode>());

			return resultQuery;
		};

		protected Func<IUnitOfWork, int> CountQueryFunction => (uow) => {
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			BottlesMovementOperation bottleMovementOperationAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			Order orderAlias = null;
			Order orderCountAlias = null;
			Order lastOrderAlias = null;
			OrderItem orderItemAlias = null;
			OrderItem orderItemsSubQueryAlias = null;
			DiscountReason discountReasonAlias = null;
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclatureSubQueryAlias = null;
			Order orderFromAnotherDPAlias = null;
			Email emailAlias = null;

			int hideSuspendedCounterpartyId = _debtorsParameters.GetSuspendedCounterpartyId;
			int hideCancellationCounterpartyId = _debtorsParameters.GetCancellationCounterpartyId;

			var ordersQuery = uow.Session.QueryOver(() => orderAlias);

			var bottleDebtByAddressQuery = QueryOver.Of(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.And(new Disjunction().Add(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
											.Add(Restrictions.On(() => deliveryPointAlias.Id).IsNull
														&& Restrictions.On(() => bottlesMovementAlias.DeliveryPoint.Id).IsNull
														&& Restrictions.On(() => bottlesMovementAlias.Order.Id).IsNotNull))
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
					Projections.Sum(() => bottlesMovementAlias.Returned),
					Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			OrderStatus[] statusOptions = {OrderStatus.Canceled, OrderStatus.NotDelivered, OrderStatus.DeliveryCanceled};

			var subQuerryOrdersCount = QueryOver.Of(() => orderCountAlias)
				.Left.JoinAlias(() => orderCountAlias.OrderItems, () => orderItemsSubQueryAlias)
				.Left.JoinAlias(() => orderItemsSubQueryAlias.Nomenclature, () => nomenclatureSubQueryAlias)
				.Where(() => nomenclatureSubQueryAlias.Category == NomenclatureCategory.water)
				.Where(() => orderCountAlias.Client.Id == counterpartyAlias.Id)
				.Where(
					Restrictions.Not(Restrictions.In(Projections.Property<Order>(x => x.OrderStatus), statusOptions)))
				.Select(Projections.GroupProperty(
					Projections.Property<Order>(o => o.Client.Id))
				)
				.Where(Restrictions.Gt(Projections.CountDistinct(() => orderCountAlias.Id), 1));

			var counterpartyContactEmailsSubQuery = QueryOver.Of(() => emailAlias)
				.Where(() => emailAlias.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Property(() => emailAlias.Id));

			#region LastOrder

			var LastOrderIdQuery = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null) || (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.Id).Desc
				.Take(1);

			var olderLastOrderIdQueryWithDate = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => lastOrderAlias.DeliveryDate > FilterViewModel.EndDate.Value)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null) || (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Order>(p => p.Id));
			
			var LastOrderIdQueryWithDate = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => lastOrderAlias.DeliveryDate >= FilterViewModel.StartDate.Value
							&& lastOrderAlias.DeliveryDate <= FilterViewModel.EndDate.Value)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null)
							|| (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.WithSubquery.WhereNotExists(olderLastOrderIdQueryWithDate)
				.Select(Projections.Property<Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.Id).Desc;

			var LastOrderNomenclatures = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => nomenclatureAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => FilterViewModel.LastOrderNomenclature.Id == nomenclatureAlias.Id);

			var LastOrderDiscount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.DiscountReason, () => discountReasonAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => discountReasonAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => FilterViewModel.DiscountReason.Id == discountReasonAlias.Id);

			var orderFromAnotherDP = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.Where(() => orderFromAnotherDPAlias.Client.Id == counterpartyAlias.Id)
				.And(() => orderFromAnotherDPAlias.OrderStatus == OrderStatus.Closed)
				.And(() => orderFromAnotherDPAlias.DeliveryDate >= orderAlias.DeliveryDate)
				.And(new Disjunction().Add(() => orderFromAnotherDPAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
						.Add(() => orderFromAnotherDPAlias.SelfDelivery && !orderAlias.SelfDelivery)
						.Add(() => !orderFromAnotherDPAlias.SelfDelivery && orderAlias.SelfDelivery));
			var ordersCountSubQuery = QueryOver.Of(() => orderCountAlias)
				.Where(() => orderCountAlias.Client.Id == counterpartyAlias.Id)
				.ToRowCountQuery();

			var orderFromSuspended = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellation = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);
			
			var orderFromSuspendedWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellationWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(LastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);

			#endregion LastOrder

			if(FilterViewModel != null && FilterViewModel.EndDate != null)
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(LastOrderIdQueryWithDate.Take(1));
			else
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(LastOrderIdQuery);

			#region Filter

			if(FilterViewModel != null) {
				if(FilterViewModel.Client != null)
					ordersQuery = ordersQuery.Where((arg) => arg.Client.Id == FilterViewModel.Client.Id);
				if(FilterViewModel.Address != null)
					ordersQuery = ordersQuery.Where((arg) => arg.DeliveryPoint.Id == FilterViewModel.Address.Id);
				if(FilterViewModel.OPF != null)
					ordersQuery = ordersQuery.Where(() => counterpartyAlias.PersonType == FilterViewModel.OPF.Value);
				if(FilterViewModel.LastOrderBottlesFrom != null)
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered >= FilterViewModel.LastOrderBottlesFrom.Value);
				if(FilterViewModel.LastOrderBottlesTo != null)
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered <= FilterViewModel.LastOrderBottlesTo.Value);
				if(FilterViewModel.StartDate != null)
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate >= FilterViewModel.StartDate.Value);
				if(FilterViewModel.EndDate != null)
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate <= FilterViewModel.EndDate.Value);
				if(FilterViewModel.EndDate != null && FilterViewModel.HideActiveCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereNotExists(orderFromAnotherDP);
				if(FilterViewModel.LastOrderNomenclature != null)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderNomenclatures);
				if(FilterViewModel.DiscountReason != null)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(LastOrderDiscount);
				if(FilterViewModel.DebtBottlesFrom != null)
					ordersQuery = ordersQuery.WithSubquery.WhereValue(FilterViewModel.DebtBottlesFrom.Value).Le(bottleDebtByAddressQuery);
				if(FilterViewModel.DebtBottlesTo != null)
					ordersQuery = ordersQuery.WithSubquery.WhereValue(FilterViewModel.DebtBottlesTo.Value).Ge(bottleDebtByAddressQuery);
				if(FilterViewModel.HideWithOneOrder)
					ordersQuery.WithSubquery
						.WhereProperty(() => counterpartyAlias.Id)
						.In(subQuerryOrdersCount);
				if(!FilterViewModel.EndDate.HasValue && FilterViewModel.ShowSuspendedCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspendedWithoutDate);
				if(!FilterViewModel.EndDate.HasValue && FilterViewModel.ShowCancellationCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellationWithoutDate);
				if(FilterViewModel.EndDate.HasValue && FilterViewModel.ShowSuspendedCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspended);
				if(FilterViewModel.EndDate.HasValue && FilterViewModel.ShowCancellationCounterparty)
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellation);
				if(FilterViewModel.HideWithoutEmail)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(counterpartyContactEmailsSubQuery);
				}
			}


			#endregion Filter

			ordersQuery.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => deliveryPointAlias.CompiledAddress,
				() => counterpartyAlias.Id,
				() => counterpartyAlias.Name
			));

			IProjection sumProj = Projections.Sum(Projections.SubQuery(bottleDebtByAddressQuery));

			var queryResult = ordersQuery
				.Left.JoinAlias(c => c.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(c => c.Client, () => counterpartyAlias)
				.Left.JoinAlias(c => c.BottlesMovementOperation, () => bottleMovementOperationAlias)
				.Select(sumProj).UnderlyingCriteria.SetTimeout(180).UniqueResult<int>();
			return queryResult;
		};

		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(new JournalAction("Акт по бутылям и залогам(по клиенту)", x => true, x => true, selectedItems => {
				var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null) {
					OpenReport(selectedNode.ClientId);
				}
			}));

			PopupActionsList.Add(new JournalAction("Акт по бутылям и залогам(по точке доставки)", x => true, x => true, selectedItems => {
				var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null) {
					OpenReport(selectedNode.ClientId, selectedNode.AddressId);
				}
			}));

			PopupActionsList.Add(NewJournalActionForOpenCounterpartyDlg());

			PopupActionsList.Add(new JournalAction("Создать задачу", x => true, x => true, selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
					var selectedNode = selectedNodes.FirstOrDefault();
					if(selectedNode != null)
						CreateTask(selectedNodes.ToArray());
				}
			));

		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			NodeActionsList.Add(new JournalAction("Создать задачи", x => true, x => true, selectedItems => CreateTask(selectedItems.AsEnumerable().OfType<DebtorJournalNode>().ToArray())));

			NodeActionsList.Add(new JournalAction("Акт по бутылям и залогам", x => true, x => true, selectedItems => {
				var selectedNodes = selectedItems.Cast<DebtorJournalNode>();
				var selectedNode = selectedNodes.FirstOrDefault();
				if(selectedNode != null) {
					OpenReport(selectedNode.ClientId, selectedNode.AddressId);
				}
			}));

			NodeActionsList.Add(new JournalAction("Печатная форма", x => true, x => true, selectedItems => {
				OpenPrintingForm();
			}));

			NodeActionsList.Add(NewJournalActionForOpenCounterpartyDlg());

			NodeActionsList.Add(NewJournalActionForOpenBulkEmail());
		}

		private JournalAction NewJournalActionForOpenCounterpartyDlg()
		{
			return new JournalAction("Открыть клиента", x => true, x => true, selectedItems =>
			{
				var selectedNode = selectedItems.Cast<DebtorJournalNode>().FirstOrDefault();
				if(selectedNode != null)
				{
					_gtkTabsOpener.OpenCounterpartyDlg(this, selectedNode.ClientId);
				}
			});
		}

		private JournalAction NewJournalActionForOpenBulkEmail()
		{
			return new JournalAction("Массовая рассылка", x => true, x => _canSendBulkEmails, selectedItems =>
			{
				var bulkEmailViewModel = new BulkEmailViewModel(null, UnitOfWorkFactory, ItemsSourceQueryFunction, _emailParametersProvider,
						commonServices, _attachmentsViewModelFactory, _currentEmployee, _emailRepository);
				
				var bulkEmailView = new BulkEmailView(bulkEmailViewModel);
				bulkEmailView.Show();
			});
		}

		protected override Func<CallTaskDlg> CreateDialogFunction => () => new CallTaskDlg();

		protected override Func<DebtorJournalNode, CallTaskDlg> OpenDialogFunction => (node) =>
		{
			return new CallTaskDlg(node.ClientId, node.AddressId);
		};

		public void OpenReport(int counterpartyId, int deliveryPointId = -1)
		{
			var dlg = CreateReportDlg(counterpartyId, deliveryPointId);
			TabParent.AddTab(dlg, this, false);
		}

		public void OpenPrintingForm()
		{
			var reportInfo = new QS.Report.ReportInfo {
				Title = TabName,
				Identifier = "Client.BottleDebtorsJournal",
				Parameters = new Dictionary<string, object>
				{
					{ "discount_reason_id", FilterViewModel?.DiscountReason?.Id ?? 0 },
					{ "nomenclature_id", FilterViewModel?.LastOrderNomenclature?.Id ?? 0},
					{ "StartDate", FilterViewModel?.StartDate},
					{ "EndDate", FilterViewModel?.EndDate},
					{ "OrderBottlesFrom", FilterViewModel?.LastOrderBottlesFrom?.ToString() ?? String.Empty},
					{ "OrderBottlesTo", FilterViewModel?.LastOrderBottlesTo?.ToString() ?? String.Empty},
					{ "AddressId", FilterViewModel?.Address?.Id ?? 0},
					{ "CounterpartyId", FilterViewModel?.Client?.Id ?? 0},
					{ "OPF", FilterViewModel?.OPF?.ToString() ?? String.Empty},
					{ "DebtBottlesFrom", FilterViewModel.DebtBottlesFrom != null ? FilterViewModel?.DebtBottlesFrom.Value.ToString() : ""},
					{ "DebtBottlesTo", FilterViewModel.DebtBottlesTo != null ? FilterViewModel?.DebtBottlesTo.Value.ToString() : ""},
					{ "HideActiveCounterparty", FilterViewModel.HideActiveCounterparty ? "true" : ""},
					{ "HideWithOneOrder", FilterViewModel.HideWithOneOrder ? "true" : ""}
				}
			};
			var dlg = new ReportViewDlg(reportInfo);
			TabParent.AddTab(dlg, this, false);
		}

		private ReportViewDlg CreateReportDlg(int counterpartyId, int deliveryPointId = -1)
		{
			var reportInfo = new QS.Report.ReportInfo {
				Title = "Акт по бутылям-залогам",
				Identifier = "Client.SummaryBottlesAndDeposits",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", null },
					{ "endDate", null },
					{ "client_id", counterpartyId},
					{ "delivery_point_id", deliveryPointId}
				}
			};
			return new ReportViewDlg(reportInfo);
		}

		public int CreateTask(DebtorJournalNode[] bottleDebtors)
		{
			int newTaskCount = 0;
			foreach(var item in bottleDebtors) {
				if(item == null)
					continue;
				CallTask task = new CallTask {
					TaskCreator = _currentEmployee,
					DeliveryPoint = UoW.GetById<DeliveryPoint>(item.AddressId),
					Counterparty = UoW.GetById<Counterparty>(item.ClientId),
					CreationDate = DateTime.Now,
					EndActivePeriod = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
					Source = TaskSource.MassCreation
				};
				newTaskCount++;
				UoW.Save(task);
			}
			commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Создано задач: {newTaskCount.ToString()}");
			UoW.Commit();
			return newTaskCount;
		}
	}
}
