using System;
using System.Linq;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.Repository.Store;

namespace Vodovoz
{
	public partial class MovementDocumentDlg : OrmGtkDialogBase<MovementDocument>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();
		bool isEditingStore = false;

		public MovementDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<MovementDocument> ();
			if(WarehouseRepository.WarehouseByPermission(UoWGeneric) != null) 
				Entity.FromWarehouse = WarehouseRepository.WarehouseByPermission(UoWGeneric);


			ConfigureDlg ();
			Entity.Author = Entity.ResponsiblePerson = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
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
			if (QSMain.User.Permissions["store_manage"])
				isEditingStore = true;
			
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

			referenceWarehouseTo.SubjectType = typeof(Warehouse);
			referenceWarehouseTo.Binding.AddBinding(Entity, e => e.ToWarehouse, w => w.Subject).InitializeFromSource();
			referenceWarehouseFrom.SubjectType = typeof(Warehouse);
			referenceWarehouseFrom.Binding.AddBinding(Entity, e => e.FromWarehouse, w => w.Subject).InitializeFromSource();
			referenceWarehouseFrom.Sensitive = isEditingStore;
			referenceDeliveryPointTo.CanEditReference = false;
			referenceDeliveryPointTo.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointTo.Binding.AddBinding(Entity, e => e.ToDeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceDeliveryPointFrom.CanEditReference = false;
			referenceDeliveryPointFrom.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointFrom.Binding.AddBinding(Entity, e => e.FromDeliveryPoint, w => w.Subject).InitializeFromSource();
			referenceEmployee.SubjectType = typeof(Employee);
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

			buttonDelivered.Sensitive = Entity.TransportationStatus == TransportationStatus.Submerged && !CheckUserAndStore();

			movementdocumentitemsview1.DocumentUoW = UoWGeneric;
		}

		private bool CheckUserAndStore()
		{
			if (QSMain.User.Permissions["store_production"] && Entity.ToWarehouse == WarehouseRepository.DefaultWarehouseForWater(UoWGeneric))
				return true;
			return false;
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

