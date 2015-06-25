using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingWaterDlg : OrmGtkDialogBase<IncomingWater>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public IncomingWaterDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<IncomingWater> ();
			ConfigureDlg ();
		}

		public IncomingWaterDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<IncomingWater> (id);
			ConfigureDlg ();
		}

		public IncomingWaterDlg (IncomingWater sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			tableWater.DataSource = subjectAdaptor;
			referenceWarehouse.SubjectType = typeof(Warehouse);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<IncomingWater> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем документ производства...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}
	}
}

