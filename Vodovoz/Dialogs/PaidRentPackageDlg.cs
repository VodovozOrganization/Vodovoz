using System;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaidRentPackageDlg : OrmGtkDialogBase<PaidRentPackage>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public PaidRentPackageDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<PaidRentPackage>();
			ConfigureDlg ();
		}

		public PaidRentPackageDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<PaidRentPackage> (id);
			ConfigureDlg ();
		}

		public PaidRentPackageDlg (PaidRentPackage sub) : this(sub.Id)
		{
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			referenceDepositService.SubjectType = typeof(Nomenclature);
			referenceDepositService.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.deposit));
			referenceRentServiceDaily.SubjectType = typeof(Nomenclature);
			referenceRentServiceDaily.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.rent));
			referenceRentServiceMonthly.SubjectType = typeof(Nomenclature);
			referenceRentServiceMonthly.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.rent));
			referenceEquipmentType.SubjectType = typeof(EquipmentType);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<PaidRentPackage> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем пакет платных услуг...");
			UoWGeneric.Save();
			return true;
		}
	}
}

