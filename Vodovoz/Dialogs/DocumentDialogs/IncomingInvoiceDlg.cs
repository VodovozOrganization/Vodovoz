using System;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	public partial class IncomingInvoiceDlg : OrmGtkDialogBase<IncomingInvoice>
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public IncomingInvoiceDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<IncomingInvoice> ();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			if (CurrentUserSettings.Settings.DefaultWarehouse != null)
				Entity.Warehouse = UoWGeneric.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);

			ConfigureDlg ();
		}

		public IncomingInvoiceDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<IncomingInvoice> (id);
			ConfigureDlg ();
		}

		public IncomingInvoiceDlg (IncomingInvoice sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			entryInvoiceNumber.Binding.AddBinding (Entity, e => e.InvoiceNumber, w => w.Text).InitializeFromSource ();
			entryWaybillNumber.Binding.AddBinding (Entity, e => e.WaybillNumber, w => w.Text).InitializeFromSource ();
			labelTimeStamp.Binding.AddBinding (Entity, e => e.DateString, w => w.LabelProp).InitializeFromSource ();

			referenceWarehouse.SubjectType = typeof(Warehouse);
			referenceWarehouse.Binding.AddBinding (Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource ();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.RestrictIncludeSupplier = true;
			counterpartyFilter.RestrictIncludeCustomer = false;
			counterpartyFilter.RestrictIncludePartner = false;
			referenceContractor.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceContractor.Binding.AddBinding(Entity, e => e.Contractor, w => w.Subject);

			incominginvoiceitemsview1.DocumentUoW = UoWGeneric;
			ytextviewComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidator<IncomingInvoice> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			logger.Info ("Сохраняем входящую накладную...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

