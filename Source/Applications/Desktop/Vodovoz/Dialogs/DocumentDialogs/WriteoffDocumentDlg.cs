using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Additions.Store;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Project.Services;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	public partial class WriteoffDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<WriteoffDocument>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly StoreDocumentHelper _storeDocumentHelper = new StoreDocumentHelper();

		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel =
			new DeliveryPointJournalFilterViewModel();

		public WriteoffDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<WriteoffDocument>();
			Entity.Author = Entity.ResponsibleEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null)
			{
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.Warehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.WriteoffEdit);
			
			ConfigureDlg ();
		}

		public WriteoffDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<WriteoffDocument> (id);
			comboType.Sensitive = false;
			ConfigureDlg ();
		}

		public WriteoffDocumentDlg (WriteoffDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			if(_storeDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.WriteoffEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.WriteoffEdit, Entity.Warehouse);
			evmeEmployee.IsEditable = textComment.Editable = editing;
			writeoffdocumentitemsview1.Sensitive = editing && (Entity.Warehouse != null || Entity.Client != null);

			textComment.Binding.AddBinding (Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource ();
			labelTimeStamp.Binding.AddBinding (Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource ();

			var clientFactory = new CounterpartyJournalFactory();
			evmeCounterparty.SetEntityAutocompleteSelectorFactory(clientFactory.CreateCounterpartyAutocompleteSelectorFactory());
			evmeCounterparty.Binding.AddBinding(Entity, e => e.Client, w => w.Subject).InitializeFromSource();
			evmeCounterparty.Changed += OnReferenceCounterpartyChanged;

			ySpecCmbWarehouses.ItemsList = _storeDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.WriteoffEdit);
			ySpecCmbWarehouses.Binding.AddBinding (Entity, e => e.Warehouse, w => w.SelectedItem).InitializeFromSource ();
			ySpecCmbWarehouses.ItemSelected += (sender, e) => {
				writeoffdocumentitemsview1.Sensitive = editing && (Entity.Warehouse != null || Entity.Client != null);
			};

			var dpFactory = new DeliveryPointJournalFactory(_deliveryPointJournalFilterViewModel);
			evmeDeliveryPoint.SetEntityAutocompleteSelectorFactory(dpFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());
			evmeDeliveryPoint.CanEditReference = false;
			evmeDeliveryPoint.Binding.AddBinding(Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource();
			
			var userHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			if(userHasOnlyAccessToWarehouseAndComplaints)
			{
				evmeEmployee.CanEditReference = false;
			}
			
			var employeeFactory = new EmployeeJournalFactory();
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Binding.AddBinding (Entity, e => e.ResponsibleEmployee, w => w.Subject).InitializeFromSource ();
			comboType.ItemsEnum = typeof(WriteoffType);
			evmeDeliveryPoint.Sensitive = evmeCounterparty.Sensitive = (UoWGeneric.Root.Client != null);
			comboType.EnumItemSelected += (object sender, Gamma.Widgets.ItemSelectedEventArgs e) => {
				ySpecCmbWarehouses.Sensitive = WriteoffType.warehouse.Equals(comboType.SelectedItem);
				evmeDeliveryPoint.Sensitive = WriteoffType.counterparty.Equals(comboType.SelectedItem) && UoWGeneric.Root.Client != null;
				evmeCounterparty.Sensitive = WriteoffType.counterparty.Equals(comboType.SelectedItem);
			};
			//FIXME Списание с контрагента не реализовано. Поэтому блокирует выбор типа списания.
			comboType.Sensitive = false;
			comboType.SelectedItem = UoWGeneric.Root.Client != null ?
				WriteoffType.counterparty :
				WriteoffType.warehouse;

			writeoffdocumentitemsview1.DocumentUoW = UoWGeneric;

			Entity.ObservableItems.ElementAdded += (aList, aIdx) => ySpecCmbWarehouses.Sensitive = !Entity.ObservableItems.Any();
			Entity.ObservableItems.ElementRemoved += (aList, aIdx, aObject) => ySpecCmbWarehouses.Sensitive = !Entity.ObservableItems.Any();
			ySpecCmbWarehouses.Sensitive = editing && !Entity.Items.Any();

			var permmissionValidator =
				new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
			
			Entity.CanEdit =
				permmissionValidator.Validate(
					typeof(WriteoffDocument), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				ySpecCmbWarehouses.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				evmeCounterparty.Sensitive = false;
				evmeDeliveryPoint.Sensitive = false;
				comboType.Sensitive = false;
				evmeEmployee.Sensitive = false;
				textComment.Sensitive = false;
				writeoffdocumentitemsview1.Sensitive = false;

				buttonSave.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}
		}

		public override bool Save ()
		{
			if(!Entity.CanEdit)
				return false;

			var valid = new QSValidator<WriteoffDocument>(UoWGeneric.Root);
			if (valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info ("Сохраняем акт списания...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnReferenceCounterpartyChanged (object sender, EventArgs e)
		{
			evmeDeliveryPoint.Sensitive = evmeCounterparty.Subject != null;
			if(evmeCounterparty.Subject != null)
			{
				_deliveryPointJournalFilterViewModel.Counterparty = evmeCounterparty.Subject as Counterparty;
			}
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(WriteoffDocument), "акта выбраковки"))
				Save ();

			var reportInfo = new QS.Report.ReportInfo {
				Title = String.Format ("Акт выбраковки №{0} от {1:d}", Entity.Id, Entity.TimeStamp),
				Identifier = "Store.WriteOff",
				Parameters = new Dictionary<string, object> {
					{ "writeoff_id",  Entity.Id }
				}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg (reportInfo)
			);
		}
	}
}
