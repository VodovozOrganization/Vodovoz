using ClosedXML.Excel;
using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Linq;
using System.Timers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using CashReceiptPermissions = Vodovoz.Core.Domain.Permissions.OrderPermissions.CashReceipt;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class OrdersWithReceiptsJournalViewModel : JournalViewModelBase
	{
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly IFileDialogService _fileDialogService;
		private CashReceiptJournalFilterViewModel _filter;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;

		public OrdersWithReceiptsJournalViewModel(
			CashReceiptJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ICashReceiptRepository cashReceiptRepository,
			IFileDialogService fileDialogService,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			Filter = filter ?? throw new ArgumentNullException(nameof(filter));

			var permissionService = commonServices.CurrentPermissionService;
			var allReceiptStatusesAvailable =
				permissionService.ValidatePresetPermission(CashReceiptPermissions.AllReceiptStatusesAvailable);
			var showOnlyCodeErrorStatusReceipts =
				permissionService.ValidatePresetPermission(CashReceiptPermissions.ShowOnlyCodeErrorStatusReceipts);
			var showOnlyReceiptSendErrorStatusReceipts =
				permissionService.ValidatePresetPermission(CashReceiptPermissions.ShowOnlyReceiptSendErrorStatusReceipts);
			
			var canReadReceipts = allReceiptStatusesAvailable || showOnlyCodeErrorStatusReceipts || showOnlyReceiptSendErrorStatusReceipts;
			if(!canReadReceipts)
			{
				AbortOpening("Нет прав просматривать кассовые чеки.");
				return;
			}

			_autoRefreshInterval = 30;

			Title = "Журнал заказов с чеками";

			var loader = new ThreadDataLoader<CashReceiptNode>(unitOfWorkFactory);
			loader.AddQuery(GetQuery);
			loader.MergeInOrderBy(x => x.OrderId, true);
			DataLoader = loader;

			CreateNodeActions();
			StartAutoRefresh();
		}

		public CashReceiptJournalFilterViewModel Filter
		{
			get => _filter;
			protected set
			{
				if(_filter != null)
				{
					_filter.OnFiltered -= FilterViewModel_OnFiltered;
				}

				_filter = value;
				if(_filter != null)
				{
					_filter.OnFiltered += FilterViewModel_OnFiltered;
				}
			}
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override string FooterInfo
		{
			get
			{
				var poolCount = _trueMarkCodesPool.GetTotalCount();
				var defectivePoolCount = _trueMarkCodesPool.GetDefectiveTotalCount();
				var autorefreshInfo = GetAutoRefreshInfo();
				var codeErrorsReceiptCount = _cashReceiptRepository.GetCodeErrorsReceiptCount(UoW);
				return $"Чеков с ошибками кодов: {codeErrorsReceiptCount} | Кодов в пуле: {poolCount}, бракованных: {defectivePoolCount} | {autorefreshInfo} | {base.FooterInfo}";
			}
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateAutorefreshAction();
			CreateExportAction();
		}

		#region Queries

		private IQueryOver<VodovozOrder> GetQuery(IUnitOfWork uow, bool isItemsCountFunction)
		{
			CashReceiptNode resultAlias = null;
			VodovozOrder orderAlias = null;
			CashReceipt cashReceiptAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Employee driverAlias = null;
			OrderItem orderItemAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias);
			query.JoinEntityQueryOver(() => cashReceiptAlias,
				Restrictions.Where(() => cashReceiptAlias.Order.Id == orderAlias.Id),
				JoinType.LeftOuterJoin);

			if(!isItemsCountFunction)
			{
				query.JoinEntityQueryOver(() => routeListItemAlias,
					Restrictions.Where(() => routeListItemAlias.Order.Id == orderAlias.Id),
					JoinType.LeftOuterJoin);
				query.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias);
				query.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias);
				query.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias);
			}

			if(_filter.Status.HasValue)
			{
				var disjunction = Restrictions.Disjunction();
				disjunction.Add(Restrictions.Eq(Projections.Property(() => cashReceiptAlias.Status), _filter.Status.Value));

				if(_filter.Status == CashReceiptStatus.ReceiptNotNeeded)
				{
					disjunction.Add(Restrictions.IsNull(Projections.Property(() => cashReceiptAlias.Id)));
				}

				query.Where(disjunction);
			}

			if(_filter.StartDate.HasValue)
			{
				var startDate = _filter.StartDate.Value;
				query.Where(() => orderAlias.DeliveryDate >= startDate);
			}

			if(_filter.EndDate.HasValue)
			{
				var endDate = _filter.EndDate.Value;
				query.Where(() => orderAlias.DeliveryDate <= endDate);
			}

			if(_filter.HasUnscannedReason)
			{
				query.Where(Restrictions.Eq(Projections.SqlFunction("IS_NULL_OR_WHITESPACE", NHibernateUtil.Boolean, Projections.Property(() => cashReceiptAlias.UnscannedCodesReason)), false));
			}

			if(_filter.AvailableReceiptStatuses == AvailableReceiptStatuses.CodeErrorAndReceiptSendError
				&& !_filter.Status.HasValue)
			{
				query.WhereRestrictionOn(() => cashReceiptAlias.Status)
					.IsInG(new[] { CashReceiptStatus.CodeError, CashReceiptStatus.ReceiptSendError });
			}

			query.Where(
				GetSearchCriterion(
					() => cashReceiptAlias.Id,
					() => cashReceiptAlias.Order.Id,
					() => cashReceiptAlias.UnscannedCodesReason
				)
			);

			var orderSumProjection = Projections.Sum(
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(IFNULL(?1, ?2) * ?3 - ?4, 2)"),
					NHibernateUtil.Decimal,
					Projections.Property(() => orderItemAlias.ActualCount),
					Projections.Property(() => orderItemAlias.Count),
					Projections.Property(() => orderItemAlias.Price),
					Projections.Property(() => orderItemAlias.DiscountMoney)
				)
			);

			if(isItemsCountFunction)
			{
				return query;
			}

			query.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Property(() => orderAlias.DeliveryDate)).WithAlias(() => resultAlias.DeliveryDate)
					.Select(Projections.Property(() => cashReceiptAlias.Id)).WithAlias(() => resultAlias.ReceiptId)
					.Select(Projections.Property(() => cashReceiptAlias.CreateDate)).WithAlias(() => resultAlias.ReceiptTime)
					.Select(orderSumProjection).WithAlias(() => resultAlias.OrderSum)
					.Select(Projections.Property(() => orderAlias.PaymentType)).WithAlias(() => resultAlias.OrderPaymentType)
					.Select(Projections.Property(() => orderAlias.SelfDelivery)).WithAlias(() => resultAlias.IsSelfdelivery)
					.Select(Projections.Property(() => routeListAlias.Id)).WithAlias(() => resultAlias.RouteListId)
					.Select(Projections.Property(() => driverAlias.Name)).WithAlias(() => resultAlias.DriverName)
					.Select(Projections.Property(() => driverAlias.LastName)).WithAlias(() => resultAlias.DriverLastName)
					.Select(Projections.Property(() => driverAlias.Patronymic)).WithAlias(() => resultAlias.DriverPatronimyc)
					.Select(Projections.Property(() => cashReceiptAlias.Status)).WithAlias(() => resultAlias.ReceiptStatus)
					.Select(Projections.Property(() => cashReceiptAlias.UnscannedCodesReason)).WithAlias(() => resultAlias.UnscannedReason)
					.Select(Projections.Property(() => cashReceiptAlias.ErrorDescription)).WithAlias(() => resultAlias.ErrorDescription)
				);
			query.OrderByAlias(() => orderAlias.Id).Desc();
			query.TransformUsing(Transformers.AliasToBean<CashReceiptNode>());

			return query;
		}

		#endregion Queries

		#region Autorefresh

		private bool autoRefreshEnabled => _autoRefreshTimer != null && _autoRefreshTimer.Enabled;

		private void StartAutoRefresh()
		{
			if(autoRefreshEnabled)
			{
				return;
			}
			_autoRefreshTimer = new Timer(_autoRefreshInterval * 1000);
			_autoRefreshTimer.Elapsed += (s, e) => Refresh();
			_autoRefreshTimer.Start();
		}

		private void StopAutoRefresh()
		{
			_autoRefreshTimer?.Stop();
			_autoRefreshTimer = null;
		}

		private void SwitchAutoRefresh()
		{
			if(autoRefreshEnabled)
			{
				StopAutoRefresh();
			}
			else
			{
				StartAutoRefresh();
			}
			OnPropertyChanged(nameof(FooterInfo));
		}

		private string GetAutoRefreshInfo()
		{
			if(autoRefreshEnabled)
			{
				return $"Автообновление каждые {_autoRefreshInterval} сек.";
			}
			else
			{
				return $"Автообновление выключено";
			}
		}

		private void CreateAutorefreshAction()
		{
			var switchAutorefreshAction = new JournalAction("Вкл/Выкл автообновление",
				(selected) => true,
				(selected) => true,
				(selected) => SwitchAutoRefresh()
			);
			NodeActionsList.Add(switchAutorefreshAction);
		}

		#endregion Autorefresh

		#region Export

		private void CreateExportAction()
		{
			var exportAction = new JournalAction("Выгрузить в Excel",
				(selected) => true,
				(selected) => true,
				(selected) => RunExportToExcel()
			);
			NodeActionsList.Add(exportAction);
		}

		private void RunExportToExcel()
		{
			StopAutoRefresh();
			try
			{
				ExportToExcel();
			}
			finally
			{
				StartAutoRefresh();
			}
		}

		private void ExportToExcel()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			using(var workbook = new XLWorkbook())
			{
				var colNumber = 1;
				var worksheet = workbook.Worksheets.Add("Реестр заказов для чека");
				worksheet.Cell(1, colNumber++).Value = "Код заказа";
				worksheet.Cell(1, colNumber++).Value = "Дата доставки";
				worksheet.Cell(1, colNumber++).Value = "Сумма";
				worksheet.Cell(1, colNumber++).Value = "Тип оплаты";
				worksheet.Cell(1, colNumber++).Value = "Самовывоз";
				worksheet.Cell(1, colNumber++).Value = "Код чека";
				worksheet.Cell(1, colNumber++).Value = "Время чека";
				worksheet.Cell(1, colNumber++).Value = "Статус";
				worksheet.Cell(1, colNumber++).Value = "МЛ";
				worksheet.Cell(1, colNumber++).Value = "Водитель";
				worksheet.Cell(1, colNumber++).Value = "Причина не отсканированных бутылей";
				worksheet.Cell(1, colNumber++).Value = "Описание ошибки";

				var excelRowCounter = 2;
				foreach(var call in Items.Cast<CashReceiptNode>())
				{
					colNumber = 1;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.OrderId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.DeliveryDate;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.OrderSum;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.PaymentType;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.IsSelfdelivery ? "Да" : "Нет";
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ReceiptId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ReceiptTime;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.Status;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.RouteListId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.DriverFIO;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.UnscannedReason;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ErrorDescription;
					excelRowCounter++;
				}
				workbook.SaveAs(result.Path);
			}
		}

		#endregion Export
	}
}
