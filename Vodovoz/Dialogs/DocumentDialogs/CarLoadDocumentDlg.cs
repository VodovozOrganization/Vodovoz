using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QSOrmProject;
using Vodovoz.Additions.Store;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz
{
	public partial class CarLoadDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<CarLoadDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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
			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.CarLoadEdit);
		}

		void ConfigureDlg()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.CarLoadEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var currentUserId = QS.Project.Services.ServicesConfig.CommonServices.UserService.CurrentUserId;
			var hasPermitionToEditDocWithClosedRL = QS.Project.Services.ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_change_car_load_and_unload_docs", currentUserId);
			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.CarLoadEdit, Entity.Warehouse);
			editing &= Entity.RouteList?.Status != RouteListStatus.Closed || hasPermitionToEditDocWithClosedRL;
			yentryrefRouteList.IsEditable = ySpecCmbWarehouses.Sensitive = ytextviewCommnet.Editable = editing;
			carloaddocumentview1.Sensitive = editing;

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();
			ySpecCmbWarehouses.ItemsList = StoreDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissions.CarLoadEdit);
			ySpecCmbWarehouses.Binding.AddBinding(Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource();
			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			var filter = new RouteListsFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.RestrictStatus = RouteListStatus.InLoading);
			yentryrefRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryrefRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();
			yentryrefRouteList.CanEditReference = UserPermissionRepository.CurrentUserPresetPermissions["can_delete"];

			enumPrint.ItemsEnum = typeof(CarLoadPrintableDocuments);

			UpdateRouteListInfo();
			Entity.UpdateStockAmount(UoW);
			Entity.UpdateAlreadyLoaded(UoW, new RouteListRepository());
			Entity.UpdateInRouteListAmount(UoW, new RouteListRepository());
			carloaddocumentview1.DocumentUoW = UoWGeneric;
			carloaddocumentview1.SetButtonEditing(editing);
			buttonSave.Sensitive = editing;
			if(!editing)
				HasChanges = false;
			if(UoW.IsNew && Entity.Warehouse != null)
				carloaddocumentview1.FillItemsByWarehouse();
			ySpecCmbWarehouses.ItemSelected += OnYSpecCmbWarehousesItemSelected;

			var permmissionValidator = new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), EmployeeSingletonRepository.GetInstance(), UserSingletonRepository.GetInstance());
			Entity.CanEdit = permmissionValidator.Validate(typeof(CarLoadDocument), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));
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

			Entity.UpdateAlreadyLoaded(UoW, new RouteListRepository());
			var valid = new QS.Validation.GtkUI.QSValidator<CarLoadDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			if(Entity.Items.Any(x => x.Amount == 0)) {
				if(MessageDialogHelper.RunQuestionDialog("В списке есть нулевые позиции. Убрать нулевые позиции перед сохранением?"))
					Entity.ClearItemsFromZero();
			}

			Entity.UpdateOperations(UoW);

			logger.Info("Сохраняем погрузочный талон...");
			UoWGeneric.Save();

			logger.Info("Меняем статус маршрутного листа...");
			if(Entity.RouteList.ShipIfCan(UoW))
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
					Entity.RouteList.Car.Model,
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
			Entity.UpdateStockAmount(UoW);
			carloaddocumentview1.UpdateAmounts();
		}

		protected void OnEnumPrintEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(CarLoadDocument), "талона"))
				Save();

			var reportInfo = new QS.Report.ReportInfo {
				Title = Entity.Title,
				Identifier = CarLoadPrintableDocuments.Common.Equals(e.ItemEnum) ? "Store.CarLoadDoc" : "Store.CarLoadDocPallets",
				Parameters = new System.Collections.Generic.Dictionary<string, object>
					{
						{ "id",  Entity.Id }
					}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo),
				this);
		}
	}

	public enum CarLoadPrintableDocuments
	{
		[Display(Name = "Универсальная")]
		Common,
		[Display(Name = "С разбивной на поддоны")]
		WithPallets
	}
}

