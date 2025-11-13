using ClosedXML.Excel;
using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Organizations;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.ViewModels.Reports.TrueMark;
using CashReceiptPermissions = Vodovoz.Core.Domain.Permissions.OrderPermissions.CashReceipt;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class CashReceiptsJournalViewModel : JournalViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly ReceiptManualController _receiptManualController;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly bool _canResendDuplicateReceipts;

		private CashReceiptJournalFilterViewModel _filter;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;
		private bool _isReportGeneratingInProcess;

		public CashReceiptsJournalViewModel(
			CashReceiptJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ICashReceiptRepository cashReceiptRepository,
			ReceiptManualController receiptManualController,
			IFileDialogService fileDialogService,
			IOrganizationSettings organizationSettings,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			if(filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_receiptManualController = receiptManualController ?? throw new ArgumentNullException(nameof(receiptManualController));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			var permissionService = _commonServices.CurrentPermissionService;

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

			_canResendDuplicateReceipts = permissionService.ValidatePresetPermission(CashReceiptPermissions.CanResendDuplicateReceipts);

			if(!filter.StartDate.HasValue)
			{
				filter.StartDate = DateTime.Now.Date.AddMonths(-1);
			}

			if(!filter.EndDate.HasValue)
			{
				filter.EndDate = DateTime.Now.Date;
			}

			Filter = filter;
			_autoRefreshInterval = 30;

			Title = "Журнал чеков";

			var levelQueryLoader = new HierarchicalQueryLoader<CashReceipt, CashReceiptJournalNode>(unitOfWorkFactory, GetCount);

			levelQueryLoader.SetLevelingModel(GetQuery)
				.AddNextLevelSource(GetDetails);

			RecuresiveConfig = levelQueryLoader.TreeConfig;

			var threadDataLoader = new ThreadDataLoader<CashReceiptJournalNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.QueryLoaders.Add(levelQueryLoader);
			DataLoader = threadDataLoader;

			CreateNodeActions();
			CreatePopupActions();
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

		public bool IsReportGeneratingInProcess
		{
			get => _isReportGeneratingInProcess;
			private set
			{
				SetField(ref _isReportGeneratingInProcess, value);
				UpdateJournalActions();
			}
		}

		public bool CanCreateProductCodesScanningReportReport =>
			Filter.StartDate.HasValue
			&& Filter.EndDate.HasValue
			&& Filter.EndDate.Value >= Filter.StartDate.Value
			&& !IsReportGeneratingInProcess;

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
				return $"Заказов с ошибками кодов: {codeErrorsReceiptCount} | Кодов в пуле: {poolCount}, бракованных: {defectivePoolCount} | {autorefreshInfo} | {base.FooterInfo}";
			}
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateHelpAction();
			CreateAutorefreshAction();
			CreateNodeManualSendAction();
			CreateNodeRefreshFiscalDocAction();
			CreateNodeRequeueFiscalDocAction();
			CreateProductCodesScanningReportAction();
			CreateExportAction();
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();
			CreatePopupManualSendAction();
			CreatePopupRefreshFiscalDocAction();
		}

		#region Queries

		private IQueryOver<CashReceipt> GetQuery(IUnitOfWork uow)
		{
			CashReceiptJournalNode resultAlias = null;
			CashReceipt cashReceiptAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Employee driverAlias = null;

			var query = uow.Session.QueryOver(() => cashReceiptAlias);
			query.JoinEntityQueryOver(() => routeListItemAlias, Restrictions.Where(() => cashReceiptAlias.Order.Id == routeListItemAlias.Order.Id), JoinType.LeftOuterJoin);
			query.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias);
			query.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias);
			query.Where(() => !cashReceiptAlias.WithoutMarks);

			if(_filter.Status.HasValue)
			{
				query.Where(Restrictions.Eq(Projections.Property(() => cashReceiptAlias.Status), _filter.Status.Value));
			}

			if(_filter.StartDate.HasValue)
			{
				var startDate = _filter.StartDate.Value;
				query.Where(
					Restrictions.Ge(
						Projections.SqlFunction("DATE",
							NHibernateUtil.Date,
							Projections.Property(() => cashReceiptAlias.CreateDate)),
						startDate
					)
				);
			}

			if(_filter.EndDate.HasValue)
			{
				var endDate = _filter.EndDate.Value;
				query.Where(
					Restrictions.Le(
						Projections.SqlFunction("DATE",
							NHibernateUtil.Date,
							Projections.Property(() => cashReceiptAlias.CreateDate)),
						endDate
					)
				);
			}

			if(_filter.HasUnscannedReason)
			{
				query.Where(Restrictions.Eq(Projections.SqlFunction("IS_NULL_OR_WHITESPACE", NHibernateUtil.Boolean, Projections.Property(() => cashReceiptAlias.UnscannedCodesReason)), false));
			}

			if(_filter.AvailableReceiptStatuses == AvailableReceiptStatuses.CodeErrorAndReceiptSendError
				&& !_filter.Status.HasValue)
			{
				query.WhereRestrictionOn(x => x.Status)
					.IsInG(new[] { CashReceiptStatus.CodeError, CashReceiptStatus.ReceiptSendError });
			}

			query.Where(
				GetSearchCriterion(
					() => cashReceiptAlias.Id,
					() => cashReceiptAlias.Order.Id,
					() => cashReceiptAlias.UnscannedCodesReason
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => cashReceiptAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(Projections.Constant(CashReceiptNodeType.Receipt)).WithAlias(() => resultAlias.NodeType)
				.Select(Projections.Property(() => cashReceiptAlias.CreateDate)).WithAlias(() => resultAlias.Created)
				.Select(Projections.Property(() => cashReceiptAlias.UpdateDate)).WithAlias(() => resultAlias.Changed)
				.Select(Projections.Property(() => cashReceiptAlias.Status)).WithAlias(() => resultAlias.ReceiptStatus)
				.Select(Projections.Property(() => cashReceiptAlias.Sum)).WithAlias(() => resultAlias.ReceiptSum)
				.Select(Projections.Property(() => routeListAlias.Id)).WithAlias(() => resultAlias.RouteListId)
				.Select(Projections.Property(() => driverAlias.Name)).WithAlias(() => resultAlias.DriverName)
				.Select(Projections.Property(() => driverAlias.LastName)).WithAlias(() => resultAlias.DriverLastName)
				.Select(Projections.Property(() => driverAlias.Patronymic)).WithAlias(() => resultAlias.DriverPatronimyc)
				.Select(Projections.Property(() => cashReceiptAlias.Order.Id)).WithAlias(() => resultAlias.OrderAndItemId)
				.Select(Projections.Property(() => cashReceiptAlias.InnerNumber)).WithAlias(() => resultAlias.ReceiptInnerNumber)
				.Select(Projections.Property(() => cashReceiptAlias.FiscalDocumentStatus)).WithAlias(() => resultAlias.FiscalDocStatus)
				.Select(Projections.Property(() => cashReceiptAlias.FiscalDocumentNumber)).WithAlias(() => resultAlias.FiscalDocNumber)
				.Select(Projections.Property(() => cashReceiptAlias.FiscalDocumentDate)).WithAlias(() => resultAlias.FiscalDocDate)
				.Select(Projections.Property(() => cashReceiptAlias.FiscalDocumentStatusChangeTime)).WithAlias(() => resultAlias.FiscalDocStatusDate)
				.Select(Projections.Property(() => cashReceiptAlias.ManualSent)).WithAlias(() => resultAlias.IsManualSentOrIsDefectiveCode)
				.Select(Projections.Property(() => cashReceiptAlias.Contact)).WithAlias(() => resultAlias.Contact)
				.Select(Projections.Property(() => cashReceiptAlias.UnscannedCodesReason)).WithAlias(() => resultAlias.UnscannedReason)
				.Select(Projections.Property(() => cashReceiptAlias.ErrorDescription)).WithAlias(() => resultAlias.ErrorDescription)
			)
			.OrderByAlias(() => cashReceiptAlias.Id).Desc()
			.TransformUsing(Transformers.AliasToBean<CashReceiptJournalNode>());

			return query;
		}

		private IList<CashReceiptJournalNode> GetDetails(IEnumerable<CashReceiptJournalNode> parentNodes)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				CashReceiptProductCode productCodeAlias = null;
				TrueMarkWaterIdentificationCode sourceCodeAlias = null;
				TrueMarkWaterIdentificationCode resultCodeAlias = null;
				CashReceiptJournalNode resultAlias = null;

				var query = uow.Session.QueryOver(() => productCodeAlias)
					.Left.JoinAlias(() => productCodeAlias.SourceCode, () => sourceCodeAlias)
					.Left.JoinAlias(() => productCodeAlias.ResultCode, () => resultCodeAlias)
					.Where(Restrictions.In(Projections.Property(() => productCodeAlias.CashReceipt.Id), parentNodes.Select(x => x.Id).ToArray()));

				query.SelectList(list => list
					.SelectGroup(() => productCodeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Constant(CashReceiptNodeType.Code)).WithAlias(() => resultAlias.NodeType)
					.Select(() => productCodeAlias.CashReceipt.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => productCodeAlias.OrderItem.Id).WithAlias(() => resultAlias.OrderAndItemId)
					.Select(() => sourceCodeAlias.Gtin).WithAlias(() => resultAlias.SourceGtin)
					.Select(() => productCodeAlias.IsUnscannedSourceCode).WithAlias(() => resultAlias.IsUnscannedProductCode)
					.Select(() => productCodeAlias.IsDuplicateSourceCode).WithAlias(() => resultAlias.IsDuplicateProductCode)
					.Select(() => sourceCodeAlias.SerialNumber).WithAlias(() => resultAlias.SourceCodeSerialNumber)
					.Select(() => resultCodeAlias.Gtin).WithAlias(() => resultAlias.ResultGtin)
					.Select(() => resultCodeAlias.SerialNumber).WithAlias(() => resultAlias.ResultSerialnumber)
					.Select(() => productCodeAlias.IsDefectiveSourceCode).WithAlias(() => resultAlias.IsManualSentOrIsDefectiveCode)
				)
				.TransformUsing(Transformers.AliasToBean<CashReceiptJournalNode>());

				return query.List<CashReceiptJournalNode>();
			}
		}

		#endregion Queries

		private int GetCount(IUnitOfWork uow)
		{
			var query = GetQuery(uow);
			var count = query.List<CashReceiptJournalNode>().Count();
			return count;
		}

		#region Help

		private void CreateHelpAction()
		{
			var helpAction = new JournalAction("Справка",
				(selected) => true,
				(selected) => true,
				(selected) => ShowHelp()
			);
			NodeActionsList.Add(helpAction);
		}

		private void ShowHelp()
		{
			var helpMessage = @"Журнал чеков отображает информацию о состоянии кассовых чеков для заказов.
Чек - запись о данных необходимых для формирования, отправки и проверки реального чека.
	Код чека / код марк. - внутренний номер записи о чеке или записи о маркировки
	Id чека - идентификатор чека, по которому его можно найти в модуль кассе
	Создан - дата создания записи о чеке, является датой начала работы с чеком.
	Изменен - дата последнего изменения какой либо информации о чеке.
Маркировка - запись связанная с чеком, представляет информацию о коде честного знака привязанной к единице товара. 
	Имеет исходную маркировку которая была отсканирована водителем с бутыля и итоговую маркировку которая уже отправляется в чек.
	Если исходная маркировка не удовлетворяет условиям для использования ее в чеке, в итоговой она заменяется на ранее специально сохраненную в пуле для замены.
Фискальный документ (Фиск. док.) -  по сути является действительным чеком, формируется после фискализации отправленной информации о чеке в модуль кассу.
	Номер фиск. док. - номер фискального документа по которому можно найти чек в ОФД
	Дата статуса фискального документа - дата последней смены статуса фискального документа.
	Дата фискального документа - дата фискализации чека

Чек можно переотправить вручную только если он является дублем по сумме.
Можно обновить информацию о фискальном документе, информация по этому чеку будет загружена с модуль кассы.

Логика подбора контакта для отправки чека:
	Подбирается первый подходящий контакт из приоритетов:
	1. Телефон для чеков точки доставки
	2. Телефон для чеков контрагента
	3. Эл.почта для чеков контрагентов
	4. Мобильный телефон точки доставки
	5. Мобильный телефон контрагента
	6. Эл.почта для счетов контрагента
	7. Иная эл. почта (не для чеков или счетов)
	8. Городской телефон точки доставки
	9. Городской телефон контрагента";


			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, helpMessage, "Справка");
		}

		#endregion Help

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

		#endregion Autorefresh

		#region Manual send

		private void CreatePopupManualSendAction()
		{
			var manualSentAction = GetManualSentAction();
			PopupActionsList.Add(manualSentAction);
		}

		private void CreateNodeManualSendAction()
		{
			var manualSentAction = GetManualSentAction();
			NodeActionsList.Add(manualSentAction);
		}

		private JournalAction GetManualSentAction()
		{
			var manualSentAction = new JournalAction("Отправить чек принудительно",
				ManualSentActionSensitive,
				(selected) => true,
				ManualSent
			);
			return manualSentAction;
		}

		private bool ManualSentActionSensitive(object[] selectedNodes)
		{
			if(!_canResendDuplicateReceipts)
			{
				return false;
			}

			var nodes = selectedNodes.OfType<CashReceiptJournalNode>();
			if(!nodes.Any())
			{
				return false;
			}

			if(nodes.Count() > 1)
			{
				return false;
			}

			var node = nodes.First();

			if(node.NodeType != CashReceiptNodeType.Receipt)
			{
				return false;
			}

			if(node.ReceiptStatus == CashReceiptStatus.DuplicateSum || node.ReceiptStatus == CashReceiptStatus.ReceiptNotNeeded)
			{
				return true;
			}

			return false;
		}

		private void ManualSent(object[] selectedNodes)
		{
			var node = selectedNodes.OfType<CashReceiptJournalNode>().Single();
			_receiptManualController.ForceSendDuplicatedReceipt(node.Id);
			Refresh();
		}

		#endregion Manual send

		#region Refresh and requeue fiscal document

		private void CreatePopupRefreshFiscalDocAction()
		{
			var refreshFiscalDocAction = GetRefreshFiscalDocAction();
			PopupActionsList.Add(refreshFiscalDocAction);
		}

		private void CreateNodeRefreshFiscalDocAction()
		{
			var refreshFiscalDocAction = GetRefreshFiscalDocAction();
			NodeActionsList.Add(refreshFiscalDocAction);
		}

		private void CreateNodeRequeueFiscalDocAction()
		{
			var requeueFiscalDocAction = GetRequeueFiscalDocAction();
			NodeActionsList.Add(requeueFiscalDocAction);
		}

		private JournalAction GetRefreshFiscalDocAction()
		{
			var manualSentAction = new JournalAction("Обновить статус фиск. документа",
				(selected) => RefreshFiscalDocActionSensitive(selected),
				(selected) => true,
				RefreshFiscalDoc
			);
			return manualSentAction;
		}

		private JournalAction GetRequeueFiscalDocAction()
		{
			var manualSentAction = new JournalAction("Повторное проведение чека",
				(selected) => RequeueFiscalDocActionSensitive(selected),
				(selected) => true,
				RequeueFiscalDoc
			);
			return manualSentAction;
		}

		private bool RefreshFiscalDocActionSensitive(object[] selectedNodes)
		{
			var nodes = selectedNodes.OfType<CashReceiptJournalNode>();
			if(!nodes.Any())
			{
				return false;
			}

			if(nodes.Count() > 1)
			{
				return false;
			}

			var node = nodes.First();

			if(node.NodeType != CashReceiptNodeType.Receipt)
			{
				return false;
			}

			if(node.ReceiptStatus == CashReceiptStatus.Sended)
			{
				return true;
			}

			return false;
		}

		private void RefreshFiscalDoc(object[] selectedNodes)
		{
			var node = selectedNodes.OfType<CashReceiptJournalNode>().Single();

			var result = _receiptManualController.RefreshFiscalDoc(node.Id);

			ShowResultToUser(result);

			Refresh();
		}

		private bool RequeueFiscalDocActionSensitive(object[] selectedNodes)
		{
			var nodes = selectedNodes.OfType<CashReceiptJournalNode>();
			if(!nodes.Any())
			{
				return false;
			}

			if(nodes.Count() > 1)
			{
				return false;
			}

			var node = nodes.First();

			if(node.NodeType != CashReceiptNodeType.Receipt)
			{
				return false;
			}

			if(node.FiscalDocStatus == FiscalDocumentStatus.Failed)
			{
				return true;
			}

			return false;
		}

		private void RequeueFiscalDoc(object[] selectedNodes)
		{
			var node = selectedNodes.OfType<CashReceiptJournalNode>().Single();

			var result = _receiptManualController.RequeueFiscalDoc(node.Id);

			ShowResultToUser(result);

			Refresh();
		}

		private void ShowResultToUser(Result result)
		{
			if(result.IsSuccess)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Запрос выполнен успешно");
			}
			else
			{
				var firstError = result.Errors.FirstOrDefault();

				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					firstError.Message ?? "При выполнении запроса произошла непредвиденная ошибка");
			}
		}

		#endregion Refresh and requeue fiscal document

		#region Product Codes Scanning Repor Creation
		private void CreateProductCodesScanningReportAction()
		{
			var createProductCodesScanningReportAction = new JournalAction("Отчет о сканировании маркировки",
				(selected) => CanCreateProductCodesScanningReportReport,
				(selected) => true,
				async (selected) => await GenerateProductCodesScanningReportAsync()
			);
			NodeActionsList.Add(createProductCodesScanningReportAction);
		}

		private async Task GenerateProductCodesScanningReportAsync()
		{
			if(!Filter.StartDate.HasValue || !Filter.EndDate.HasValue)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для создания отчета необходимо выбрать дату начала и дату окончания периода");

				return;
			}

			if(IsReportGeneratingInProcess)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Отчет уже в процессе формирования");

				return;
			}

			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.FileName = typeof(ProductCodesScanningReport).GetClassUserFriendlyName().Nominative
				+ $" с {Filter.StartDate:dd.MM.yyyy} по {Filter.EndDate.Value.Date:dd.MM.yyyy}";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			IsReportGeneratingInProcess = true;

			ProductCodesScanningReport report;

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot())
			{
				report = await ProductCodesScanningReport.GenerateAsync(
					unitOfWork, _organizationSettings, Filter.StartDate.Value, Filter.EndDate.Value);
			}

			await report?.ExportReportToExcelAsync(result.Path);

			IsReportGeneratingInProcess = false;
		}
		#endregion

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
			dialogSettings.FileName = $"{Title} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			using(var workbook = new XLWorkbook())
			{
				var colNumber = 1;
				var worksheet = workbook.Worksheets.Add("Журнал чеков");
				worksheet.Cell(1, colNumber++).Value = "Код чека";
				worksheet.Cell(1, colNumber++).Value = "Id чека";
				worksheet.Cell(1, colNumber++).Value = "Создан";
				worksheet.Cell(1, colNumber++).Value = "Изменён";
				worksheet.Cell(1, colNumber++).Value = "Статус";
				worksheet.Cell(1, colNumber++).Value = "Сумма";
				worksheet.Cell(1, colNumber++).Value = "Код МЛ";
				worksheet.Cell(1, colNumber++).Value = "Водитель";
				worksheet.Cell(1, colNumber++).Value = "Код заказа";
				worksheet.Cell(1, colNumber++).Value = "Статус фикс.док.";
				worksheet.Cell(1, colNumber++).Value = "Номер фикс.док.";
				worksheet.Cell(1, colNumber++).Value = "Дата фикс.док.";
				worksheet.Cell(1, colNumber++).Value = "Дата статуса фикс.док.";
				worksheet.Cell(1, colNumber++).Value = "Ручная отправка";
				worksheet.Cell(1, colNumber++).Value = "Отправлен на";
				worksheet.Cell(1, colNumber++).Value = "Причина не отскан.бутылей";
				worksheet.Cell(1, colNumber++).Value = "Описание ошибки";

				var excelRowCounter = 2;

				var nodes = Items.Cast<CashReceiptJournalNode>();

				worksheet.Column(6).Style.NumberFormat.SetFormat("# ##0.00");

				foreach(var call in nodes)
				{
					colNumber = 1;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ReceiptOrProductCodeId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ReceiptDocId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.CreatedTime;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ChangedTime;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.Status;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ReceiptSum;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.RouteList;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.DriverFIO;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.OrderAndItemId;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.FiscalDocStatusOrSourceGtin;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.FiscalDocNumberOrSourceCodeInfo;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.FiscalDocDateOrResultGtin;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.FiscalDocStatusDateOrResultSerialnumber;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.IsManualSentOrIsDefectiveCode ? "Да" : null;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.Contact;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.UnscannedReason;
					worksheet.Cell(excelRowCounter, colNumber++).Value = call.ErrorDescription;
					excelRowCounter++;
				}

				workbook.SaveAs(result.Path);
			}
		}

		#endregion
	}
}
