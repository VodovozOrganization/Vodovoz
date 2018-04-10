using System;
using Vodovoz;
using QSOrmProject;
using NLog;
using Vodovoz.Domain.Employees;
using QSValidation;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class SubdivisionDlg : OrmGtkDialogBase<Subdivision>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public SubdivisionDlg()
		{
			this.Build();
			TabName = "Новое подразделение";
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Subdivision> ();
			ConfigureDlg ();
		}

		public SubdivisionDlg(int id)
		{
			this.Build ();
			logger.Info ("Загрузка информации о подразделении...");
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Subdivision> (id);
			ConfigureDlg ();
		}

		public SubdivisionDlg(Subdivision sub) : this(sub.Id)
		{
		}

		private void ConfigureDlg ()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryreferenceChief.RepresentationModel = new EmployeesVM(new EmployeeFilter());
			yentryreferenceChief.Binding.AddBinding(Entity, e => e.Chief, w => w.Subject).InitializeFromSource();
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QSValidator<Subdivision> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			
			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}

