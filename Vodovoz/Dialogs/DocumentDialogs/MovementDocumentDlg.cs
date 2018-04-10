using System;
using System.Linq;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Additions.Store;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class MovementDocumentDlg : OrmGtkDialogBase<MovementDocument>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public MovementDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<MovementDocument> ();

			Entity.Author = Entity.ResponsiblePerson = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.FromWarehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.MovementEdit);
			Entity.ToWarehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.MovementEdit);
			
			ConfigureDlg ();
		}

		public MovementDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<MovementDocument> (id);
			ConfigureDlg ();
		}

		public MovementDocumentDlg (MovementDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.MovementEdit, Entity.FromWarehouse, Entity.ToWarehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.MovementEdit, Entity.FromWarehouse, Entity.ToWarehouse);
			enumMovementType.Sensitive = referenceEmployee.IsEditable = referenceWarehouseTo.Sensitive
				 = yentryrefWagon.IsEditable = textComment.Sensitive = editing;
			movementdocumentitemsview1.Sensitive = editing;

			textComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelTimeStamp.Binding.AddBinding(Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.RestrictIncludeSupplier = false;
			counterpartyFilter.RestrictIncludeCustomer = true;
			counterpartyFilter.RestrictIncludePartner = false;
			referenceCounterpartyFrom.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceCounterpartyFrom.Binding.AddBinding(Entity, e => e.FromClient, w => w.Subject).InitializeFromSource();

			counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.RestrictIncludeSupplier = false;
			counterpartyFilter.RestrictIncludeCustomer = true;
			counterpartyFilter.RestrictIncludePartner = false;
			referenceCounterpartyTo.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceCounterpartyTo.Binding.AddBinding(Entity, e => e.ToClient, w => w.Subject).InitializeFromSource();

			referenceWarehouseTo.SubjectType = typeof(Warehouse); //Можем выбрать любой склад, куда везем, а не только склад к которому имеем доступ.
			referenceWarehouseTo.Binding.AddBinding(Entity, e => e.ToWarehouse, w => w.Subject).InitializeFromSource();
			referenceWarehouseFrom.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.MovementEdit);
			referenceWarehouseFrom.Binding.AddBinding(Entity, e => e.FromWarehouse, w => w.Subject).InitializeFromSource();
			referenceWarehouseFrom.IsEditable = StoreDocumentHelper.CanEditDocument(WarehousePermissions.MovementEdit, Entity.FromWarehouse);
			referenceDeliveryPointTo.CanEditReference = false;
			referenceDeliveryPointTo.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointTo.Binding.AddBinding(Entity, e => e.ToDeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceDeliveryPointFrom.CanEditReference = false;
			referenceDeliveryPointFrom.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointFrom.Binding.AddBinding(Entity, e => e.FromDeliveryPoint, w => w.Subject).InitializeFromSource();
			var filterEmployee = new EmployeeFilter(UoW);
			referenceEmployee.RepresentationModel = new EmployeesVM(filterEmployee);
			referenceEmployee.Binding.AddBinding(Entity, e => e.ResponsiblePerson, w => w.Subject).InitializeFromSource();

			yentryrefWagon.SubjectType = typeof(MovementWagon);
			yentryrefWagon.Binding.AddBinding(Entity, e => e.MovementWagon, w => w.Subject).InitializeFromSource();

			ylabelTransportationStatus.Binding.AddBinding(Entity, e => e.TransportationDescription, w => w.LabelProp).InitializeFromSource();

			MovementDocumentCategory[] filteredDoctypeList = { MovementDocumentCategory.counterparty };
			object[] MovementDocumentList = Array.ConvertAll(filteredDoctypeList, x => (object)x);
			enumMovementType.ItemsEnum = typeof(MovementDocumentCategory);
			enumMovementType.AddEnumToHideList(MovementDocumentList);
			enumMovementType.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();
			Entity.Category = MovementDocumentCategory.Transportation;

			buttonDelivered.Sensitive = Entity.TransportationStatus == TransportationStatus.Submerged 
				&& CurrentPermissions.Warehouse[WarehousePermissions.MovementEdit, Entity.ToWarehouse];

			movementdocumentitemsview1.DocumentUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<MovementDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info ("Сохраняем документ перемещения...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnEnumMovementTypeChanged (object sender, EventArgs e)
		{
			var selected = Entity.Category;
			referenceWarehouseTo.Visible = referenceWarehouseFrom.Visible = labelStockFrom.Visible = labelStockTo.Visible 
				= (selected == MovementDocumentCategory.warehouse || selected == MovementDocumentCategory.Transportation);

			referenceCounterpartyTo.Visible = referenceCounterpartyFrom.Visible = labelClientFrom.Visible = labelClientTo.Visible
				= referenceDeliveryPointFrom.Visible = referenceDeliveryPointTo.Visible = labelPointFrom.Visible = labelPointTo.Visible
				= (selected == MovementDocumentCategory.counterparty);
			referenceDeliveryPointFrom.Sensitive = (referenceCounterpartyFrom.Subject != null && selected == MovementDocumentCategory.counterparty);
			referenceDeliveryPointTo.Sensitive = (referenceCounterpartyTo.Subject != null && selected == MovementDocumentCategory.counterparty);

			//Траспортировка
			labelWagon.Visible = hboxTransportation.Visible = yentryrefWagon.Visible = labelTransportationTitle.Visible
				= selected == MovementDocumentCategory.Transportation;
		}

		protected void OnReferenceCounterpartyFromChanged (object sender, EventArgs e)
		{
			referenceDeliveryPointFrom.Sensitive = referenceCounterpartyFrom.Subject != null;
			if (referenceCounterpartyFrom.Subject != null) {
				var points = ((Counterparty)referenceCounterpartyFrom.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				referenceDeliveryPointFrom.ItemsCriteria = UoWGeneric.Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}

		protected void OnReferenceCounterpartyToChanged (object sender, EventArgs e)
		{
			referenceDeliveryPointTo.Sensitive = referenceCounterpartyTo.Subject != null;
			if (referenceCounterpartyTo.Subject != null) {
				var points = ((Counterparty)referenceCounterpartyTo.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				referenceDeliveryPointTo.ItemsCriteria = UoWGeneric.Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}

		protected void OnButtonDeliveredClicked(object sender, EventArgs e)
		{
			buttonDelivered.Sensitive = false;
			Entity.TransportationCompleted();
		}
	}
}

