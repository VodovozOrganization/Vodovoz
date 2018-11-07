using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;

namespace Vodovoz
{
	public partial class ResidueDlg :  QS.Dialog.Gtk.EntityDialogBase<Residue>
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
			Entity.Date = new DateTime(2017, 4, 23);
			ypickerDocDate.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			yreferenceClientSelector.RepresentationModel = new ViewModel.CounterpartyVM(UoW);
			yreferenceClientSelector.Binding.AddBinding(Entity, e => e.Customer, w => w.Subject).InitializeFromSource();

			yreferenceDeliveryPoint.Binding.AddBinding(Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource();

			disablespinBottlesResidue.Binding.AddBinding(Entity, e => e.BottlesResidue, w => w.ValueAsInt).InitializeFromSource();
			yenumcomboDebtPaymentType.ItemsEnum = typeof(PaymentType);
			yenumcomboDebtPaymentType.Binding.AddBinding(Entity, e => e.DebtPaymentType, w => w.SelectedItem).InitializeFromSource();
			yenumcomboDebtPaymentType.Sensitive = disablespinMoneyDebt.Active;
			disablespinBottlesDeposit.Binding.AddBinding(Entity, e => e.DepositResidueBottels, w => w.ValueAsDecimal).InitializeFromSource();
			disablespinEquipmentDeposit.Binding.AddBinding(Entity, e => e.DepositResidueEquipment, w => w.ValueAsDecimal).InitializeFromSource();
			disablespinMoneyDebt.Binding.AddBinding(Entity, e => e.DebtResidue, w => w.ValueAsDecimal).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
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

			Entity.DeliveryPoint.HaveResidue = true;
			Entity.UpdateOperations(UoW);

			var valid = new QSValidator<Residue> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();
			return true;
		}

		private void UpdateResidue()
		{
			int bottleDebt;
			if(Entity.DeliveryPoint == null)
				bottleDebt = Repository.Operations.BottlesRepository.GetBottlesAtCounterparty(UoW, Entity.Customer, Entity.Date);
			else
				bottleDebt = Repository.Operations.BottlesRepository.GetBottlesAtDeliveryPoint(UoW, Entity.DeliveryPoint, Entity.Date);
			labelCurrentBootle.LabelProp = RusNumber.FormatCase(bottleDebt, "{0} бутыль", "{0} бутыли", "{0} бутылей");

			decimal bottleDeposit;
			if(Entity.DeliveryPoint == null)
				bottleDeposit = Repository.Operations.DepositRepository.GetDepositsAtCounterparty(UoW, Entity.Customer, DepositType.Bottles, Entity.Date);
			else
				bottleDeposit = Repository.Operations.DepositRepository.GetDepositsAtDeliveryPoint(UoW, Entity.DeliveryPoint, DepositType.Bottles, Entity.Date);
			labelCurrentBottleDeposit.LabelProp = CurrencyWorks.GetShortCurrencyString(bottleDeposit);

			decimal equipmentDeposit;
			if(Entity.DeliveryPoint == null)
				equipmentDeposit = Repository.Operations.DepositRepository.GetDepositsAtCounterparty(UoW, Entity.Customer, DepositType.Equipment, Entity.Date);
			else
				equipmentDeposit = Repository.Operations.DepositRepository.GetDepositsAtDeliveryPoint(UoW, Entity.DeliveryPoint, DepositType.Equipment, Entity.Date);
			labelCurrentEquipmentDeposit.LabelProp = CurrencyWorks.GetShortCurrencyString(equipmentDeposit);

			decimal debt = Repository.Operations.MoneyRepository.GetCounterpartyDebt(UoW, Entity.Customer, Entity.Date);
			labelCurrentMoneyDebt.LabelProp = CurrencyWorks.GetShortCurrencyString(debt);
		}

		protected void OnYentryreferencevmClientSelectorChanged (object sender, EventArgs e)
		{
			yreferenceDeliveryPoint.Sensitive = yreferenceClientSelector.Subject != null;
			if (yreferenceClientSelector.Subject == null)
				yreferenceDeliveryPoint.Subject = null;
			else {
				yreferenceDeliveryPoint.Subject = null;
				yreferenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM (UoW, (Counterparty)yreferenceClientSelector.Subject);
			}
			UpdateResidue();
		}

		protected void OnYreferenceDeliveryPointChanged(object sender, EventArgs e)
		{
			UpdateResidue();
		}

		protected void OnDisablespinMoneyDebtActiveChanged (object sender, EventArgs e)
		{
			yenumcomboDebtPaymentType.Sensitive = disablespinMoneyDebt.Active;
		}
	}
}

