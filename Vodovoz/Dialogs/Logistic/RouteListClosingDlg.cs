﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Parameters;
using QS.Validation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Employees;
using QS.Project.Services;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.WageCalculation;
using QS.Project.Journal.EntitySelector;
using QS.Tools;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Tools;
using Vodovoz.JournalViewModels;
using Vodovoz.Services;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalFilters;

namespace Vodovoz
{
	public partial class RouteListClosingDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
	{
		#region поля

		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(_parametersProvider);
		
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IDeliveryShiftRepository _deliveryShiftRepository = new DeliveryShiftRepository();
		private readonly ICashRepository _cashRepository = new CashRepository();
		private readonly ICategoryRepository _categoryRepository = new CategoryRepository(_parametersProvider);
		private readonly IAccountableDebtsRepository _accountableDebtsRepository = new AccountableDebtsRepository();
		private readonly ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(_parametersProvider);
		private readonly ITrackRepository _trackRepository = new TrackRepository();
		private readonly IFuelRepository _fuelRepository = new FuelRepository();
		private readonly IRouteListRepository _routeListRepository = new RouteListRepository(new StockRepository(), _baseParametersProvider);
		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(_parametersProvider));

		private Track track = null;
		private decimal balanceBeforeOp = default(decimal);
		private readonly bool _editing = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("role_сashier");
		private bool canCloseRoutelist = false;
		private Employee previousForwarder = null;
		
		WageParameterService wageParameterService = new WageParameterService(new WageCalculationRepository(), _baseParametersProvider);
		private EmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository = new EmployeeNomenclatureMovementRepository();
		private bool _needToSelectTerminalCondition = false;
		private bool _hasAccessToDriverTerminal = false;

		List<ReturnsNode> allReturnsToWarehouse;
		private IEnumerable<DefectSource> defectiveReasons;
		int bottlesReturnedToWarehouse;
		int bottlesReturnedTotal;
		int defectiveBottlesReturnedToWarehouse;

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						new OrderRepository(),
						_employeeRepository,
						_baseParametersProvider,
						ServicesConfig.CommonServices.UserService,
						SingletonErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		enum RouteListActions
		{
			[Display(Name = "Новый штраф")]
			CreateNewFine,
			[Display(Name = "Перенести разгрузку в другой МЛ")]
			TransferReceptionToAnotherRL,
			[Display(Name = "Перенести разгрузку в этот МЛ")]
			TransferReceptionToThisRL,
			[Display(Name = "Перенести адреса в этот МЛ")]
			TransferAddressesToThisRL,
			[Display(Name = "Перенести адреса из этого МЛ")]
			TransferAddressesToAnotherRL

		}

		public enum RouteListPrintDocuments
		{
			[Display(Name = "Все")]
			All,
			[Display(Name = "Маршрутный лист")]
			RouteList,
			[Display(Name = "Штрафы")]
			Fines
		}

		#endregion

		#region Конструкторы и конфигурирование диалога

		public RouteListClosingDlg(RouteList routeList) : this(routeList.Id) { }

		public RouteListClosingDlg(int routeListId)
		{
			this.Build();

			PerformanceHelper.StartMeasurement();

			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(routeListId);

			TabName = string.Format("Закрытие маршрутного листа №{0}", Entity.Id);
			PerformanceHelper.AddTimePoint("Создан UoW");
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			if(Entity.AddressesOrderWasChangedAfterPrinted) {
				MessageDialogHelper.RunInfoDialog("<span color=\"red\">ВНИМАНИЕ!</span> Порядок адресов в Мл был изменен!");
			}

			permissioncommentview.UoW = UoW;
			permissioncommentview.Title = "Комментарий по проверке закрытия МЛ: ";
			permissioncommentview.Comment = Entity.CashierReviewComment;
			permissioncommentview.PermissionName = "can_edit_cashier_review_comment";
			permissioncommentview.Comment = Entity.CashierReviewComment;
			permissioncommentview.CommentChanged += (comment) => { 
				Entity.CashierReviewComment = comment;
				HasChanges = true;
			};


			canCloseRoutelist = new PermissionRepository()
				.HasAccessToClosingRoutelist(UoW, _subdivisionRepository, _employeeRepository, ServicesConfig.UserService);
			Entity.ObservableFuelDocuments.ElementAdded += ObservableFuelDocuments_ElementAdded;
			Entity.ObservableFuelDocuments.ElementRemoved += ObservableFuelDocuments_ElementRemoved;

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);

			var filterDriver = new EmployeeRepresentationFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();

			previousForwarder = Entity.Forwarder;
			var filterForwarder = new EmployeeRepresentationFilterViewModel();
			filterForwarder.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.forwarder,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceForwarder.RepresentationModel = new EmployeesVM(filterForwarder);
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.Changed += ReferenceForwarder_Changed;

