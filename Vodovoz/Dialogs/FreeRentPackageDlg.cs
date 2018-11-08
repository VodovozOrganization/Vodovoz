using System;
using NHibernate.Criterion;
using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	public partial class FreeRentPackageDlg : QS.Dialog.Gtk.EntityDialogBase<FreeRentPackage>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public FreeRentPackageDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FreeRentPackage>();
			TabName = "Новый пакет бесплатной аренды";
			ConfigureDlg ();
		}

		public FreeRentPackageDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FreeRentPackage> (id);
			ConfigureDlg ();
		}

		public FreeRentPackageDlg (FreeRentPackage sub) : this (sub.Id) {}

		private void ConfigureDlg ()
		{
			dataentryName.Binding.AddBinding (Entity, e => e.Name, w => w.Text).InitializeFromSource ();
			spinDeposit.Binding.AddBinding (Entity, e => e.Deposit, w => w.ValueAsDecimal).InitializeFromSource ();
			spinMinWaterAmount.Binding.AddBinding (Entity, e => e.MinWaterAmount, w => w.ValueAsInt).InitializeFromSource ();

			referenceDepositService.SubjectType = typeof(Nomenclature);
			referenceDepositService.ItemsCriteria = UoW.Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.deposit));
			referenceDepositService.Binding.AddBinding (Entity, e => e.DepositService, w => w.Subject).InitializeFromSource ();
			referenceEquipmentType.SubjectType = typeof(EquipmentType);
			referenceEquipmentType.Binding.AddBinding (Entity, e => e.EquipmentType, w => w.Subject).InitializeFromSource ();
		}

		public override bool Save ()
		{
			var valid = new QSValidator<FreeRentPackage> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем пакет бесплатной аренды...");
			UoWGeneric.Save();
			return true;
		}
	}
}

