using System;
using QSOrmProject;
using Vodovoz.Domain;
using QSValidation;
using Vodovoz.Domain.Client;
using System.Linq;
using NHibernate.Criterion;

namespace Vodovoz
{
	public partial class ResidueDlg :  OrmGtkDialogBase<Residue>
	{
		public ResidueDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Residue> ();
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

			yspinbuttonBottlesResidue.Binding.AddBinding(Entity, e => e.BottlesResidue, w => w.ValueAsInt).InitializeFromSource();
			yspinbuttonBottlesDeposit.Binding.AddBinding(Entity, e => e.DepositResidueBottels, w => w.ValueAsDecimal).InitializeFromSource();
			yspinbuttonEquipmentDeposit.Binding.AddBinding(Entity, e => e.DepositResidueEquipment, w => w.ValueAsDecimal).InitializeFromSource();
			yspinbuttonMoney.Binding.AddBinding(Entity, e => e.MoneyResidue, w => w.ValueAsDecimal).InitializeFromSource();

			checkbuttonBottlesResidue.Active = Entity.BottlesResidue.HasValue;
		}

		public override bool Save ()
		{
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

		protected void OnCheckbuttonBottlesResidueToggled (object sender, EventArgs e)
		{
			
			yspinbuttonBottlesResidue.Sensitive = checkbuttonBottlesResidue.Active;
			if (checkbuttonBottlesResidue.Active)
				Entity.BottlesResidue = null;
			
		}
	}
}

