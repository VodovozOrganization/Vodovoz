using System;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FreeRentPackageDlg : OrmGtkDialogBase<FreeRentPackage>
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
			datatable1.DataSource = subjectAdaptor;
			referenceDepositService.SubjectType = typeof(Nomenclature);
			referenceEquipmentType.SubjectType = typeof(EquipmentType);
			referenceDepositService.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.deposit));
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

