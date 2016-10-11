using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public partial class FuelDocumentDlg : OrmGtkDialogBase<FuelDocument>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public FuelDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument>();
			ConfigureDlg ();
		}

		public FuelDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FuelDocument> (id);
			ConfigureDlg ();
		}

		public FuelDocumentDlg (FuelDocument sub) : this (sub.Id) {}

		private void ConfigureDlg ()
		{
			//comboFuelType.SetRenderTextFunc<FuelType>(x=>x.Name);
			//comboFuelType.ItemsList = UoW.GetAll<FuelType>().ToList();

			ydatepicker.Binding.AddBinding 		(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			yentrydriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			yentrydriver.Binding.AddBinding    (Entity, e => e.Driver, w => w.Subject).InitializeFromSource();
			yentryfuel.SubjectType = typeof(FuelType);
			yentryfuel.Binding.AddBinding    (Entity, e => e.Fuel, w => w.Subject).InitializeFromSource();

		}

		public override bool Save ()
		{
			var valid = new QSValidator<FuelDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем топливный документ...");
			UoWGeneric.Save();
			return true;
		}
	}
}

