using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	public partial class MeasurementUnitsDlg : OrmGtkDialogBase<MeasurementUnits>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public MeasurementUnitsDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<MeasurementUnits>();
			ConfigureDlg ();
		}

		public MeasurementUnitsDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<MeasurementUnits> (id);
			ConfigureDlg ();
		}

		public MeasurementUnitsDlg (MeasurementUnits sub): this(sub.Id) {}


		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<MeasurementUnits> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем единицы измерения...");
			UoWGeneric.Save();
			return true;
		}

	}
}

