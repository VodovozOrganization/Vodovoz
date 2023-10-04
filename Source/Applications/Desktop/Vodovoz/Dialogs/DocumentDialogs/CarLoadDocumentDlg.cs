using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.PermissionExtensions;
using QS.Project.Services;
using QS.Report;
using Vodovoz.Controllers;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Parameters;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Models;
using Vodovoz.Tools;
using Vodovoz.Tools.Store;
using QS.Validation;
using Vodovoz.Infrastructure;
using Vodovoz.Extensions;

namespace Vodovoz
{
	public partial class CarLoadDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<CarLoadDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private IStockRepository _stockRepository = new StockRepository();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IRouteListRepository _routeListRepository =
			new RouteListRepository(new StockRepository(), new BaseParametersProvider(new ParametersProvider()));
		private readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(new ParametersProvider());
		private readonly IRouteListDailyNumberProvider _routeListDailyNumberProvider = new RouteListDailyNumberProvider(UnitOfWorkFactory.GetDefaultFactory);

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						new OrderRepository(),
						_employeeRepository,
						new BaseParametersProvider(new ParametersProvider()),
						ServicesConfig.CommonServices.UserService,
						ErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		public CarLoadDocumentDlg()
		{
			this.Build();

			ConfigureNewDoc();
			ConfigureDlg();
		}

		public CarLoadDocumentDlg(int routeListId, int? warehouseId)
		{
			this.Build();
			ConfigureNewDoc();

			if(warehouseId.HasValue) {
				Entity.Warehouse = UoW.GetById<Warehouse>(warehouseId.Value);

			}
			Entity.RouteList = UoW.GetById<RouteList>(routeListId);
			ConfigureDlg();
		}

		public CarLoadDocumentDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarLoadDocument>(id);
			ConfigureDlg();
		}

		public CarLoadDocumentDlg(CarLoadDocument sub) : this(sub.Id) { }

		void ConfigureNewDoc()
		{
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarLoadDocument>();
			Entity.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			var storeDocument = new StoreDocumentHelper(new UserSettingsGetter());
			Entity.Warehouse = storeDocument.GetDefaultWarehouse(UoW, WarehousePermissionsType.CarLoadEdit);
		}

		void ConfigureDlg()
		{
			
			var storeDocument = new StoreDocumentHelper(new UserSettingsGetter());
			if(storeDocument.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.CarLoadEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var currentUserId = ServicesConfig.CommonServices.UserService.CurrentUserId;
			var hasPermitionToEditDocWithClosedRL = 
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(
					"can_change_car_load_and_unload_docs", currentUserId);
			
			var editing = storeDocument.CanEditDocument(WarehousePermissionsType.CarLoadEdit, Entity.Warehouse);
			editing &= Entity.RouteList?.Status != RouteListStatus.Closed || hasPermitionToEditDocWithClosedRL;
			yentryrefRouteList.IsEditable = ySpecCmbWarehouses.Sensitive = ytextviewCommnet.Editable = editing;
			carloaddocumentview1.Sensitive = editing;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			ySpecCmbWarehouses.ItemsList = storeDocument.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.CarLoadEdit);
			ySpecCmbWarehouses.Binding.AddBinding(Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new RouteListsFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.RestrictedStatuses = new[] { RouteListStatus.InLoading });
			yentryrefRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryrefRouteList.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");
			yentryrefRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();

			enumPrint.ItemsEnum = typeof(CarLoadPrintableDocuments);

			UpdateRouteListInfo();
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAlreadyLoaded(UoW, _routeListRepository);
			Entity.UpdateInRouteListAmount(UoW, _routeListRepository);
			carloaddocumentview1.DocumentUoW = UoWGeneric;
			carloaddocumentview1.SetButtonEditing(editing);
			buttonSave.Sensitive = editing;
			if(!editing)
				HasChanges = false;
			if(UoW.IsNew && Entity.Warehouse != null)
				carloaddocumentview1.FillItemsByWarehouse();
			ySpecCmbWarehouses.ItemSelected += OnYSpecCmbWarehousesItemSelected;

			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			
			Entity.CanEdit =
				permmissionValidator.Validate(typeof(CarLoadDocument), currentUserId, nameof(RetroactivelyClosePermission));
			
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				yentryrefRouteList.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ySpecCmbWarehouses.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewRouteListInfo.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				carloaddocumentview1.Sensitive = false;

				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}
		}

		public override bool Save()
		{
			if(!Entity.CanEdit)
				return false;
			
			Entity.UpdateAlreadyLoaded(UoW, _routeListRepository);
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			if(Entity.Items.Any(x => x.Amount == 0))
			{
				var res = MessageDialogHelper.RunQuestionYesNoCancelDialog(
					$"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">В списке есть нулевые позиции. Убрать нулевые позиции перед сохранением?</span>");
				switch(res)
				{
					case -4:			//DeleteEvent
					case -6:			//Cancel
						return false;
					case -8:			//Yes
						Entity.ClearItemsFromZero();
						break;
					case -9:			//No
						break;
				}
			}

			Entity.UpdateOperations(UoW, _baseParametersProvider.GetNomenclatureIdForTerminal);

			logger.Info("Сохраняем погрузочный талон...");
			UoWGeneric.Save();

			logger.Info("Меняем статус маршрутного листа...");
			if(Entity.RouteList.ShipIfCan(UoW, CallTaskWorker, out _))
				MessageDialogHelper.RunInfoDialog("Маршрутный лист отгружен полностью.");
			UoW.Save(Entity.RouteList);

			UoW.Commit();

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

		protected void OnYentryrefRouteListChangedByUser(object sender, EventArgs e)
		{
			UpdateRouteListInfo();
			if(Entity.Warehouse != null && Entity.RouteList != null)
				carloaddocumentview1.FillItemsByWarehouse();
		}

		protected void OnYSpecCmbWarehousesItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			Entity.UpdateStockAmount(UoW, _stockRepository);
			Entity.UpdateAmounts();
		}

		protected void OnEnumPrintEnumItemClicked(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			if(UoWGeneric.HasChanges)
			{
				if(CommonDialogs.SaveBeforePrint(typeof(CarLoadDocument), "талона"))
				{
					if(!Save())
					{
						return;
					}
				}
				else
				{
					return;
				}
			}

			_routeListDailyNumberProvider.GetOrCreateDailyNumber(Entity.RouteList.Id, Entity.RouteList.Date);

			var reportInfo = new QS.Report.ReportInfo {
				Title = Entity.Title,
				Identifier = "Store.CarLoadDocument",
				Parameters = new System.Collections.Generic.Dictionary<string, object>
					{
						{ "id",  Entity.Id }
					},
				PrintType = ReportInfo.PrintingType.MultiplePrinters
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
		}
	}

	public enum CarLoadPrintableDocuments
	{
		[Display(Name = "Документ погрузки")]
		LoadDocument
	}
}

