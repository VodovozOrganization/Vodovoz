using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Report;
using QS.Services;
using QSReport;
using RabbitMQ.Infrastructure;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.ViewModels.ViewModels.Reports.DebtorsJournalReport;
using Vodovoz.Views;
using Expression = System.Linq.Expressions.Expression;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Representations
{
	public class DebtorsJournalViewModel : EntityJournalViewModelBase<Order, CallTaskViewModel, DebtorJournalNode>
	{
		private readonly OrderStatus[] _notDeliveredStatuses = { OrderStatus.Canceled, OrderStatus.NotDelivered, OrderStatus.DeliveryCanceled };

		private readonly IDebtorsSettings _debtorsParameters;
		private readonly ILogger<BulkEmailViewModel> _loggerBulkEmailViewModel;
		private readonly ILogger<RabbitMQConnectionFactory> _rabbitConnectionFactoryLogger;
		private readonly DebtorsJournalFilterViewModel _filterViewModel;
		private readonly IInteractiveService _interactiveService;
		private readonly ICommonServices _commonServices;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IEmailSettings _emailSettings;
		private readonly IAttachmentsViewModelFactory _attachmentsViewModelFactory;
		private readonly IEmailRepository _emailRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private readonly Employee _currentEmployee;
		private readonly bool _canSendBulkEmails;
		private Task _newTask;
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private string _footerInfo = "Идёт загрузка данных...";
		private readonly decimal _waterSemiozeriePrice;
		private readonly int _waterSemiozerieId;

		public DebtorsJournalViewModel(
			ILogger<BulkEmailViewModel> loggerBulkEmailViewModel,
			ILogger<RabbitMQConnectionFactory> rabbitConnectionFactoryLogger,
			DebtorsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeRepository employeeRepository,
			IGtkTabsOpener gtkTabsOpener,
			IDebtorsSettings debtorsParameters,
			IEmailSettings emailSettings,
			IAttachmentsViewModelFactory attachmentsViewModelFactory,
			IEmailRepository emailRepository,
			IFileDialogService fileDialogService,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			IGenericRepository<Nomenclature> nomenclatureRepository,
			INomenclatureSettings nomenclatureSettings)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(nomenclatureRepository == null)
			{
				throw new ArgumentNullException(nameof(nomenclatureRepository));
			}

			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_attachmentsViewModelFactory = attachmentsViewModelFactory ?? throw new ArgumentNullException(nameof(attachmentsViewModelFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory ?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			_debtorsParameters = debtorsParameters ?? throw new ArgumentNullException(nameof(debtorsParameters));
			_loggerBulkEmailViewModel = loggerBulkEmailViewModel ?? throw new ArgumentNullException(nameof(loggerBulkEmailViewModel));
			_rabbitConnectionFactoryLogger = rabbitConnectionFactoryLogger;
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			_currentEmployee = employeeRepository.GetEmployeeForCurrentUser(UoW);

			_canSendBulkEmails = currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmailPermissions.CanSendBulkEmails);

			_waterSemiozerieId = nomenclatureSettings.WaterSemiozerieId;
			_waterSemiozeriePrice = nomenclatureRepository.Get(UoW, n => n.Id == _waterSemiozerieId)
				?.FirstOrDefault()
				?.NomenclaturePrice
				?.FirstOrDefault(x => x.MinCount == 1)
				?.Price ?? 0m;

			filterViewModel.Journal = this;
			JournalFilter = _filterViewModel;
			filterViewModel.OnFiltered += OnFilterFiltered;

			TabName = "Журнал задолженности";
			SelectionMode = JournalSelectionMode.Multiple;
			DataLoader.ItemsListUpdated += UpdateFooterInfo;
			CreatePopupActions();
		}

		private void OnFilterFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override string FooterInfo
		{
			get => _footerInfo;
			set => SetField(ref _footerInfo, value);
		}

		private void UpdateFooterInfo(object sender, EventArgs e)
		{
			if(_newTask?.Status == TaskStatus.Running)
			{
				_cts.Cancel();
				_cts = new CancellationTokenSource();
			}

			_newTask = Task.Run(() => SetInfo(_cts.Token));
		}

		protected void SetInfo(CancellationToken token)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				FooterInfo = $"Идёт загрузка данных...";
				var result = CountQueryFunction.Invoke(uow);

				if(token.IsCancellationRequested)
				{
					return;
				}

				FooterInfo = $"Сумма всех долгов по таре (по адресам): {result}  |  " + base.FooterInfo;
			}
		}
		
		private Expression<Func<bool>> GetFixPriceExpression(NomenclatureFixedPrice nomenclatureFixedPriceAlias, DeliveryPoint deliveryPointAlias)
		{
			Expression<Func<bool>> resultExpression =
				() => deliveryPointAlias.Id == nomenclatureFixedPriceAlias.DeliveryPoint.Id
				      && nomenclatureFixedPriceAlias.Nomenclature.Id == _waterSemiozerieId
				      && nomenclatureFixedPriceAlias.MinCount == 1;

			if(_filterViewModel.FixPriceFrom.HasValue)
			{
				Expression<Func<bool>> expression = () => nomenclatureFixedPriceAlias.Price >= _filterViewModel.FixPriceFrom;

				resultExpression = Expression.Lambda<Func<bool>>
				(
					Expression.AndAlso
					(
						resultExpression.Body,
						expression.Body
					)
				);
			}

			if(_filterViewModel.FixPriceTo.HasValue)
			{
				Expression<Func<bool>> expression = () => nomenclatureFixedPriceAlias.Price <= _filterViewModel.FixPriceTo;

				resultExpression = Expression.Lambda<Func<bool>>
				(
					Expression.AndAlso
					(
						resultExpression.Body,
						expression.Body
					)
				);
			}

			return resultExpression;
		}

		protected Func<IUnitOfWork, int> CountQueryFunction => (uow) =>
		{
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			Employee salesManagerAlias = null;
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
			CallTask taskAlias = null;
			NomenclatureFixedPrice nomenclatureFixedPriceAlias = null;

			int hideSuspendedCounterpartyId = _debtorsParameters.GetSuspendedCounterpartyId;
			int hideCancellationCounterpartyId = _debtorsParameters.GetCancellationCounterpartyId;

			var ordersQuery = uow.Session.QueryOver(() => orderAlias);
			
			#region FixPrice
			
			ordersQuery.JoinEntityAlias(() => nomenclatureFixedPriceAlias,
				GetFixPriceExpression(nomenclatureFixedPriceAlias, deliveryPointAlias),
				JoinType.LeftOuterJoin);
			
			#endregion FixPrice

			var bottleDebtByAddressQuery = QueryOver.Of(() => bottlesMovementAlias)
				.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
				.And(new Disjunction()
					.Add(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
					.Add(Restrictions.On(() => deliveryPointAlias.Id).IsNull
						&& Restrictions.On(() => bottlesMovementAlias.DeliveryPoint.Id).IsNull
						&& Restrictions.On(() => bottlesMovementAlias.Order.Id).IsNotNull))
				.Select(
					Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
						NHibernateUtil.Int32, new IProjection[] {
						Projections.Sum(() => bottlesMovementAlias.Returned),
						Projections.Sum(() => bottlesMovementAlias.Delivered)}));

			var subQuerryOrdersCount = QueryOver.Of(() => orderCountAlias)
				.Left.JoinAlias(() => orderCountAlias.OrderItems, () => orderItemsSubQueryAlias)
				.Left.JoinAlias(() => orderItemsSubQueryAlias.Nomenclature, () => nomenclatureSubQueryAlias)
				.Where(() => nomenclatureSubQueryAlias.Category == NomenclatureCategory.water)
				.Where(() => orderCountAlias.Client.Id == counterpartyAlias.Id)
				.Where(
					Restrictions.Not(Restrictions.In(Projections.Property<Order>(x => x.OrderStatus), _notDeliveredStatuses)))
				.Select(Projections.GroupProperty(
					Projections.Property<Order>(o => o.Client.Id)));

			var counterpartyContactEmailsSubQuery = QueryOver.Of(() => emailAlias)
				.Where(() => emailAlias.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Property(() => emailAlias.Id));

			
			
			var countDeliveryPoint = QueryOver.Of(() => deliveryPointAlias)
				.Where(x => x.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Count(Projections.Id()));

			var countDeliveryPointFixedPricesSubQuery = QueryOver.Of(() => nomenclatureFixedPriceAlias)
				.Where(() => nomenclatureFixedPriceAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select(Projections.Id());

			#region LastOrder

			var lastOrderIdQuery = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null)
					|| (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.Id).Desc
				.Take(1);

			var olderLastOrderIdQueryWithDate = GetOlderLastOrderIdWithDateQuery(
				lastOrderAlias, counterpartyAlias, orderAlias, deliveryPointAlias);

			var lastOrderIdQueryWithDate = GetLastOrderIdWithDateQuery(
				lastOrderAlias, counterpartyAlias, orderAlias, deliveryPointAlias, olderLastOrderIdQueryWithDate);

			var lastOrderNomenclatures = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => nomenclatureAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => _filterViewModel.LastOrderNomenclature.Id == nomenclatureAlias.Id);

			var lastOrderDiscount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.DiscountReason, () => discountReasonAlias, JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => discountReasonAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => _filterViewModel.DiscountReason.Id == discountReasonAlias.Id);

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
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellation = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);

			var orderFromSuspendedWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellationWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);

			var taskExistQuery = QueryOver.Of(() => taskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPointAlias.Id)
				.And(() => taskAlias.IsTaskComplete == false)
				.Select(Projections.Property(() => taskAlias.Id))
				.Take(1);

			#endregion LastOrder

			if(_filterViewModel != null && _filterViewModel.EndDate != null)
			{
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(lastOrderIdQueryWithDate.Take(1));
			}
			else
			{
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(lastOrderIdQuery);
			}

			if(_filterViewModel != null && _filterViewModel.DebtorsTaskStatus != null)
			{
				if(_filterViewModel.DebtorsTaskStatus.Value == DebtorsTaskStatus.HasTask)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(taskExistQuery);
				}
				else
				{
					ordersQuery = ordersQuery.WithSubquery.WhereNotExists(taskExistQuery);
				}
			}
			
			#region Filter

			if(_filterViewModel != null)
			{
				if(_filterViewModel.Client != null)
				{
					ordersQuery = ordersQuery.Where((arg) => arg.Client.Id == _filterViewModel.Client.Id);
				}

				if(_filterViewModel.Address != null)
				{
					ordersQuery = ordersQuery.Where((arg) => arg.DeliveryPoint.Id == _filterViewModel.Address.Id);
				}

				if(_filterViewModel.SalesManager != null)
				{
					ordersQuery = ordersQuery
						.Where(() => counterpartyAlias.SalesManager.Id == _filterViewModel.SalesManager.Id);
				}

				if(_filterViewModel.OPF != null)
				{
					ordersQuery = ordersQuery.Where(() => counterpartyAlias.PersonType == _filterViewModel.OPF.Value);
				}

				if(_filterViewModel.LastOrderBottlesFrom != null)
				{
					ordersQuery =
						ordersQuery.Where(() => bottleMovementOperationAlias.Delivered >= _filterViewModel.LastOrderBottlesFrom.Value);
				}

				if(_filterViewModel.LastOrderBottlesTo != null)
				{
					ordersQuery =
						ordersQuery.Where(() => bottleMovementOperationAlias.Delivered <= _filterViewModel.LastOrderBottlesTo.Value);
				}

				if(_filterViewModel.DeliveryPointsFrom != null)
				{
					ordersQuery = ordersQuery.Where(Restrictions.Ge(Projections.SubQuery(countDeliveryPoint), _filterViewModel.DeliveryPointsFrom.Value));
				}

				if(_filterViewModel.DeliveryPointsTo != null)
				{
					ordersQuery = ordersQuery.Where(Restrictions.Le(Projections.SubQuery(countDeliveryPoint), _filterViewModel.DeliveryPointsTo.Value));
				}

				if(_filterViewModel.StartDate != null)
				{
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate >= _filterViewModel.StartDate.Value);
				}

				if(_filterViewModel.EndDate != null)
				{
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate <= _filterViewModel.EndDate.Value);
				}

				if(_filterViewModel.EndDate != null && _filterViewModel.HideActiveCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereNotExists(orderFromAnotherDP);
				}

				if(_filterViewModel.LastOrderNomenclature != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(lastOrderNomenclatures);
				}

				if(_filterViewModel.DiscountReason != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(lastOrderDiscount);
				}

				if(_filterViewModel.DebtBottlesFrom != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereValue(_filterViewModel.DebtBottlesFrom.Value).Le(bottleDebtByAddressQuery);
				}

				if(_filterViewModel.DebtBottlesTo != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereValue(_filterViewModel.DebtBottlesTo.Value).Ge(bottleDebtByAddressQuery);
				}

				if(_filterViewModel.WithOneOrder != null)
				{
					var countProjection = Projections.CountDistinct(() => orderCountAlias.Id);

					subQuerryOrdersCount.Where(_filterViewModel.WithOneOrder.Value
						? Restrictions.Eq(countProjection, 1)
						: Restrictions.Not(Restrictions.Eq(countProjection, 1)));

					ordersQuery.WithSubquery
						.WhereProperty(() => counterpartyAlias.Id)
						.In(subQuerryOrdersCount);
				}

				if(!_filterViewModel.EndDate.HasValue && _filterViewModel.ShowSuspendedCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspendedWithoutDate);
				}

				if(!_filterViewModel.EndDate.HasValue && _filterViewModel.ShowCancellationCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellationWithoutDate);
				}

				if(_filterViewModel.EndDate.HasValue && _filterViewModel.ShowSuspendedCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspended);
				}

				if(_filterViewModel.EndDate.HasValue && _filterViewModel.ShowCancellationCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellation);
				}

				if(_filterViewModel.HideWithoutEmail)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(counterpartyContactEmailsSubQuery);
				}

				if(_filterViewModel.HideWithoutFixedPrices)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(countDeliveryPointFixedPricesSubQuery);
					//.Where(Restrictions.Gt(Projections.SubQuery(countDeliveryPointFixedPricesSubQuery), 0));
				}

				if(_filterViewModel.SelectedDeliveryPointCategory != null)
				{
					ordersQuery.Where(() => deliveryPointAlias.Category.Id == _filterViewModel.SelectedDeliveryPointCategory.Id);
				}

				if(_filterViewModel.HideExcludeFromAutoCalls)
				{
					ordersQuery.Where(() => !counterpartyAlias.ExcludeFromAutoCalls);
				}

				if(_filterViewModel.FixPriceFrom != null || _filterViewModel.FixPriceTo != null)
				{
					ordersQuery = ordersQuery.Where(() =>
						nomenclatureFixedPriceAlias.Id != null
						|| (_waterSemiozeriePrice >= (_filterViewModel.FixPriceFrom ?? _waterSemiozeriePrice)
						    && _waterSemiozeriePrice <= (_filterViewModel.FixPriceTo ?? _waterSemiozeriePrice)));
				}
			}

			#endregion Filter

			ordersQuery.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => deliveryPointAlias.CompiledAddress,
				() => counterpartyAlias.Id,
				() => counterpartyAlias.Name));

			IProjection sumProj = Projections.Sum(Projections.SubQuery(bottleDebtByAddressQuery));

			var queryResult = ordersQuery
				.JoinAlias(c => c.DeliveryPoint,
					() => deliveryPointAlias,
					(_filterViewModel != null && _filterViewModel.HideWithoutFixedPrices)
						? JoinType.InnerJoin
						: JoinType.LeftOuterJoin)
				.Left.JoinAlias(c => c.Client, () => counterpartyAlias)
				.Left.JoinAlias(c => c.BottlesMovementOperation, () => bottleMovementOperationAlias)
				.Select(sumProj).UnderlyingCriteria.SetTimeout(300).UniqueResult<int>();

			return queryResult;
		};

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();

			PopupActionsList.Add(new JournalAction(
				"Акт по бутылям и залогам(по клиенту)",
				x => true,
				x => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems
						.Cast<DebtorJournalNode>();

					var selectedNode = selectedNodes
						.FirstOrDefault();

					if(selectedNode != null)
					{
						OpenReport(selectedNode.ClientId);
					}
				}));

			PopupActionsList.Add(new JournalAction(
				"Акт по бутылям и залогам(по точке доставки)",
				x => true,
				x => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems
						.Cast<DebtorJournalNode>();

					var selectedNode = selectedNodes
						.FirstOrDefault();

					if(selectedNode != null)
					{
						OpenReport(selectedNode.ClientId, selectedNode.AddressId);
					}
				}));

			PopupActionsList.Add(NewJournalActionForOpenCounterpartyDlg());

			PopupActionsList.Add(new JournalAction(
				"Создать задачу",
				x => true,
				x => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems
						.Cast<DebtorJournalNode>();

					var selectedNode = selectedNodes
						.FirstOrDefault();

					if(selectedNode != null)
					{
						CreateTask(selectedNodes.ToArray());
					}
				}));
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			NodeActionsList.Add(new JournalAction(
				"Создать задачи",
				x => true,
				x => true,
				selectedItems => CreateTask(selectedItems
					.AsEnumerable()
					.OfType<DebtorJournalNode>()
					.ToArray())));

			NodeActionsList.Add(new JournalAction(
				"Акт по бутылям и залогам",
				x => true,
				x => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems
						.Cast<DebtorJournalNode>();

					var selectedNode = selectedNodes
						.FirstOrDefault();

					if(selectedNode != null)
					{
						OpenReport(selectedNode.ClientId, selectedNode.AddressId);
					}
				}));

			NodeActionsList.Add(new JournalAction(
				"Экспорт в Эксель",
				x => true,
				x => true,
				selectedItems =>
				{
					ExportToExcel();
				}));

			NodeActionsList.Add(NewJournalActionForOpenCounterpartyDlg());

			NodeActionsList.Add(NewJournalActionForOpenBulkEmail());
		}

		private JournalAction NewJournalActionForOpenCounterpartyDlg()
		{
			return new JournalAction(
				"Открыть клиента",
				x => true,
				x => true,
				selectedItems =>
				{
					var selectedNode = selectedItems
						.Cast<DebtorJournalNode>()
						.FirstOrDefault();

					if(selectedNode != null)
					{
						_gtkTabsOpener.OpenCounterpartyDlg(this, selectedNode.ClientId);
					}
				});
		}

		private JournalAction NewJournalActionForOpenBulkEmail()
		{
			return new JournalAction(
				"Массовая рассылка",
				x => true,
				x => _canSendBulkEmails,
				selectedItems =>
				{
					var bulkEmailViewModel = new BulkEmailViewModel(
						_loggerBulkEmailViewModel,
						_rabbitConnectionFactoryLogger,
						null,
						UnitOfWorkFactory,
						ItemsQuery,
						_emailSettings,
						_commonServices,
						_attachmentsViewModelFactory,
						_currentEmployee,
						_emailRepository,
						_attachedFileInformationsViewModelFactory);

					var bulkEmailView = new BulkEmailView(bulkEmailViewModel);

					bulkEmailView.Show();
				});
		}

		//Имена параметров должны быть такие же как и в основном запросе
		private QueryOver<Order, Order> GetLastOrderIdWithDateQuery(
			Order lastOrderAlias,
			Counterparty counterpartyAlias,
			Order orderAlias,
			DeliveryPoint deliveryPointAlias,
			QueryOver<Order, Order> olderLastOrderIdQueryWithDate)
		{
			var query = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null)
					|| (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.WithSubquery.WhereNotExists(olderLastOrderIdQueryWithDate)
				.Select(Projections.Property<Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.Id).Desc;

			if(_filterViewModel?.StartDate != null)
			{
				query.And(() => lastOrderAlias.DeliveryDate >= _filterViewModel.StartDate);
			}

			if(_filterViewModel?.EndDate != null)
			{
				query.And(() => lastOrderAlias.DeliveryDate <= _filterViewModel.EndDate);
			}

			return query;
		}

		//Имена параметров должны быть такие же как и в основном запросе
		private QueryOver<Order, Order> GetOlderLastOrderIdWithDateQuery(
			Order lastOrderAlias,
			Counterparty counterpartyAlias,
			Order orderAlias,
			DeliveryPoint deliveryPointAlias)
		{
			var query = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null)
					|| (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Order>(p => p.Id));

			if(_filterViewModel?.EndDate != null)
			{
				query.And(() => lastOrderAlias.DeliveryDate > _filterViewModel.EndDate);
			}

			return query;
		}

		protected override void EditEntityDialog(DebtorJournalNode node)
		{
			NavigationManager.OpenViewModel<CallTaskViewModel>(this, OpenPageOptions.AsSlave, vm =>
			{
				vm.SetCounterpartyById(node.ClientId);
				vm.SetDeliveryPointById(node.AddressId);
			});
		}

		public void OpenReport(int counterpartyId, int deliveryPointId = -1)
		{
			var dlg = CreateReportDlg(counterpartyId, deliveryPointId);
			TabParent.AddTab(dlg, this, false);
		}

		public void ExportToExcel()
		{
			var rows = ItemsQuery(UoW).List<DebtorJournalNode>();
			var report = new DebtorsJournalReport(rows, _fileDialogService);
			report.Export();
		}

		private ReportViewDlg CreateReportDlg(int counterpartyId, int deliveryPointId = -1)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = "Акт по бутылям-залогам";
			reportInfo.Identifier = "Client.SummaryBottlesAndDeposits";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "startDate", null },
				{ "endDate", null },
				{ "client_id", counterpartyId},
				{ "delivery_point_id", deliveryPointId}
			};

			return new ReportViewDlg(reportInfo);
		}

		public int CreateTask(DebtorJournalNode[] bottleDebtors)
		{
			int newTaskCount = 0;

			foreach(var item in bottleDebtors)
			{
				if(item == null)
				{
					continue;
				}

				CallTask task = new CallTask
				{
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

			
			_interactiveService.ShowMessage(ImportanceLevel.Info, $"Создано задач: {newTaskCount.ToString()}");
			UoW.Commit();

			return newTaskCount;
		}

		protected override IQueryOver<Order> ItemsQuery(IUnitOfWork uow)
		{
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
			Phone phoneAlias = null;
			NomenclatureFixedPrice nomenclatureFixedPriceAlias = null;

			int hideSuspendedCounterpartyId = _debtorsParameters.GetSuspendedCounterpartyId;
			int hideCancellationCounterpartyId = _debtorsParameters.GetCancellationCounterpartyId;

			var ordersQuery = uow.Session.QueryOver(() => orderAlias);
			
			#region FixPrice
			
			ordersQuery.JoinEntityAlias(() => nomenclatureFixedPriceAlias,
				GetFixPriceExpression(nomenclatureFixedPriceAlias, deliveryPointAlias),
				JoinType.LeftOuterJoin);

			var fixedPriceProjection = CustomProjections.Coalesce(NHibernateUtil.Decimal, 
				Projections.Property(() => nomenclatureFixedPriceAlias.Price), Projections.Constant(_waterSemiozeriePrice));
			
			#endregion FixPrice

			var bottleDebtByAddressQuery = QueryOver.Of(() => bottlesMovementAlias)
				.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
				.And(new Disjunction()
					.Add(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
					.Add(Restrictions.On(() => deliveryPointAlias.Id).IsNull
						&& Restrictions.On(() => bottlesMovementAlias.DeliveryPoint.Id).IsNull
						&& Restrictions.On(() => bottlesMovementAlias.Order.Id).IsNotNull))
				.Select(
					Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
						NHibernateUtil.Int32, new IProjection[]
						{
							Projections.Sum(() => bottlesMovementAlias.Returned),
							Projections.Sum(() => bottlesMovementAlias.Delivered)
						}));

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
						NHibernateUtil.Int32, new IProjection[]
						{
							Projections.Sum(() => bottlesMovementAlias.Returned),
							Projections.Sum(() => bottlesMovementAlias.Delivered)
						}));

			var taskExistQuery = QueryOver.Of(() => taskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPointAlias.Id)
				.And(() => taskAlias.IsTaskComplete == false)
				.Select(Projections.Property(() => taskAlias.Id))
				.Take(1);

			var countDeliveryPoint = QueryOver.Of(() => deliveryPointAlias)
				.Where(x => x.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Count(Projections.Id()));

			var counterpartyContactEmailsSubQuery = QueryOver.Of(() => emailAlias)
				.Where(() => emailAlias.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.Property(() => emailAlias.Id));

			var countDeliveryPointFixedPricesSubQuery = QueryOver.Of(() => nomenclatureFixedPriceAlias)
				.Where(() => nomenclatureFixedPriceAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select(Projections.Id());

			#region Phones Subqueries

			var deliveryPointPhonesSubquery = QueryOver.Of(() => phoneAlias)
				.Where(() => phoneAlias.DeliveryPoint.Id == orderAlias.DeliveryPoint.Id)
				.AndNot(() => phoneAlias.IsArchive)
				.Select(
					CustomProjections.GroupConcat(
						CustomProjections.Concat_WS(
							"",
							Projections.Constant("8"),
							Projections.Property(() => phoneAlias.DigitsNumber)),
						separator: ";\n"));

			var counterpartyPhonesSubquery = QueryOver.Of(() => phoneAlias)
				.Where(() => phoneAlias.Counterparty.Id == orderAlias.Client.Id)
				.AndNot(() => phoneAlias.IsArchive)
				.Select(
					CustomProjections.GroupConcat(
						CustomProjections.Concat_WS(
							"",
							Projections.Constant("8"),
							Projections.Property(() => phoneAlias.DigitsNumber)),
						separator: ";\n"));

			var phoneProjection = Projections.Conditional(
				Restrictions.IsNull(Projections.SubQuery(deliveryPointPhonesSubquery)),
				Projections.SubQuery(counterpartyPhonesSubquery),
				Projections.SubQuery(deliveryPointPhonesSubquery));

			#endregion

			var emailSubquery = QueryOver.Of(() => emailAlias)
				.Where(() => emailAlias.Counterparty.Id == orderAlias.Client.Id)
				.Select(CustomProjections.GroupConcat(() => emailAlias.Address, separator: ";\n"));

			#region LastOrder

			var lastOrderIdQuery = QueryOver.Of(() => lastOrderAlias)
				.Where(() => lastOrderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => (lastOrderAlias.SelfDelivery && orderAlias.DeliveryPoint == null) || (lastOrderAlias.DeliveryPoint.Id == deliveryPointAlias.Id))
				.And((x) => x.OrderStatus == OrderStatus.Closed)
				.Select(Projections.Property<Order>(p => p.Id))
				.OrderByAlias(() => orderAlias.Id).Desc
				.Take(1);

			var olderLastOrderIdQueryWithDate = GetOlderLastOrderIdWithDateQuery(
				lastOrderAlias, counterpartyAlias, orderAlias, deliveryPointAlias);

			var lastOrderIdQueryWithDate = GetLastOrderIdWithDateQuery(
				lastOrderAlias, counterpartyAlias, orderAlias, deliveryPointAlias, olderLastOrderIdQueryWithDate);

			var lastOrderNomenclatures = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => nomenclatureAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => _filterViewModel.LastOrderNomenclature.Id == nomenclatureAlias.Id);

			var lastOrderDiscount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.DiscountReason, () => discountReasonAlias, JoinType.LeftOuterJoin)
				.Select(Projections.Property(() => discountReasonAlias.Id))
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => _filterViewModel.DiscountReason.Id == discountReasonAlias.Id);

			var orderFromAnotherDP = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.Where(() => orderFromAnotherDPAlias.Client.Id == counterpartyAlias.Id)
				.And(() => orderFromAnotherDPAlias.OrderStatus == OrderStatus.Closed)
				.And(() => orderFromAnotherDPAlias.DeliveryDate >= orderAlias.DeliveryDate)
				.And(new Disjunction().Add(() => orderFromAnotherDPAlias.DeliveryPoint.Id != deliveryPointAlias.Id)
					.Add(() => orderFromAnotherDPAlias.SelfDelivery && !orderAlias.SelfDelivery)
					.Add(() => !orderFromAnotherDPAlias.SelfDelivery && orderAlias.SelfDelivery));

			var orderFromSuspended = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellation = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQueryWithDate)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);

			var orderFromSuspendedWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideSuspendedCounterpartyId).Take(1);

			var orderFromCancellationWithoutDate = QueryOver.Of(() => orderFromAnotherDPAlias)
				.Select(Projections.Property(() => orderFromAnotherDPAlias.Id))
				.WithSubquery.WhereProperty(x => x.Id).Eq(lastOrderIdQuery)
				.Where(x => x.ReturnTareReasonCategory.Id == hideCancellationCounterpartyId).Take(1);

			var subQuerryOrdersCount = QueryOver.Of(() => orderCountAlias)
				.Left.JoinAlias(() => orderCountAlias.OrderItems, () => orderItemsSubQueryAlias)
				.Left.JoinAlias(() => orderItemsSubQueryAlias.Nomenclature, () => nomenclatureSubQueryAlias)
				.Where(() => nomenclatureSubQueryAlias.Category == NomenclatureCategory.water)
				.Where(() => orderCountAlias.Client.Id == counterpartyAlias.Id)
				.Where(
					Restrictions.Not(Restrictions.In(Projections.Property<Order>(x => x.OrderStatus), _notDeliveredStatuses)))
				.Select(Projections.GroupProperty(
					Projections.Property<Order>(o => o.Client.Id)));

			#endregion LastOrder
			
			if(_filterViewModel != null && _filterViewModel.EndDate != null)
			{
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(lastOrderIdQueryWithDate.Take(1));
			}
			else
			{
				ordersQuery = ordersQuery.WithSubquery.WhereProperty(p => p.Id).Eq(lastOrderIdQuery);
			}
			
			#region Filter

			if(_filterViewModel != null)
			{
				if(_filterViewModel.Client != null)
				{
					ordersQuery = ordersQuery.Where((arg) => arg.Client.Id == _filterViewModel.Client.Id);
				}

				if(_filterViewModel.Address != null)
				{
					ordersQuery = ordersQuery.Where((arg) => arg.DeliveryPoint.Id == _filterViewModel.Address.Id);
				}
				
				if(_filterViewModel.SalesManager != null)
				{
					ordersQuery = ordersQuery
						.Where(() => counterpartyAlias.SalesManager.Id == _filterViewModel.SalesManager.Id);
				}

				if(_filterViewModel.OPF != null)
				{
					ordersQuery = ordersQuery.Where(() => counterpartyAlias.PersonType == _filterViewModel.OPF.Value);
				}

				if(_filterViewModel.LastOrderBottlesFrom != null)
				{
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered >= _filterViewModel.LastOrderBottlesFrom.Value);
				}

				if(_filterViewModel.LastOrderBottlesTo != null)
				{
					ordersQuery = ordersQuery.Where(() => bottleMovementOperationAlias.Delivered <= _filterViewModel.LastOrderBottlesTo.Value);
				}

				if(_filterViewModel.DeliveryPointsFrom != null)
				{
					ordersQuery = ordersQuery.Where(Restrictions.Ge(Projections.SubQuery(countDeliveryPoint), _filterViewModel.DeliveryPointsFrom.Value));
				}

				if(_filterViewModel.DeliveryPointsTo != null)
				{
					ordersQuery = ordersQuery.Where(Restrictions.Le(Projections.SubQuery(countDeliveryPoint), _filterViewModel.DeliveryPointsTo.Value));
				}

				if(_filterViewModel.StartDate != null)
				{
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate >= _filterViewModel.StartDate.Value);
				}

				if(_filterViewModel.EndDate != null)
				{
					ordersQuery = ordersQuery.Where(() => orderAlias.DeliveryDate <= _filterViewModel.EndDate.Value);
				}

				if(_filterViewModel.EndDate != null && _filterViewModel.HideActiveCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereNotExists(orderFromAnotherDP);
				}

				if(_filterViewModel.WithOneOrder != null)
				{
					var countProjection = Projections.CountDistinct(() => orderCountAlias.Id);

					subQuerryOrdersCount.Where(_filterViewModel.WithOneOrder.Value
						? Restrictions.Eq(countProjection, 1)
						: Restrictions.Not(Restrictions.Eq(countProjection, 1)));

					ordersQuery.WithSubquery
						.WhereProperty(() => counterpartyAlias.Id)
						.In(subQuerryOrdersCount);
				}

				if(_filterViewModel.DebtorsTaskStatus != null)
				{
					if(_filterViewModel.DebtorsTaskStatus.Value == DebtorsTaskStatus.HasTask)
					{
						ordersQuery = ordersQuery.WithSubquery.WhereExists(taskExistQuery);
					}
					else
					{
						ordersQuery = ordersQuery.WithSubquery.WhereNotExists(taskExistQuery);
					}
				}

				if(_filterViewModel.LastOrderNomenclature != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(lastOrderNomenclatures);
				}

				if(_filterViewModel.DiscountReason != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(lastOrderDiscount);
				}

				if(_filterViewModel.DebtBottlesFrom != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereValue(_filterViewModel.DebtBottlesFrom.Value).Le(bottleDebtByAddressQuery);
				}

				if(_filterViewModel.DebtBottlesTo != null)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereValue(_filterViewModel.DebtBottlesTo.Value).Ge(bottleDebtByAddressQuery);
				}

				if(!_filterViewModel.EndDate.HasValue && _filterViewModel.ShowSuspendedCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspendedWithoutDate);
				}

				if(!_filterViewModel.EndDate.HasValue && _filterViewModel.ShowCancellationCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellationWithoutDate);
				}

				if(_filterViewModel.EndDate.HasValue && _filterViewModel.ShowSuspendedCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromSuspended);
				}

				if(_filterViewModel.EndDate.HasValue && _filterViewModel.ShowCancellationCounterparty)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(orderFromCancellation);
				}

				if(_filterViewModel.HideWithoutEmail)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(counterpartyContactEmailsSubQuery);
				}

				if(_filterViewModel.HideWithoutFixedPrices)
				{
					ordersQuery = ordersQuery.WithSubquery.WhereExists(countDeliveryPointFixedPricesSubQuery);
				}

				if(_filterViewModel.SelectedDeliveryPointCategory != null)
				{
					ordersQuery.Where(() => deliveryPointAlias.Category.Id == _filterViewModel.SelectedDeliveryPointCategory.Id);
				}

				if(_filterViewModel.HideExcludeFromAutoCalls)
				{
					ordersQuery.Where(() => !counterpartyAlias.ExcludeFromAutoCalls);
				}

				#region FixPrice

				if(_filterViewModel.FixPriceFrom != null || _filterViewModel.FixPriceTo != null)
				{
					ordersQuery = ordersQuery.Where(() =>
						nomenclatureFixedPriceAlias.Id != null
						|| (_waterSemiozeriePrice >= (_filterViewModel.FixPriceFrom ?? _waterSemiozeriePrice)
						    && _waterSemiozeriePrice <= (_filterViewModel.FixPriceTo ?? _waterSemiozeriePrice)));
				}
				
				#endregion FixPrice
			}

			#endregion Filter

			ordersQuery.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => deliveryPointAlias.CompiledAddress,
				() => counterpartyAlias.Id,
				() => counterpartyAlias.Name));

			var resultQuery = ordersQuery
				.Left.JoinAlias(c => c.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(c => c.Client, () => counterpartyAlias)
				.Left.JoinAlias(c => c.BottlesMovementOperation, () => bottleMovementOperationAlias)
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
					.SelectSubQuery(taskExistQuery).WithAlias(() => resultAlias.TaskId)
					.SelectSubQuery(countDeliveryPoint).WithAlias(() => resultAlias.CountOfDeliveryPoint)
					.Select(phoneProjection).WithAlias(() => resultAlias.Phones)
					.SelectSubQuery(emailSubquery).WithAlias(() => resultAlias.Emails)
					.Select(fixedPriceProjection).WithAlias(() => resultAlias.FixPrice)
				)
				.SetTimeout(300)
				.TransformUsing(Transformers.AliasToBean<DebtorJournalNode>());

			return resultQuery;
		}

		public override void Dispose()
		{
			DataLoader.ItemsListUpdated -= UpdateFooterInfo;

			base.Dispose();
		}
	}
}
