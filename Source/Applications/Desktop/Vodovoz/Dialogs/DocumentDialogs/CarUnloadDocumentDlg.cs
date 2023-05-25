using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repository.Store;
using Vodovoz.ViewWidgets.Store;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Equipments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Parameters;
using Vodovoz.Domain.Operations;
using Vodovoz.Tools.Store;

namespace Vodovoz
{
	public partial class CarUnloadDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<CarUnloadDocument>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(_parametersProvider);

		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly ITrackRepository _trackRepository = new TrackRepository();
		private readonly IEquipmentRepository _equipmentRepository = new EquipmentRepository();
		private readonly ICarUnloadRepository _carUnloadRepository = new CarUnloadRepository();
		private readonly IRouteListRepository
			_routeListRepository = new RouteListRepository(new StockRepository(), _baseParametersProvider);
		private WageParameterService wageParameterService =
			new WageParameterService(new WageCalculationRepository(), _baseParametersProvider);
		private CallTaskWorker callTaskWorker;

		#region Конструкторы
		public CarUnloadDocumentDlg()
		{
			this.Build();
			ConfigureNewDoc();
			ConfigureDlg();
		}


		public CarUnloadDocumentDlg(int routeListId, int? warehouseId)
		{
			this.Build();
			ConfigureNewDoc();

			if(warehouseId.HasValue)
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);
			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			ConfigureDlg();
		}

		public CarUnloadDocumentDlg(int routeListId, int warehouseId, DateTime date) : this(routeListId, warehouseId)
		{
			Entity.TimeStamp = date;
		}

		public CarUnloadDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarUnloadDocument>(id);
			ConfigureDlg();
		}

		public CarUnloadDocumentDlg(CarUnloadDocument sub) : this(sub.Id) { }
		#endregion

		#region Методы

		void ConfigureNewDoc()
		{
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarUnloadDocument>();
			Entity.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			var storeDocument = new StoreDocumentHelper(new UserSettingsGetter());
			Entity.Warehouse = storeDocument.GetDefaultWarehouse(UoW, WarehousePermissionsType.CarUnloadEdit);
		}

		void ConfigureDlg()
		{
			var storeDocument = new StoreDocumentHelper(new UserSettingsGetter());
			callTaskWorker = new CallTaskWorker(
				CallTaskSingletonFactory.GetInstance(),
				new CallTaskRepository(),
				new OrderRepository(),
				_employeeRepository,
				_baseParametersProvider,
				ServicesConfig.CommonServices.UserService,
				ErrorReporter.Instance);

			if(storeDocument.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.CarUnloadEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var currentUserId = ServicesConfig.UserService.CurrentUserId;
			var hasPermitionToEditDocWithClosedRL =
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(
					"can_change_car_load_and_unload_docs", currentUserId);
			
			var editing = storeDocument.CanEditDocument(WarehousePermissionsType.CarUnloadEdit, Entity.Warehouse);
			editing &= Entity.RouteList?.Status != RouteListStatus.Closed || hasPermitionToEditDocWithClosedRL;
			Entity.InitializeDefaultValues(UoW, new NomenclatureRepository(new NomenclatureParametersProvider(_parametersProvider)));
			yentryrefRouteList.IsEditable = ySpecCmbWarehouses.Sensitive = ytextviewCommnet.Editable = editing;
			returnsreceptionview.Sensitive =
				hbxTareToReturn.Sensitive =
					nonserialequipmentreceptionview1.Sensitive =
						defectiveitemsreceptionview1.Sensitive = editing;

			// 20230309 Если спустя время не понадобится, то вырезать всё, что связано с этим, вместе с CarUnloadDocument.TareToReturn
			hbxTareToReturn.Visible = false;

			defectiveitemsreceptionview1.UoW =
				returnsreceptionview.UoW = UoW;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			ySpecCmbWarehouses.ItemsList = storeDocument.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.CarUnloadEdit);
			ySpecCmbWarehouses.Binding.AddBinding(Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new RouteListsFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.RestrictedStatuses = new[]{ RouteListStatus.EnRoute});
			yentryrefRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryrefRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();
			yentryrefRouteList.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			Entity.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(Entity.Warehouse)) OnWarehouseChanged();
                if (e.PropertyName == nameof(Entity.RouteList)) UpdateWidgetsVisible();
            };

            lblTareReturnedBefore.Binding.AddFuncBinding(Entity, e => e.ReturnedTareBeforeText, w => w.Text).InitializeFromSource();
			spnTareToReturn.Binding.AddBinding(Entity, e => e.TareToReturn, w => w.ValueAsInt).InitializeFromSource();

			defectiveitemsreceptionview1.Warehouse = returnsreceptionview.Warehouse = Entity.Warehouse;

			UpdateWidgetsVisible();
			buttonSave.Sensitive = editing;
			if(!editing)
				HasChanges = false;
			if(!UoW.IsNew)
				LoadReception();

			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			
			Entity.CanEdit =
				permmissionValidator.Validate(typeof(CarUnloadDocument), currentUserId, nameof(RetroactivelyClosePermission));
			
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				yentryrefRouteList.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ySpecCmbWarehouses.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewRouteListInfo.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				spnTareToReturn.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				defectiveitemsreceptionview1.Sensitive = false;
				nonserialequipmentreceptionview1.Sensitive = false;
				returnsreceptionview.Sensitive = false;

				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}

			spnTareToReturn.ValueChanged += (sender, e) => HasChanges = true;
			((GenericObservableList<ReceptionItemNode>)returnsreceptionview.Items).ListContentChanged += (sender, e) => HasChanges = true;
			((GenericObservableList<ReceptionNonSerialEquipmentItemNode>)nonserialequipmentreceptionview1.Items).ListContentChanged +=
				(sender, e) => HasChanges = true;
			((GenericObservableList<DefectiveItemNode>)defectiveitemsreceptionview1.Items).ListContentChanged += (sender, e) => HasChanges = true;
		}

		public override bool Save()
		{
			if(!Entity.CanEdit)
				return false;

			if(!UpdateReceivedItemsOnEntity(_baseParametersProvider.GetNomenclatureIdForTerminal))
				return false;

			var valid = new QS.Validation.QSValidator<CarUnloadDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			if(!_carUnloadRepository.IsUniqueDocumentAtDay(UoW, Entity.RouteList, Entity.Warehouse, Entity.Id)) {
				MessageDialogHelper.RunWarningDialog("Документ по данному МЛ и складу уже сформирован");
				return false;
			}

			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			if (Entity.RouteList.Status == RouteListStatus.Delivered)
			{
				Entity.RouteList.CompleteRouteAndCreateTask(wageParameterService, callTaskWorker, _trackRepository);
			}
			
			logger.Info("Сохраняем разгрузочный талон...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		void UpdateRouteListInfo()
		{
			if(Entity.RouteList == null) {
				ytextviewRouteListInfo.Buffer.Text = string.Empty;
				return;
			}

			ytextviewRouteListInfo.Buffer.Text =
				string.Format("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
					Entity.RouteList.Id,
					Entity.RouteList.Date,
					Entity.RouteList.Driver.FullName,
					Entity.RouteList.Car.CarModel.Name,
					Entity.RouteList.Car.RegistrationNumber,
					Entity.RouteList.Forwarder != null ? Entity.RouteList.Forwarder.FullName : "(Отсутствует)"
				);
		}

		void FillOtherReturnsTable()
		{
			if(Entity.RouteList == null || Entity.Warehouse == null)
				return;
			Dictionary<int, decimal> returns = _carUnloadRepository.NomenclatureUnloaded(UoW, Entity.RouteList, Entity.Warehouse, Entity);

			treeOtherReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<Nomenclature>()
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Количество").AddTextRenderer(x => ((int)returns[x.Id]).ToString())
				.Finish();

			Nomenclature nomenclatureAlias = null;

			var query = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
						   .WhereRestrictionOn(() => nomenclatureAlias.Id)
						   .IsIn(returns.Keys)
						   .List<Nomenclature>();

			treeOtherReturns.ItemsDataSource = query;
		}

		void SetupForNewRouteList()
		{
			UpdateRouteListInfo();
			
			nonserialequipmentreceptionview1.RouteList =
				defectiveitemsreceptionview1.RouteList =
					returnsreceptionview.RouteList = Entity.RouteList;
		}

		private void UpdateWidgetsVisible()
		{
			//20230320 Если не понадобится после обновления, вырезать всё, что с этим связано
			lblTareReturnedBefore.Visible = false; // Entity.RouteList != null;
			nonserialequipmentreceptionview1.Visible = Entity.Warehouse != null && Entity.Warehouse.CanReceiveEquipment;
		}

		void LoadReception()
		{
			//TODO проверить работу диалога
			foreach(var item in Entity.Items) {
				if(defectiveitemsreceptionview1.Items.Any(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id))
					continue;

				var returned = 
					returnsreceptionview.Items.FirstOrDefault(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);
				
				if(returned != null)
				{
					returned.Amount = (int)item.GoodsAccountingOperation.Amount;
					returned.Redhead = item.Redhead;
					continue;
				}

				switch(item.ReciveType) {
					case ReciveTypes.Equipment:
						var equipmentByNomenclature = nonserialequipmentreceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);
						if(equipmentByNomenclature != null) {
							equipmentByNomenclature.Amount = (int)item.GoodsAccountingOperation.Amount;
							continue;
						}
						nonserialequipmentreceptionview1.Items.Add(
							new ReceptionNonSerialEquipmentItemNode {
								NomenclatureCategory = NomenclatureCategory.equipment,
								NomenclatureId = item.GoodsAccountingOperation.Nomenclature.Id,
								Amount = (int)item.GoodsAccountingOperation.Amount,
								Name = item.GoodsAccountingOperation.Nomenclature.Name
							}
						);
						continue;
					case ReciveTypes.Bottle:
					case ReciveTypes.Returnes:
					case ReciveTypes.ReturnCashEquipment:
						break;
					case ReciveTypes.Defective:
						var defective = defectiveitemsreceptionview1.Items.FirstOrDefault(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature.Id);
						if(defective != null) {
							defective.Amount = (int)item.GoodsAccountingOperation.Amount;
							continue;
						}
						defectiveitemsreceptionview1.Items.Add(
							new DefectiveItemNode {
								NomenclatureCategory = item.GoodsAccountingOperation.Nomenclature.Category,
								NomenclatureId = item.GoodsAccountingOperation.Nomenclature.Id,
								Amount = (int)item.GoodsAccountingOperation.Amount,
								Name = item.GoodsAccountingOperation.Nomenclature.Name,
								Source = item.DefectSource,
								TypeOfDefect = item.TypeOfDefect
							}
						);
						continue;
				}

				logger.Warn("Номенклатура {0} не найдена в заказа мл, добавляем отдельно...", item.GoodsAccountingOperation.Nomenclature);
				var newItem = new ReceptionItemNode(item);
				returnsreceptionview.AddItem(newItem);
			}
		}

		bool UpdateReceivedItemsOnEntity(int terminalId)
		{
			//Собираем список всего на возврат из разных виджетов.
			var tempItemList = new List<InternalItem>();
			if(Entity.TareToReturn > 0)
				tempItemList.Add(
					new InternalItem {
						ReciveType = ReciveTypes.Bottle,
						NomenclatureId = Entity.DefBottleId,
						Amount = Entity.TareToReturn
					}
				);

			var defectiveItemsList = new List<InternalItem>();
			foreach(var node in defectiveitemsreceptionview1.Items) {
				if(node.Amount == 0)
					continue;

				var item = new InternalItem {
					ReciveType = ReciveTypes.Defective,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount,
					MovementOperationId = node.MovementOperation?.Id ?? 0,
					TypeOfDefect = node.TypeOfDefect,
					Source = node.Source
				};

				if(!defectiveItemsList.Any(i => i.EqualsToAnotherInternalItem(item)))
					defectiveItemsList.Add(item);
			}

			foreach(var node in returnsreceptionview.Items) {
				if(node.Amount == 0)
					continue;

				var item = new InternalItem {
					ReciveType = node.NomenclatureId == terminalId
						? ReciveTypes.ReturnCashEquipment
						: ReciveTypes.Returnes,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount,
					Redhead = node.Redhead
				};

				tempItemList.Add(item);
			}

			foreach(var node in nonserialequipmentreceptionview1.Items) {
				if(node.Amount == 0)
					continue;

				var item = new InternalItem {
					ReciveType = ReciveTypes.Equipment,
					NomenclatureId = node.NomenclatureId,
					Amount = node.Amount
				};
				tempItemList.Add(item);
			}

			//Обновляем Entity
			foreach(var tempItem in defectiveItemsList) {
				//валидация брака
				if(tempItem.TypeOfDefect == null) {
					MessageDialogHelper.RunWarningDialog("Для брака необходимо указать его вид");
					return false;
				}

				//проверка на дубли. если несколько одинаковых, то устанавливаем кол-во в 0 для последующего удаления из коллекции
				if(tempItem.Amount > 0 && defectiveItemsList.Count(i => i.EqualsToAnotherInternalItem(tempItem)) > 1)
					tempItem.Amount = 0;
			}

			foreach(var tempItem in defectiveItemsList) {
				var item = Entity.Items.FirstOrDefault(x => x.GoodsAccountingOperation.Id > 0 && x.GoodsAccountingOperation.Id == tempItem.MovementOperationId);
				if(item == null) {
					Entity.AddItem(
						tempItem.ReciveType,
						UoW.GetById<Nomenclature>(tempItem.NomenclatureId),
						null,
						tempItem.Amount,
						null,
						terminalId,
						null,
						tempItem.Source,
						tempItem.TypeOfDefect
					);
				} else {
					if(item.GoodsAccountingOperation.Amount != tempItem.Amount)
						item.GoodsAccountingOperation.Amount = tempItem.Amount;
					if(item.TypeOfDefect != tempItem.TypeOfDefect)
						item.TypeOfDefect = tempItem.TypeOfDefect;
					if(item.DefectSource != tempItem.Source)
						item.DefectSource = tempItem.Source;
				}
			}

			var nomenclatures = UoW.GetById<Nomenclature>(tempItemList.Select(x => x.NomenclatureId).ToArray());
			foreach(var tempItem in tempItemList) {
				var item = Entity.Items.FirstOrDefault(x => x.GoodsAccountingOperation.Nomenclature.Id == tempItem.NomenclatureId);
				if(item == null) {
					var nomenclature = nomenclatures.First(x => x.Id == tempItem.NomenclatureId);
					Entity.AddItem(
						tempItem.ReciveType,
						nomenclature,
						null,
						tempItem.Amount,
						null,
						terminalId,
						tempItem.Redhead
					);
				} else {
					if(item.GoodsAccountingOperation.Amount != tempItem.Amount)
						item.GoodsAccountingOperation.Amount = tempItem.Amount;
					if(item.EmployeeNomenclatureMovementOperation != null && item.EmployeeNomenclatureMovementOperation.Amount != -tempItem.Amount)
						item.EmployeeNomenclatureMovementOperation.Amount = -tempItem.Amount;

					if(item.Redhead != tempItem.Redhead)
						item.Redhead = tempItem.Redhead;

					item.CreateOrUpdateDeliveryFreeBalanceOperation(terminalId);
				}
			}

			foreach(var item in Entity.Items.ToList()) {
				bool exist = true;
				if(item.ReciveType != ReciveTypes.Defective)
					exist = tempItemList.Any(x => x.NomenclatureId == item.GoodsAccountingOperation.Nomenclature?.Id);
				else
					exist = defectiveItemsList.Any(x => x.MovementOperationId == item.GoodsAccountingOperation.Id && x.Amount > 0);

				if(!exist) {
					UoW.Delete(item.GoodsAccountingOperation);
					Entity.ObservableItems.Remove(item);
				}
			}

			return true;
		}
		#endregion

		#region События
		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(CarUnloadDocument), "талона"))
				Save();

			var reportInfo = new QS.Report.ReportInfo {
				Title = Entity.Title,
				Identifier = "Store.CarUnloadDoc",
				Parameters = new Dictionary<string, object>
					{
						{ "id",  Entity.Id }
					}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
		}

		protected void OnWarehouseChanged()
		{
			UpdateWidgetsVisible();
			returnsreceptionview.Warehouse = Entity.Warehouse;
			FillOtherReturnsTable();
		}

		protected void OnYentryrefRouteListChanged(object sender, EventArgs e)
		{
			SetupForNewRouteList();
			FillOtherReturnsTable();
			Entity.ReturnedEmptyBottlesBefore(UoW, _routeListRepository);
		}
		#endregion

		class InternalItem
		{
			public ReciveTypes ReciveType;
			public int NomenclatureId;

			public decimal Amount;
			public string Redhead;

			public DefectSource Source;
			public CullingCategory TypeOfDefect;

			public int MovementOperationId;

			public bool EqualsToAnotherInternalItem(InternalItem item)
			{
				if(item.TypeOfDefect == null || TypeOfDefect == null)
					return false;
				bool eq = item.ReciveType == ReciveType;
				eq &= item.Source == Source;
				eq &= item.NomenclatureId == NomenclatureId;
				eq &= item.TypeOfDefect.Id == TypeOfDefect.Id;
				return eq;
			}
		}
	}
}