			var filterLogistican = new EmployeeRepresentationFilterViewModel();
			filterLogistican.SetAndRefilterAtOnce(x => x.Status = EmployeeStatus.IsWorking);
			referenceLogistican.RepresentationModel = new EmployeesVM(filterLogistican);
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistician, widget => widget.Subject).InitializeFromSource();

			speccomboShift.ItemsList = _deliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();

			ycheckConfirmDifferences.Binding.AddBinding(Entity, e => e.DifferencesConfirmed, w => w.Active).InitializeFromSource();

			decimal unclosedAdvanceMoney = _accountableDebtsRepository.EmployeeDebt(UoW, Entity.Driver);
			ylabelUnclosedAdvancesMoney.LabelProp =
				string.Format(unclosedAdvanceMoney > 0m ? "<span foreground='red'><b>Долг: {0}</b></span>" : "", unclosedAdvanceMoney);

			ytextClosingComment.Binding.AddBinding(Entity, e => e.ClosingComment, w => w.Buffer.Text).InitializeFromSource();
			labelOrderEarly.Text = "Сдано ранее:" + GetCashOrder().ToString();
			spinCashOrder.Value = 0;
			advanceSpinbutton.Value = 0;
			advanceSpinbutton.Visible = false;

			PerformanceHelper.AddTimePoint("Создан диалог");

			PerformanceHelper.AddTimePoint("Предварительная загрузка");

			routeListAddressesView.UoW = UoW;
			routeListAddressesView.RouteList = Entity;
			foreach(RouteListItem item in routeListAddressesView.Items) 
			{
				item.Order.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged;
				item.Order.ObservableOrderItems.ElementAdded += ObservableOrderItems_ElementAdded;
				item.Order.ObservableOrderItems.ElementRemoved += ObservableOrderItems_ElementRemoved;

				item.Order.ObservableOrderEquipments.ElementChanged += ObservableOrderItems_ElementChanged;
				item.Order.ObservableOrderEquipments.ElementAdded += ObservableOrderItems_ElementAdded;
				item.Order.ObservableOrderEquipments.ElementRemoved += ObservableOrderItems_ElementRemoved;

				item.Order.ObservableOrderDepositItems.ElementChanged += ObservableOrderItems_ElementChanged;
				item.Order.ObservableOrderDepositItems.ElementAdded += ObservableOrderItems_ElementAdded;
				item.Order.ObservableOrderDepositItems.ElementRemoved += ObservableOrderItems_ElementRemoved;
			}
			routeListAddressesView.Items.ElementChanged += OnRouteListItemChanged;
			routeListAddressesView.OnClosingItemActivated += OnRouteListItemActivated;
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
			PerformanceHelper.AddTimePoint("заполнили список адресов");
			ReloadReturnedToWarehouse();
			var returnableOrderItems = routeListAddressesView.Items.Where(address => address.IsDelivered())
																   .SelectMany(address => address.Order.OrderItems)
																   .Where(orderItem => !orderItem.Nomenclature.IsSerial)
																   .Where(orderItem => Nomenclature.GetCategoriesForShipment().Any(nom => nom == orderItem.Nomenclature.Category));
			foreach(var item in returnableOrderItems) {
				if(allReturnsToWarehouse.All(r => r.NomenclatureId != item.Nomenclature.Id))
					allReturnsToWarehouse.Add(new EntityRepositories.Logistic.ReturnsNode {
						Name = item.Nomenclature.Name,
						Trackable = item.Nomenclature.IsSerial,
						NomenclatureId = item.Nomenclature.Id,
						Nomenclature = item.Nomenclature,
						Amount = 0
					});
			}

			routelistdiscrepancyview.RouteList = Entity;
			routelistdiscrepancyview.ItemsLoaded = Entity.NotLoadedNomenclatures(false,
				_baseParametersProvider.GetNomenclatureIdForTerminal);
			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			routelistdiscrepancyview.FineChanged += Routelistdiscrepancyview_FineChanged;

			PerformanceHelper.AddTimePoint("Получили возврат на склад");
			//FIXME Убрать из этого места первоначальное заполнение. Сейчас оно вызывается при переводе статуса на сдачу. После того как не нормально не переведенных в закрытие маршрутников, тут заполение можно убрать.
			if(!Entity.ClosingFilled)
				Entity.FirstFillClosing(wageParameterService);

			PerformanceHelper.AddTimePoint("Закончено первоначальное заполнение");

			rightsidepanel1.Panel = vboxHidenPanel;
			rightsidepanel1.IsHided = true;

			xpndRouteListInfo.Expanded = Entity.Status == RouteListStatus.Closed;

			PerformanceHelper.AddTimePoint("Заполнили расхождения");

			ytreeviewFuelDocuments.ItemsDataSource = Entity.ObservableFuelDocuments;
			ytreeviewFuelDocuments.Reorderable = true;
			Entity.ObservableFuelDocuments.ListChanged += ObservableFuelDocuments_ListChanged;
			UpdateFuelDocumentsColumns();

			enummenuRLActions.ItemsEnum = typeof(RouteListActions);
			enummenuRLActions.EnumItemClicked += EnummenuRLActions_EnumItemClicked;

			CheckWage();

			LoadDataFromFine();
			OnItemsUpdated();
			PerformanceHelper.AddTimePoint("Загрузка штрафов");
			GetFuelInfo();
			UpdateFuelInfo();
			PerformanceHelper.AddTimePoint("Загрузка бензина");

			PerformanceHelper.Main.PrintAllPoints(logger);

			//Подписки на обновления
			OrmMain.GetObjectDescription<CarUnloadDocument>().ObjectUpdatedGeneric += OnCalUnloadUpdated;

			enumPrint.ItemsEnum = typeof(RouteListPrintDocuments);
			enumPrint.EnumItemClicked += (sender, e) => PrintSelectedDocument((RouteListPrintDocuments)e.ItemEnum);

			Entity.PropertyChanged += Entity_PropertyChanged;
			
			foreach (RouteListItem routeListItem in routeListAddressesView.Items)
				routeListItem.RecalculateTotalCash();
			CalculateTotal();

			UpdateSensitivity();

			notebook1.ShowTabs = false;
			notebook1.Page = 0;

			_hasAccessToDriverTerminal = _editing;
			var baseDoc = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, Entity.Driver);
			_needToSelectTerminalCondition = baseDoc is DriverAttachedTerminalGiveoutDocument && baseDoc.CreationDate.Date <= Entity?.Date;
			hboxTerminalCondition.Visible = _hasAccessToDriverTerminal && _needToSelectTerminalCondition;

			enumTerminalCondition.ItemsEnum = typeof(DriverTerminalCondition);
			enumTerminalCondition.Binding
				.AddBinding(Entity, e => e.DriverTerminalCondition, w => w.SelectedItemOrNull).InitializeFromSource();
		}

		private void UpdateSensitivity()
		{
			var sensStatuses = new[] { RouteListStatus.OnClosing, RouteListStatus.MileageCheck, RouteListStatus.Delivered};
			if(!sensStatuses.Contains(Entity.Status))
			{
				ytextviewFuelInfo.Sensitive = false;
				ycheckHideCells.Sensitive = false;
				vbxFuelTickets.Sensitive = false;
				speccomboShift.Sensitive = false;
				referenceLogistican.Sensitive = false;
				referenceDriver.Sensitive = false;
				referenceForwarder.Sensitive = false;
				entityviewmodelentryCar.Sensitive = false;
				datePickerDate.Sensitive = false;
				hbox11.Sensitive = false;
				routelistdiscrepancyview.Sensitive = false;
				hbxStatistics1.Sensitive = false;
				hbxStatistics2.Sensitive = false;
				enummenuRLActions.Sensitive = false;
				toggleWageDetails.Sensitive = _editing;

				HasChanges = false;

				return;
			}

			speccomboShift.Sensitive = false;
			vbxFuelTickets.Sensitive = CheckIfCashier();
			entityviewmodelentryCar.Sensitive = _editing;
			referenceDriver.Sensitive = _editing;
			referenceForwarder.Sensitive = _editing;
			referenceLogistican.Sensitive = _editing;
			datePickerDate.Sensitive = _editing;
			ycheckConfirmDifferences.Sensitive = _editing &&
				(Entity.Status == RouteListStatus.OnClosing || 
				 Entity.Status == RouteListStatus.Delivered);
			ytextClosingComment.Sensitive = _editing;
			routeListAddressesView.IsEditing = _editing;
			ycheckHideCells.Sensitive = _editing;
			routelistdiscrepancyview.Sensitive = _editing;
			buttonAddFuelDocument.Sensitive = Entity.Car?.FuelType?.Cost != null && Entity.Driver != null && _editing;
			buttonDeleteFuelDocument.Sensitive = Entity.Car?.FuelType?.Cost != null && Entity.Driver != null && _editing;
			enummenuRLActions.Sensitive = _editing;
			advanceCheckbox.Sensitive = advanceSpinbutton.Sensitive = _editing;
			spinCashOrder.Sensitive = buttonCreateCashOrder.Sensitive = _editing;
			buttonCalculateCash.Sensitive = _editing;
			labelWage1.Visible = _editing;
			toggleWageDetails.Sensitive = _editing;
			UpdateButtonState();
		}

		/// <summary>
		/// Перепроверка зарплаты водителя и экспедитора
		/// </summary>
		private void CheckWage()
		{
			decimal driverCurrentWage = Entity.GetDriversTotalWage();
			decimal forwarderCurrentWage = Entity.GetForwardersTotalWage();
			decimal driverRecalcWage = Entity.GetRecalculatedDriverWage(wageParameterService);
			decimal forwarderRecalcWage = Entity.GetRecalculatedForwarderWage(wageParameterService);

			string recalcWageMessage = "Найдены расхождения после пересчета зарплаты:";
			bool hasDiscrepancy = false;
			if(driverRecalcWage != driverCurrentWage) {
				recalcWageMessage += string.Format("\nВодителя: до {0}, после {1}", driverCurrentWage, driverRecalcWage);
				hasDiscrepancy = true;
			}
			if(forwarderRecalcWage != forwarderCurrentWage) {
				recalcWageMessage += string.Format("\nЭкспедитора: до {0}, после {1}", forwarderCurrentWage, forwarderRecalcWage);
				hasDiscrepancy = true;
			}
			recalcWageMessage += string.Format("\nПересчитано.");

			if(hasDiscrepancy && Entity.Status == RouteListStatus.Closed) {
				MessageDialogHelper.RunInfoDialog(recalcWageMessage);
				Entity.RecalculateAllWages(wageParameterService);
			}
		}

		void ObservableFuelDocuments_ListChanged(object aList)
		{
			UpdateFuelDocumentsColumns();
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.NormalWage))
				Entity.RecalculateAllWages(wageParameterService);

			if(e.PropertyName == nameof(Entity.Car)) {
				Entity.RecalculateAllWages(wageParameterService);
			}
		}

		private void UpdateFuelDocumentsColumns()
		{
			var config = ColumnsConfigFactory.Create<FuelDocument>();

			config.AddColumn("Дата")
					.AddTextRenderer(node => node.Date.ToShortDateString())
				  .AddColumn("Литры")
					.AddNumericRenderer(node => node.FuelOperation.LitersGived)
					.Adjustment(new Adjustment(0, -100000, 100000, 10, 100, 10))
				  .AddColumn("№ ТК")
					.AddTextRenderer(n => n.FuelCardNumber)
					.Editable(Entity.Car.CanEditFuelCardNumber)
				  .AddColumn("")
					.AddTextRenderer()
				  .RowCells();

			ytreeviewFuelDocuments.ColumnsConfig = config.Finish();
		}

		private decimal GetCashOrder() => _cashRepository.CurrentRouteListCash(UoW, Entity.Id);

		private decimal GetTerminalOrdersSum() {
			var result = Entity.Addresses.Where(x => x.Order.PaymentType == PaymentType.Terminal &&
																		x.Status != RouteListItemStatus.Transfered)
												 .Sum(x => x.Order.ActualTotalSum);

			return result;
		}

		void OnCalUnloadUpdated(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<CarUnloadDocument> e)
		{
			if(e.UpdatedSubjects.Any(x => x.RouteList.Id == Entity.Id))
				ReloadDiscrepancies();
		}

		#endregion

		#region Методы

		void ReferenceForwarder_Changed(object sender, EventArgs e)
		{
			var newForwarder = Entity.Forwarder;

			if((previousForwarder == null && newForwarder != null)
			 || (previousForwarder != null && newForwarder == null))
				Entity.RecalculateAllWages(wageParameterService);

			previousForwarder = Entity.Forwarder;
		}

		void EnummenuRLActions_EnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			switch((RouteListActions)e.ItemEnum) {
				case RouteListActions.CreateNewFine:
					this.TabParent.AddSlaveTab(
						this, new FineDlg(Entity)
					);
					break;
				case RouteListActions.TransferReceptionToAnotherRL:
					this.TabParent.AddSlaveTab(
						this, new TransferGoodsBetweenRLDlg(Entity, 
							TransferGoodsBetweenRLDlg.OpenParameter.Sender,
							employeeNomenclatureMovementRepository)
					);
					break;
				case RouteListActions.TransferReceptionToThisRL:
					this.TabParent.AddSlaveTab(
						this, new TransferGoodsBetweenRLDlg(Entity, 
							TransferGoodsBetweenRLDlg.OpenParameter.Receiver,
							employeeNomenclatureMovementRepository)
					);
					break;
				case RouteListActions.TransferAddressesToThisRL:
					if(UoW.HasChanges) {
						if(MessageDialogHelper.RunQuestionDialog("Необходимо сохранить документ.\nСохранить?"))
							this.Save();
						else
							return;
					}
					this.TabParent.AddSlaveTab(
						this, 
						new RouteListAddressesTransferringDlg(
							Entity.Id, 
							RouteListAddressesTransferringDlg.OpenParameter.Receiver,
							employeeNomenclatureMovementRepository,
							_baseParametersProvider,
							_routeListRepository,
							new EmployeeService(),
							ServicesConfig.CommonServices,
							_categoryRepository
						)
					);
					break;
				case RouteListActions.TransferAddressesToAnotherRL:
					if(UoW.HasChanges) {
						if(MessageDialogHelper.RunQuestionDialog("Необходимо сохранить документ.\nСохранить?"))
							this.Save();
						else
							return;
					}
					this.TabParent.AddSlaveTab(
						this, 
						new RouteListAddressesTransferringDlg(
							Entity.Id, 
							RouteListAddressesTransferringDlg.OpenParameter.Sender,
							employeeNomenclatureMovementRepository,
							_baseParametersProvider,
							_routeListRepository,
							new EmployeeService(),
							ServicesConfig.CommonServices,
							_categoryRepository
						)
					);
					break;
				default:
					break;
			}
		}


		void Routelistdiscrepancyview_FineChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		void OnRouteListItemActivated(object sender, RowActivatedArgs args)
		{
			var node = routeListAddressesView.GetSelectedRouteListItem();
			var dlg = new OrderReturnsView(node, UoW);
			TabParent.AddSlaveTab(this, dlg);
		}

		void OnRouteListItemChanged(object aList, int[] aIdx)
		{
			var item = routeListAddressesView.Items[aIdx[0]];

			Entity.RecalculateWagesForRouteListItem(item, wageParameterService);
			item.RecalculateTotalCash();
			if(!item.IsDelivered() && item.Status != RouteListItemStatus.Transfered)
				foreach(var itm in item.Order.OrderItems)
					itm.ActualCount = 0m;

			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			OnItemsUpdated();
		}

		void ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			OrderReturnsChanged();
		}

		void ObservableOrderItems_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			OrderReturnsChanged();
		}

		void ObservableOrderItems_ElementChanged(object aList, int[] aIdx)
		{
			OrderReturnsChanged();
		}

		void OrderReturnsChanged()
		{
			foreach(var item in routeListAddressesView.Items) {
				var rli = item as RouteListItem;
				Entity.RecalculateWagesForRouteListItem(rli, wageParameterService);
				rli.RecalculateTotalCash();
			}
			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			OnItemsUpdated();
		}

		void OnItemsUpdated()
		{
			CalculateTotal();
			UpdateButtonState();
		}

		void UpdateButtonState()
		{
			buttonAccept.Sensitive = 
				(Entity.Status == RouteListStatus.OnClosing || Entity.Status == RouteListStatus.MileageCheck
				                                            || Entity.Status == RouteListStatus.Delivered) 
				&& canCloseRoutelist;
		}

		private bool buttonFineEditState;

		/// <summary>
		/// Не использовать это поле напрямую, используйте свойство DefaultBottle
		/// </summary>
		Nomenclature defaultBottle;
		Nomenclature DefaultBottle {
			get {
				if(defaultBottle == null) {
					var db = _nomenclatureRepository.GetDefaultBottleNomenclature(UoW);
					defaultBottle = db ?? throw new Exception("Не найдена номенклатура бутыли по умолчанию, указанная в параметрах приложения: default_bottle_nomenclature");
				}
				return defaultBottle;
			}
		}

		void CalculateTotal()
		{
			var items = routeListAddressesView.Items.Where(item => item.IsDelivered());
			bottlesReturnedTotal = items.Sum(item => item.BottlesReturned + item.Order.BottlesByStockActualCount);
			int fullBottlesTotal = (int)items.SelectMany(item => item.Order.OrderItems)
										.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && item.Nomenclature.TareVolume == TareVolume.Vol19L)
										.Sum(item => item.ActualCount ?? 0);
			decimal depositsCollectedTotal = items.Sum(item => item.BottleDepositsCollected);
			decimal equipmentDepositsCollectedTotal = items.Sum(item => item.EquipmentDepositsCollected);
			decimal totalCollected = items.Sum(item => item.TotalCash);
			Entity.CalculateWages(wageParameterService);
			decimal driverWage = Entity.GetDriversTotalWage();
			decimal forwarderWage = Entity.GetForwardersTotalWage();

			labelAddressCount.Text = string.Format("Адр.: {0}", Entity.UniqueAddressCount);
			labelPhone.Text = string.Format(
				"Сот. связь: {0} {1}",
				Entity.PhoneSum,
				CurrencyWorks.CurrencyShortName
			);
			labelFullBottles.Text = string.Format("Полных бут.: {0}", fullBottlesTotal);
			labelEmptyBottles.Text = string.Format("Пустых бут.: {0}", bottlesReturnedTotal);
			labelDeposits.Text = string.Format(
				"Залогов: {0} {1}",
				depositsCollectedTotal + equipmentDepositsCollectedTotal,
				CurrencyWorks.CurrencyShortName
			);
			labelCash.Text = string.Format(
				"Сдано по накладным: {0} {1}",
				totalCollected,
				CurrencyWorks.CurrencyShortName
			);
			labelTotalCollected.Text = string.Format(
				"Итоговая сумма(нал.): {0} {1}",
				totalCollected - Entity.PhoneSum,
				CurrencyWorks.CurrencyShortName
			);
			labelTerminalSum.Text = $"По терминалу: {GetTerminalOrdersSum()} {CurrencyWorks.CurrencyShortName}";
			labelTotal.Markup = string.Format(
				"Итого сдано: <b>{0:F2}</b> {1}",
				GetCashOrder() - (decimal)advanceSpinbutton.Value,
				CurrencyWorks.CurrencyShortName
			);
			labelWage1.Markup = string.Format(
				"ЗП вод.: <b>{0}</b> {2}" + "  " + "ЗП эксп.: <b>{1}</b> {2}",
				driverWage,
				forwarderWage,
				CurrencyWorks.CurrencyShortName
			);
			labelEmptyBottlesFommula.Markup = string.Format("Тара: <b>{0}</b><sub>(выгружено на склад)</sub> - <b>{1}</b><sub>(по документам)</sub> =",
				bottlesReturnedToWarehouse,
				bottlesReturnedTotal
			);

			if(defectiveBottlesReturnedToWarehouse > 0) {
				lblQtyOfDefectiveGoods.Visible = true;
				lblQtyOfDefectiveGoods.Markup = string.Format(
					"Единиц брака: <b>{0}</b> шт.",
						defectiveBottlesReturnedToWarehouse);
				var namesOfDefectiveReasons = defectiveReasons.Select(x => x.GetEnumTitle());
				DefectSourceLabel.Text = " Причины: " + String.Join(", ", namesOfDefectiveReasons);
			} else {
				lblQtyOfDefectiveGoods.Visible = false;
				DefectSourceLabel.Visible = false;
			}

			var bottleDifference = bottlesReturnedToWarehouse - bottlesReturnedTotal;
			var differenceAttributes = bottlesReturnedToWarehouse - bottlesReturnedTotal > 0 ? "background=\"#ff5555\"" : "";
			var bottleDifferenceFormat = "<span {1}><b>{0}</b><sub>(осталось)</sub></span>";
			checkUseBottleFine.Visible = bottleDifference < 0;
			if(bottleDifference != 0) {
				checkUseBottleFine.Label = string.Format("({0:C})", DefaultBottle.SumOfDamage * (-bottleDifference));
			}
			labelBottleDifference.Markup = string.Format(bottleDifferenceFormat, bottleDifference, differenceAttributes);

			//Штрафы
			decimal totalSumOfDamage = 0;
			if(checkUseBottleFine.Active)
				totalSumOfDamage += DefaultBottle.SumOfDamage * (-bottleDifference);
			totalSumOfDamage += routelistdiscrepancyview.Items.Where(x => x.UseFine).Sum(x => x.SumOfDamage);

			StringBuilder fineText = new StringBuilder();
			if(totalSumOfDamage != 0) {
				fineText.AppendLine(string.Format("Выб. ущерб: {0:C}", totalSumOfDamage));
			}
			if(Entity.BottleFine != null) {
				fineText.AppendLine(string.Format("Штраф: {0:C}", Entity.BottleFine.TotalMoney));
			}
			labelBottleFine.LabelProp = fineText.ToString().TrimEnd('\n');
			buttonBottleAddEditFine.Sensitive = totalSumOfDamage != 0;
			buttonBottleDelFine.Sensitive = Entity.BottleFine != null;
			if(buttonFineEditState != (Entity.BottleFine != null)) {
				(buttonBottleAddEditFine.Image as Image).Pixbuf = new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(),
					Entity.BottleFine != null ? "Vodovoz.icons.buttons.edit.png" : "Vodovoz.icons.buttons.add.png"
				);
				buttonFineEditState = Entity.BottleFine != null;
			}
		}

		protected bool IsConsistentWithUnloadDocument()
		{
			var hasItemsDiscrepancies = routelistdiscrepancyview.Items.Any(discrepancy => discrepancy.Remainder != 0);
			bool hasFine = Entity.BottleFine != null;
			var items = Entity.Addresses.Where(item => item.IsDelivered());
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned + item.Order.BottlesByStockActualCount);
			var hasTotalBottlesDiscrepancy = bottlesReturnedToWarehouse != bottlesReturnedTotal;
			return hasFine || (!hasTotalBottlesDiscrepancy && !hasItemsDiscrepancies) || Entity.DifferencesConfirmed;
		}

		public override bool Save() {
			
			var valid = new QSValidator<RouteList>(Entity,
				new Dictionary<object, object>
				{
					{nameof(IRouteListItemRepository), new RouteListItemRepository()},
					{nameof(DriverTerminalCondition), _needToSelectTerminalCondition && Entity.Status == RouteListStatus.Closed}
				});
			
			permissioncommentview.Save();

			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return false;

			if(!ValidateOrders()) {
				return false;
			}

			if(!TrySetCashier()) {
				return false;
			}

			if(Entity.Status == RouteListStatus.Delivered) {
				Entity.ChangeStatusAndCreateTask(Entity.ActiveCarVersion.IsCompanyCar && Entity.Car?.CarModel?.TypeOfUse != CarTypeOfUse.Truck ? RouteListStatus.MileageCheck : RouteListStatus.OnClosing, CallTaskWorker);
			}
			
			UoW.Save();

			return true;
		}

		private bool ValidateOrders()
		{
			bool isOrdersValid = true;
			string orderIds = "";
			byte ordersCounter = 0;
			OrderParametersProvider orderParametersProvider = new OrderParametersProvider(new ParametersProvider());
			ValidationContext validationContext;
			foreach(var item in Entity.Addresses) {
				validationContext = new ValidationContext(item.Order);
				validationContext.ServiceContainer.AddService(typeof(IOrderParametersProvider), orderParametersProvider);
				if(!ServicesConfig.ValidationService.Validate(item.Order, validationContext))
				{
					if(string.IsNullOrWhiteSpace(orderIds)) {
						orderIds = string.Format("{0}", item.Order.Id);
					} else {
						orderIds = string.Format("{0}{2} {1}", orderIds, item.Order.Id, ordersCounter == 4 ? "\n" : ",");
					}
					isOrdersValid = false;
					if(ordersCounter == 4) {
						ordersCounter = 0;
						continue;
					}
					ordersCounter++;
				}
			}

			if(!isOrdersValid) {
				MessageDialogHelper.RunErrorDialog(string.Format("Следующие заказы заполнены некорректно:\n {0}", orderIds));
				return false;
			}
			return true;
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			PerformanceHelper.StartMeasurement();
			
			if(!TrySetCashier()) {
				return;
			}

			var validationContext = new Dictionary<object, object> {
				{"NewStatus", RouteListStatus.MileageCheck},
				{"cash_order_close", true},
				{nameof(IRouteListItemRepository), new RouteListItemRepository()},
				{nameof(DriverTerminalCondition), _needToSelectTerminalCondition}
			};
			var valid = new QSValidator<RouteList>(UoWGeneric.Root, validationContext);
			if(valid.RunDlgIfNotValid((Window)this.Toplevel)) {
				return;
			}

			PerformanceHelper.AddTimePoint("Валидация МЛ");
			
			if(advanceCheckbox.Active && advanceSpinbutton.Value > 0) {
				EmployeeAdvanceOrder((decimal)advanceSpinbutton.Value);
				
				PerformanceHelper.AddTimePoint("Создан расходный ордер");
			}

			var cash = _cashRepository.CurrentRouteListCash(UoW, Entity.Id);
			if(Entity.Total != cash) {
				MessageDialogHelper.RunWarningDialog($"Невозможно подтвердить МЛ, сумма МЛ ({CurrencyWorks.GetShortCurrencyString(Entity.Total)}) не соответствует кассе ({CurrencyWorks.GetShortCurrencyString(cash)}).");
				if(Entity.Status == RouteListStatus.OnClosing && Entity.ConfirmedDistance <= 0 && Entity.NeedMileageCheck && MessageDialogHelper.RunQuestionDialog("По МЛ не принят километраж, перевести в статус проверки километража?")) {
					Entity.ChangeStatusAndCreateTask(RouteListStatus.MileageCheck, CallTaskWorker);
					
					PerformanceHelper.AddTimePoint("Статус сменен на 'проверка километража' и создано задание");
				}
				return;
			}

			if(Entity.ActiveCarVersion.CarOwnershipType == CarOwnershipType.RaskatCar)
			{
				Entity.RecountMileage();
			}

			Entity.UpdateMovementOperations(_categoryRepository);

			PerformanceHelper.AddTimePoint("Обновлены операции перемещения");

			if(Entity.Status == RouteListStatus.OnClosing) {
				Entity.AcceptCash(CallTaskWorker);
				
				PerformanceHelper.AddTimePoint("Создано задание на обзвон");
			}

			if(Entity.Status == RouteListStatus.Delivered) {
				if(routelistdiscrepancyview.Items.Any(discrepancy => discrepancy.Remainder != 0)
				&& !Entity.DifferencesConfirmed) {
					Entity.ChangeStatusAndCreateTask(RouteListStatus.OnClosing, CallTaskWorker);
				} else {
					if(Entity.ActiveCarVersion.IsCompanyCar && Entity.Car.CarModel.TypeOfUse != CarTypeOfUse.Truck) {
						Entity.ChangeStatusAndCreateTask(RouteListStatus.MileageCheck, CallTaskWorker);
					} else {
						Entity.ChangeStatusAndCreateTask(RouteListStatus.Closed, CallTaskWorker);
					}
				}
			}
			
			Entity.WasAcceptedByCashier = true;

			SaveAndClose();
			
			PerformanceHelper.AddTimePoint("Сохранение и закрытие завершено");
			
			PerformanceHelper.Main.PrintAllPoints(logger);
		}

		void PrintSelectedDocument(RouteListPrintDocuments choise)
		{
			if(!MessageDialogHelper.RunQuestionDialog("Перед печатью необходимо сохранить документ.\nСохранить?"))
				return;
			UoW.Save();

			switch(choise) {
				case RouteListPrintDocuments.All:
					PrintRouteList();
					PrintFines();
					break;
				case RouteListPrintDocuments.RouteList:
					PrintRouteList();
					break;
				case RouteListPrintDocuments.Fines:
					PrintFines();
					break;
			}
		}

		void PrintRouteList()
		{
			{
				var document = Additions.Logistic.PrintRouteListHelper.GetRDLRouteList(UoW, Entity);
				var reportDlg = new QSReport.ReportViewDlg(document);
				reportDlg.ReportPrinted += SavePrintTime;
				this.TabParent.OpenTab(
					QS.Dialog.Gtk.TdiTabBase.GenerateHashName<QSReport.ReportViewDlg>(),
					() => reportDlg);
			}
		}

		void SavePrintTime(object sender, EventArgs e)
		{
			Entity.AddPrintHistory();
			UoW.Save();
		}

		void PrintFines()
		{
			{
				var document = Additions.Logistic.PrintRouteListHelper.GetRDLFine(Entity);
				this.TabParent.OpenTab(
					QS.Dialog.Gtk.TdiTabBase.GenerateHashName<QSReport.ReportViewDlg>(),
					() => new QSReport.ReportViewDlg(document));
			}
		}

		protected void OnButtonBottleAddEditFineClicked(object sender, EventArgs e)
		{
			string fineReason = "Недосдача";
			var bottleDifference = bottlesReturnedTotal - bottlesReturnedToWarehouse;
			var summ = DefaultBottle.SumOfDamage * (bottleDifference > 0 ? bottleDifference : (decimal)0);
			summ += routelistdiscrepancyview.Items.Where(x => x.UseFine).Sum(x => x.SumOfDamage);
			var nomenclatures = routelistdiscrepancyview.Items.Where(x => x.UseFine)
				.ToDictionary(x => x.Nomenclature, x => -x.Remainder);
			if(checkUseBottleFine.Active)
				nomenclatures.Add(DefaultBottle, bottleDifference);

			FineDlg fineDlg;
			if(Entity.BottleFine != null) {
				fineDlg = new FineDlg(Entity.BottleFine);

				Entity.BottleFine.UpdateNomenclature(nomenclatures);
				fineDlg.Entity.TotalMoney = summ;
				fineDlg.EntitySaved += FineDlgExist_EntitySaved;
			} else {
				fineDlg = new FineDlg(summ, Entity, fineReason, DateTime.Now, Entity.Driver);
				fineDlg.Entity.AddNomenclature(nomenclatures);
				fineDlg.EntitySaved += FineDlgNew_EntitySaved;
			}
			TabParent.AddSlaveTab(this, fineDlg);
		}

		void FineDlgNew_EntitySaved(object sender, QS.Tdi.EntitySavedEventArgs e)
		{
			Entity.BottleFine = e.Entity as Fine;
			CalculateTotal();
			UpdateButtonState();
		}

		void FineDlgExist_EntitySaved(object sender, QS.Tdi.EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(Entity.BottleFine);
			CalculateTotal();
		}

		protected void OnButtonBottleDelFineClicked(object sender, EventArgs e)
		{
			OrmMain.DeleteObject<Fine>(Entity.BottleFine.Id, UoW);
			Entity.BottleFine = null;
			CalculateTotal();
			UpdateButtonState();
		}

		protected void OnCheckUseBottleFineToggled(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		private void GetFuelInfo()
		{
			track = _trackRepository.GetTrackByRouteListId(UoW, Entity.Id);

			var fuelOtlayedOp = UoWGeneric.Root.FuelOutlayedOperation;
			var givedOp = Entity.FuelDocuments.Select(x => x.FuelOperation.Id);
			//Проверяем существование операций и исключаем их.
			var exclude = new List<int>();
			exclude.AddRange(givedOp);
			if(fuelOtlayedOp != null && fuelOtlayedOp.Id != 0) {
				exclude.Add(fuelOtlayedOp.Id);
			}
			if(exclude.Count == 0)
				exclude = null;

			if(Entity.Car.FuelType != null) {
				Employee driver = Entity.Driver;
				var car = Entity.Car;

				if(Entity.ActiveCarVersion.IsCompanyCar)
					driver = null;
				else
					car = null;

				balanceBeforeOp = _fuelRepository.GetFuelBalance(
					UoW, driver, car, Entity.ClosingDate ?? DateTime.Now, exclude?.ToArray());
			}
		}

		private void UpdateFuelInfo()
		{
			var text = new List<string>();
			decimal spentFuel = (decimal)Entity.Car.FuelConsumption / 100 * Entity.ConfirmedDistance;

			bool hasTrack = track?.Distance.HasValue ?? false;

			if(Entity.PlanedDistance != null && Entity.PlanedDistance != 0) {
				text.Add(string.Format("Планируемое расстояние: {0:F1} км", Entity.PlanedDistance));
			}
			if(hasTrack) {
				text.Add(string.Format("Расстояние по треку: {0:F1} км.", track.TotalDistance));
			}
			text.Add(string.Format("Расстояние подтвержденное логистами: {0:F1} км.", Entity.ConfirmedDistance));

			if(Entity.Car.FuelType != null) {
				text.Add(string.Format("Вид топлива: {0}", Entity.Car.FuelType.Name));
			} else {
				text.Add("Не указан вид топлива");
			}

			if(Entity.FuelDocuments.Select(x => x.FuelOperation).Any()) {
				text.Add(string.Format("Остаток без выдачи {0:F2} л.", balanceBeforeOp));
			}

			text.Add(string.Format("Израсходовано топлива: {0:F2} л. ({1:F2} л/100км)", spentFuel, (decimal)Entity.Car.FuelConsumption));

			if(Entity.FuelDocuments.Select(x => x.FuelOperation).Any()) {
				text.Add(string.Format("Выдано {0:F2} литров",
					 Entity.FuelDocuments.Select(x => x.FuelOperation.LitersGived).Sum()));
			}

			if(Entity.Car.FuelType != null) {
				text.Add(
					string.Format(
						"Текущий остаток топлива {0:F2} л.",
						balanceBeforeOp + Entity.FuelDocuments.Select(x => x.FuelOperation.LitersGived).Sum() - spentFuel
					)
				);
			}

			text.Add($"Номер топливной карты: {Entity.Car.FuelCardNumber}");

			ytextviewFuelInfo.Buffer.Text = string.Join("\n", text);
		}

		void LoadDataFromFine()
		{
			if(Entity.BottleFine == null)
				return;

			foreach(var nom in Entity.BottleFine.Nomenclatures) {
				if(nom.Nomenclature.Id == DefaultBottle.Id) {
					checkUseBottleFine.Active = true;
					continue;
				}

				var found = routelistdiscrepancyview.Items.FirstOrDefault(x => x.Nomenclature.Id == nom.Nomenclature.Id);
				if(found != null)
					found.UseFine = true;
			}
		}

		protected void OnYspinActualDistanceValueChanged(object sender, EventArgs e)
		{
			UpdateFuelInfo();
		}

		void ObservableFuelDocuments_ElementAdded(object aList, int[] aIdx)
		{
			UpdateFuelInfo();
			CalculateTotal();
		}

		void ObservableFuelDocuments_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			UpdateFuelInfo();
			CalculateTotal();
		}

		protected void OnYcheckConfirmDifferencesToggled(object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		protected void OnYcheckHideCellsToggled(object sender, EventArgs e)
		{
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
		}

		protected void OnButtonReturnedRefreshClicked(object sender, EventArgs e)
		{
			ReloadDiscrepancies();
		}

		private void ReloadDiscrepancies()
		{
			ReloadReturnedToWarehouse();
			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			CalculateTotal();
		}

		private void ReloadReturnedToWarehouse()
		{
			allReturnsToWarehouse = _routeListRepository.GetReturnsToWarehouse(UoW, Entity.Id, Nomenclature.GetCategoriesForShipment());
			var returnedBottlesNom = int.Parse(_parametersProvider.GetParameterValue("returned_bottle_nomenclature_id"));
			bottlesReturnedToWarehouse = (int)_routeListRepository.GetReturnsToWarehouse(
				UoW,
				Entity.Id,
				returnedBottlesNom)
			.Sum(item => item.Amount);

			var defectiveNomenclaturesIds = _nomenclatureRepository
				.GetNomenclatureOfDefectiveGoods(UoW)
				.Select(n => n.Id)
				.ToArray();

			var returnedDefectiveItems = _routeListRepository.GetReturnsToWarehouse(
				UoW,
				Entity.Id,
				defectiveNomenclaturesIds);

			defectiveReasons = returnedDefectiveItems
				.Select(x => x.DefectSource).Distinct();
			
			defectiveBottlesReturnedToWarehouse = (int)returnedDefectiveItems.Sum(item => item.Amount);
		}

		public override void Destroy()
		{
			OrmMain.GetObjectDescription<CarUnloadDocument>().ObjectUpdatedGeneric -= OnCalUnloadUpdated;
			base.Destroy();
		}

		protected void OnButtonCreateCashOrderClicked(object sender, EventArgs e)
		{
			var messages = new List<string>();

			if(!TrySetCashier()) {
				return;
			}

			Income cashIncome = null;
			Expense cashExpense = null;

			var inputCashOrder = (decimal)spinCashOrder.Value;
			try
			{
				messages.AddRange(Entity.ManualCashOperations(ref cashIncome, ref cashExpense, inputCashOrder, _categoryRepository));
			}
			catch(MissingOrdersWithCashlessPaymentTypeException ex)
			{
				MessageDialogHelper.RunErrorDialog(ex.Message);
			}

			if (cashIncome != null) UoW.Save(cashIncome);
			if (cashExpense != null) UoW.Save(cashExpense);

			UoW.Save();

			CalculateTotal();

			if(messages.Any())
				MessageDialogHelper.RunInfoDialog(string.Format("Были выполнены следующие действия:\n*{0}", string.Join("\n*", messages)));
		}

		private void EmployeeAdvanceOrder(decimal cashInput)
		{
			string message, ifAdvanceIsBigger;

			Expense cashExpense = null;
			decimal cashToReturn = Entity.MoneyToReturn - cashInput;

			ifAdvanceIsBigger = (cashToReturn > 0) ? "Сумма для сдачи в кассу" : "Сумма для выдачи из кассы";

			if(!TrySetCashier()) {
				return;
			}

			message = Entity.EmployeeAdvanceOperation(ref cashExpense, cashInput, _categoryRepository);

			if(cashExpense != null)
				UoW.Save(cashExpense);
			cashExpense.UpdateWagesOperations(UoW);
			UoW.Save();

			MessageDialogHelper.RunInfoDialog(string.Format("{0}\n\n{1}: {2:C0}", message, ifAdvanceIsBigger, Math.Abs(cashToReturn)));
		}

		private bool TrySetCashier()
		{
			var cashier = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(cashier == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете закрыть МЛ, так как некого указывать в качестве кассира.");
				return false;
			}

			Entity.Cashier = cashier;
			return true;
		}

		bool CheckIfCashier()
		{
			var cashSubdivisions = _subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });
			return cashSubdivisions.Contains(_employeeRepository.GetEmployeeForCurrentUser(UoW)?.Subdivision);
		}

		protected void OnAdvanceCheckboxToggled(object sender, EventArgs e)
		{
			advanceSpinbutton.Visible = advanceCheckbox.Active;
		}

		protected void OnAdvanceSpinbuttonChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		protected void OnButtonDeleteFuelDocumentClicked(object sender, EventArgs e)
		{
			FuelDocument fd = ytreeviewFuelDocuments.GetSelectedObject<FuelDocument>();
			if(fd == null) {
				return;
			}
			Entity.ObservableFuelDocuments.Remove(fd);
		}

		protected void OnButtonAddFuelDocumentClicked(object sender, EventArgs e)
		{
			var tab = new FuelDocumentViewModel(
					  UoW,
					  Entity,
					  ServicesConfig.CommonServices,
					  _subdivisionRepository,
					  _employeeRepository,
					  new FuelRepository(),
					  NavigationManagerProvider.NavigationManager,
					  _trackRepository,
					  _categoryRepository
  			);
			TabParent.AddSlaveTab(this, tab);
		}

		protected void OnYtreeviewFuelDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			var tab = new FuelDocumentViewModel(
				  UoW,
				  ytreeviewFuelDocuments.GetSelectedObject<FuelDocument>(),
				  ServicesConfig.CommonServices,
				  _subdivisionRepository,
				  _employeeRepository,
				  new FuelRepository(),
				  NavigationManagerProvider.NavigationManager,
				  _trackRepository,
				  _categoryRepository
		  	);
			TabParent.AddSlaveTab(this, tab);
		}

		protected void OnButtonCalculateCashClicked(object sender, EventArgs e)
		{
			var messages = new List<string>();

			if(!TrySetCashier()) {
				return;
			}

			if(Entity.FuelOperationHaveDiscrepancy()) {
				if(!MessageDialogHelper.RunQuestionDialog("Был изменен водитель или автомобиль, баланс по топливу изменится с учетом этих изменений. Продолжить?")) {
					return;
				}
			}

			var operationsResultMessage = Entity.UpdateCashOperations(_categoryRepository);
			messages.AddRange(operationsResultMessage);

			CalculateTotal();

			if(messages.Any()) {
				MessageDialogHelper.RunInfoDialog(string.Format("Были выполнены следующие действия:\n*{0}", string.Join("\n*", messages)));
			} else {
				MessageDialogHelper.RunInfoDialog("Сумма по кассе соответствует сумме МЛ.");
			}
		}

		#endregion

		#region Toggle buttons

		protected void OnToggleClosingToggled(object sender, EventArgs e)
		{
			if(toggleClosing.Active)
			{
				notebook1.CurrentPage = 0;
			}
		}

		protected void OnToggleWageToggled(object sender, EventArgs e)
		{
			if(toggleWageDetails.Active)
			{
				notebook1.CurrentPage = 1;
				textWageDetails.Buffer.Text = Entity.GetWageCalculationDetails(wageParameterService);
			}
		}

		#endregion
	}

}
