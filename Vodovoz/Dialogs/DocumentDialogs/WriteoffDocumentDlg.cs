using System;
using System.Linq;
using NHibernate.Criterion;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
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
			tableWriteoff.DataSource = subjectAdaptor;
			referenceCounterparty.SubjectType = typeof(Counterparty);
			referenceCounterparty.ItemsCriteria = UoWGeneric.Session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Eq ("CounterpartyType", CounterpartyType.customer));
			referenceWarehouse.SubjectType = typeof(Warehouse);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.CanEditReference = false;
			referenceEmployee.SubjectType = typeof(Employee);
			comboType.Sensitive = true;
			comboType.ItemsEnum = typeof(WriteoffType);
			referenceWarehouse.Sensitive = (UoWGeneric.Root.WriteoffWarehouse != null);
			referenceDeliveryPoint.Sensitive = referenceCounterparty.Sensitive = (UoWGeneric.Root.Client != null);
			comboType.EnumItemSelected += (object sender, EnumItemClickedEventArgs e) => {
				referenceDeliveryPoint.Sensitive = (comboType.Active == (int)WriteoffType.counterparty && UoWGeneric.Root.Client != null);
				referenceCounterparty.Sensitive = (comboType.Active == (int)WriteoffType.counterparty);
				referenceWarehouse.Sensitive = (comboType.Active == (int)WriteoffType.warehouse);
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
				referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
		}
	}
}

