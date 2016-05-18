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

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MovementDocumentDlg : OrmGtkDialogBase<MovementDocument>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public MovementDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<MovementDocument> ();
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
			tableSender.DataSource = subjectAdaptor;
			tableCommon.DataSource = subjectAdaptor;
			tableReceiver.DataSource = subjectAdaptor;
			referenceCounterpartyTo.SubjectType = typeof(Counterparty);
			referenceCounterpartyFrom.SubjectType = typeof(Counterparty);
			referenceCounterpartyTo.ItemsCriteria = UoWGeneric.Session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.customer));
			referenceCounterpartyFrom.ItemsCriteria = UoWGeneric.Session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.customer));
			referenceWarehouseTo.SubjectType = typeof(Warehouse);
			referenceWarehouseFrom.SubjectType = typeof(Warehouse);
			referenceDeliveryPointTo.CanEditReference = false;
			referenceDeliveryPointTo.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPointFrom.CanEditReference = false;
			referenceDeliveryPointFrom.SubjectType = typeof(DeliveryPoint);
			referenceEmployee.SubjectType = typeof(Employee);

			yentryrefWagon.SubjectType = typeof(MovementWagon);
			yentryrefWagon.Binding.AddBinding(Entity, e => e.MovementWagon, w => w.Subject).InitializeFromSource();

			ylabelTransportationStatus.Binding.AddBinding(Entity, e => e.TransportationDescription, w => w.LabelProp).InitializeFromSource();

			enumMovementType.ItemsEnum = typeof(MovementDocumentCategory);
			enumMovementType.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			buttonDelivered.Sensitive = Entity.TransportationStatus == TransportationStatus.Submerged;

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
				referenceDeliveryPointFrom.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}

		protected void OnReferenceCounterpartyToChanged (object sender, EventArgs e)
		{
			referenceDeliveryPointTo.Sensitive = referenceCounterpartyTo.Subject != null;
			if (referenceCounterpartyTo.Subject != null) {
				var points = ((Counterparty)referenceCounterpartyTo.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				referenceDeliveryPointTo.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
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

