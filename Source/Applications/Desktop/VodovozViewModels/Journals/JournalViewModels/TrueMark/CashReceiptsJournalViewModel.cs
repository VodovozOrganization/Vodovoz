using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentNHibernate.Automapping;
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
using System.Timers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.ViewModels.Reports.TrueMark;
using CashReceiptPermissions = Vodovoz.Permissions.Order.CashReceipt;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class CashReceiptsJournalViewModel : JournalViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly ReceiptManualController _receiptManualController;
		private readonly TrueMarkCodePoolLoader _codePoolLoader;
		private readonly IFileDialogService _fileDialogService;
		private readonly bool _canResendDuplicateReceipts;

		private CashReceiptJournalFilterViewModel _filter;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;

		public CashReceiptsJournalViewModel(
			CashReceiptJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ICashReceiptRepository cashReceiptRepository,
			ReceiptManualController receiptManualController,
			TrueMarkCodePoolLoader codePoolLoader,
			IFileDialogService fileDialogService,
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
			_codePoolLoader = codePoolLoader ?? throw new ArgumentNullException(nameof(codePoolLoader));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
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
			CreateLoadCodesToPoolAction();
			CreateProductCodesScanningReportAction();
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
					.Select(() => sourceCodeAlias.GTIN).WithAlias(() => resultAlias.SourceGtin)
					.Select(() => productCodeAlias.IsUnscannedSourceCode).WithAlias(() => resultAlias.IsUnscannedProductCode)
					.Select(() => productCodeAlias.IsDuplicateSourceCode).WithAlias(() => resultAlias.IsDuplicateProductCode)
					.Select(() => sourceCodeAlias.SerialNumber).WithAlias(() => resultAlias.SourceCodeSerialNumber)
					.Select(() => resultCodeAlias.GTIN).WithAlias(() => resultAlias.ResultGtin)
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

		#region Refresh fiscal document

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

		private JournalAction GetRefreshFiscalDocAction()
		{
			var manualSentAction = new JournalAction("Обновить статус фиск. документа",
				(selected) => RefreshFiscalDocActionSensitive(selected),
				(selected) => true,
				RefreshFiscalDoc
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

			try
			{
				_receiptManualController.RefreshFiscalDoc(node.Id);
			}
			catch(Exception ex)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, $"Невозможно подключиться к сервису обработки чеков. Повторите попытку позже.\n{ex.Message}");
			}
			Refresh();
		}

		#endregion Refresh fiscal document

		#region Load codes to pool

		private void CreateLoadCodesToPoolAction()
		{
			var loadCodesToPoolAction = new JournalAction("Загрузить коды в пул",
				(selected) => true,
				(selected) => true,
				(selected) => LoadCodesToPool()
			);
			NodeActionsList.Add(loadCodesToPoolAction);
		}

		private void LoadCodesToPool()
		{
			var interactiveService = _commonServices.InteractiveService;
			var dialogSettings = new DialogSettings();
			dialogSettings.SelectMultiple = false;
			dialogSettings.Title = "Выберите файл содержащий коды";
			dialogSettings.FileFilters.Add(new DialogFileFilter("Файлы содержащие коды", "*.xlsx", "*.mxl", "*.csv", "*.txt"));
			var result = _fileDialogService.RunOpenFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			try
			{
				var lodingResult = _codePoolLoader.LoadFromFile(result.Path);

				interactiveService.ShowMessage(ImportanceLevel.Info,
					$"Найдено кодов: {lodingResult.TotalFound}" +
					$"\nЗагружено: {lodingResult.SuccessfulLoaded}" +
					$"\nУже существуют в системе: {lodingResult.TotalFound - lodingResult.SuccessfulLoaded}");
			}
			catch(IOException ex)
			{
				interactiveService.ShowMessage(ImportanceLevel.Error, ex.Message);
			}
		}

		#endregion Load codes to pool

		#region Product Codes Scanning Repor Creation
		private void CreateProductCodesScanningReportAction()
		{
			var createProductCodesScanningReportAction = new JournalAction("Отчет о сканировании маркировки",
				(selected) => Filter.StartDate.HasValue && Filter.EndDate.HasValue && Filter.EndDate.Value >= Filter.StartDate.Value,
				(selected) => true,
				(selected) => GenerateProductCodesScanningReport()
			);
			NodeActionsList.Add(createProductCodesScanningReportAction);
		}

		private void GenerateProductCodesScanningReport()
		{
			if(!Filter.StartDate.HasValue || !Filter.EndDate.HasValue)
			{
				var interactiveService = _commonServices.InteractiveService;
				interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для создания отчета необходимо выбрать дату начала и дату окончания периода");
			}

			var dialogSettings = new DialogSettings();
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			var report = ProductCodesScanningReport.Generate(UoW, Filter.StartDate.Value, Filter.EndDate.Value);

			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Сканирование кодов маркировки");
				var sheetTitleRowNumber = 1;
				var tableTitlesRowNumber = 3;

				SetColumnsWidth(worksheet);

				var reportTitle = $"{report.Title} за период с {report.CreateDateFrom:dd.MM.yyyy} по {report.CreateDateTo:dd.MM.yyyy}";

				RenderWorksheetTitleCell(worksheet, sheetTitleRowNumber, 1, reportTitle);

				RenderTableTitleRow(worksheet, tableTitlesRowNumber);

				var excelRowCounter = ++tableTitlesRowNumber;

				for(int i = 0; i < report.Rows.Count; i++)
				{
					RenderReportRow(worksheet, excelRowCounter, report.Rows[i], i+1);
					excelRowCounter++;
				}

				workbook.SaveAs(result.Path);
			}
		}

		private void SetColumnsWidth(IXLWorksheet worksheet)
		{
			var firstColumnWidth = 5;
			var columnsWidth = 18;

			for(int i = 0; i < 13; i++)
			{
				var column = worksheet.Column(i + 1);

				column.Width = i == 0 ? firstColumnWidth : columnsWidth;
			}
		}

		private void RenderTableTitleRow(IXLWorksheet worksheet, int rowNumber)
		{
			var colNumber = 1;

			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "№");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Водитель");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Требуется кодов в заказах за период, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Успешно отсканировано кодов, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Успешно отсканировано кодов, %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Не отсканировано кодов, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Не отсканировано кодов, %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты одноразовые (из пула), шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты одноразовые (из пула), %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты множественные, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты множественные, %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Недействительные коды, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Недействительные коды, %");
		}

		private void RenderReportRow(IXLWorksheet worksheet, int rowNumber, ProductCodesScanningReport.Row values, int dataNumber)
		{
			var colNumber = 1;

			RenderNumericCell(worksheet, rowNumber, colNumber++, dataNumber);
			RenderStringCell(worksheet, rowNumber, colNumber++, values.DriverFIO);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.TotalCodesCount);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.SuccessfullyScannedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.SuccessfullyScannedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.UnscannedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.UnscannedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.SingleDuplicatedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.SingleDuplicatedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.MultiplyDuplicatedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.MultiplyDuplicatedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.InvalidCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.InvalidCodesPercent);
		}

		private void RenderWorksheetTitleCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			string value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number, isBold: true, isWrapText: false, fontSize: 13);
		}

		private void RenderTableTitleCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			string value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number, isBold: true);
		}

		private void RenderNumericCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			int value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number);
		}

		private void RenderNumericFloatingPointCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			decimal value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number, numericFormat: "##0.00");
		}

		private void RenderStringCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			string value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Text);
		}

		private void RenderCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			object value,
			XLDataType dataType,
			bool isBold = false,
			bool isWrapText = true,
			double fontSize = 11,
			string numericFormat = "")
		{
			var cell = worksheet.Cell(rowNumber, columnNumber);

			cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cell.Style.Font.Bold = isBold;
			cell.Style.Font.FontSize = fontSize;
			cell.Style.Alignment.WrapText = isWrapText;

			cell.DataType = dataType;

			if(dataType == XLDataType.Number)
			{
				if(!string.IsNullOrWhiteSpace(numericFormat))
				{
					cell.Style.NumberFormat.Format = numericFormat;
				}
			}

			cell.Value = value;
		}
		#endregion
	}
}
