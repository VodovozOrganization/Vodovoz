using System;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using System.Linq;
using NHibernate.Criterion;
using Gamma.ColumnConfig;
using Vodovoz.ViewModel;
using QSProjectsLib;

namespace Vodovoz
{
	public partial class ResidueDlg :  OrmGtkDialogBase<Residue>
	{
		public ResidueDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Residue> ();
			Entity.Author = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			ConfigureDlg ();
		}

		public ResidueDlg (Residue sub) : this (sub.Id)
		{
		}

		public ResidueDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Residue> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			ypickerDocDate.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			yreferenceClientSelector.RepresentationModel = new ViewModel.CounterpartyVM(UoW);
			yreferenceClientSelector.Binding.AddBinding(Entity, e => e.Customer, w => w.Subject).InitializeFromSource();

			//yreferenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			//yreferenceDeliveryPoint.CanEditReference = false;
			yreferenceDeliveryPoint.Binding.AddBinding(Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource();

			disablespinBottlesResidue.Binding.AddBinding(Entity, e => e.BottlesResidue, w => w.ValueAsInt).InitializeFromSource();
			disablespinBottlesDeposit.Binding.AddBinding(Entity, e => e.DepositResidueBottels, w => w.ValueAsDecimal).InitializeFromSource();
			disablespinEquipmentDeposit.Binding.AddBinding(Entity, e => e.DepositResidueEquipment, w => w.ValueAsDecimal).InitializeFromSource();
			disablespinMoney.Binding.AddBinding(Entity, e => e.MoneyResidue, w => w.ValueAsDecimal).InitializeFromSource();
		}

		public override bool Save ()
		{
			Entity.LastEditAuthor = Repository.EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditTime = DateTime.Now;
			if(Entity.LastEditAuthor == null)
			{
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}




			var valid = new QSValidator<Residue> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();
			return true;
		}


		protected void OnYentryreferencevmClientSelectorChanged (object sender, EventArgs e)
		{
			/*
			yreferenceDeliveryPoint.Sensitive = yreferenceClientSelector.Subject != null;
			if (yreferenceClientSelector.Subject != null) {
				var points = ((Counterparty)yreferenceClientSelector.Subject).DeliveryPoints.Select (o => o.Id).ToList ();
				yreferenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
					.Add (Restrictions.In ("Id", points));
			}
			*/
			yreferenceDeliveryPoint.Sensitive = yreferenceClientSelector.Subject != null;
			if (yreferenceClientSelector.Subject == null)
				yreferenceDeliveryPoint.Subject = null;
			else {
				yreferenceDeliveryPoint.Subject = null;
				yreferenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM (UoW, (Counterparty)yreferenceClientSelector.Subject);
			}
		}
	}
}

