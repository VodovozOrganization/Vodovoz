using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Additions.Store;
using Vodovoz.Core.Permissions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class WriteoffDocumentDlg : OrmGtkDialogBase<WriteoffDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public WriteoffDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<WriteoffDocument> ();
			Entity.Author = Entity.ResponsibleEmployee = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			Entity.WriteoffWarehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.WriteoffEdit);
			
			ConfigureDlg ();
		}

		public WriteoffDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<WriteoffDocument> (id);
			ConfigureDlg ();
		}

		public WriteoffDocumentDlg (WriteoffDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.WriteoffEdit, Entity.WriteoffWarehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.WriteoffEdit, Entity.WriteoffWarehouse);
			comboType.Sensitive = referenceEmployee.IsEditable = referenceWarehouse.IsEditable = referenceCounterparty.IsEditable
				= referenceDeliveryPoint.IsEditable = textComment.Editable = editing;
			writeoffdocumentitemsview1.Sensitive = editing;

			textComment.Binding.AddBinding (Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource ();
			labelTimeStamp.Binding.AddBinding (Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource ();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.RestrictIncludeSupplier = false;
			counterpartyFilter.RestrictIncludeCustomer = true;
			counterpartyFilter.RestrictIncludePartner = false;
			referenceCounterparty.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceCounterparty.Binding.AddBinding(Entity, e => e.Client, w => w.Subject).InitializeFromSource();

			referenceWarehouse.SubjectType = typeof(Warehouse);
			referenceWarehouse.Binding.AddBinding (Entity, e => e.WriteoffWarehouse, w => w.Subject).InitializeFromSource ();
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.CanEditReference = false;
			referenceDeliveryPoint.Binding.AddBinding (Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource ();
			referenceEmployee.SubjectType = typeof(Employee);
			referenceEmployee.Binding.AddBinding (Entity, e => e.ResponsibleEmployee, w => w.Subject).InitializeFromSource ();
			comboType.ItemsEnum = typeof(WriteoffType);
			referenceWarehouse.Sensitive = (UoWGeneric.Root.WriteoffWarehouse != null);
			referenceDeliveryPoint.Sensitive = referenceCounterparty.Sensitive = (UoWGeneric.Root.Client != null);
			comboType.EnumItemSelected += (object sender, Gamma.Widgets.ItemSelectedEventArgs e) => {
				referenceDeliveryPoint.Sensitive = (comboType.Active == (int)WriteoffType.counterparty && UoWGeneric.Root.Client != null);
				referenceCounterparty.Sensitive = (comboType.Active == (int)WriteoffType.counterparty);
			};
			comboType.Active = UoWGeneric.Root.Client != null ?
				(int)WriteoffType.counterparty :
				(int)WriteoffType.warehouse;

			writeoffdocumentitemsview1.DocumentUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<WriteoffDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info ("Сохраняем акт списания...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnReferenceCounterpartyChanged (object sender, EventArgs e)
		{
			referenceDeliveryPoint.Sensitive = referenceCounterparty.Subject != null;
			if (referenceCounterparty.Subject != null) {
				var points = ((Counterparty)referenceCounterparty.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				referenceDeliveryPoint.ItemsCriteria = UoW.Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(WriteoffDocument), "акта выбраковки"))
				Save ();

			var reportInfo = new QSReport.ReportInfo {
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

