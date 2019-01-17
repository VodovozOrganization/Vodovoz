using Gamma.GtkWidgets;
using NLog;
using QS.DomainModel.UoW;
using QSValidation;
using Vodovoz.ViewModel;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz
{
	public partial class SubdivisionDlg : QS.Dialog.Gtk.EntityDialogBase<Subdivision>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public SubdivisionDlg()
		{
			this.Build();
			TabName = "Новое подразделение";
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Subdivision>();
			ConfigureDlg();
		}

		public SubdivisionDlg(int id)
		{
			this.Build();
			logger.Info("Загрузка информации о подразделении...");
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Subdivision>(id);
			ConfigureDlg();
		}

		public SubdivisionDlg(Subdivision sub) : this(sub.Id) { }

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryrefParentSubdivision.SubjectType = typeof(Subdivision);
			yentryrefParentSubdivision.Binding.AddBinding(Entity, e => e.ParentSubdivision, w => w.Subject).InitializeFromSource();
			yentryreferenceChief.RepresentationModel = new EmployeesVM(new EmployeeFilter(UoW));
			yentryreferenceChief.Binding.AddBinding(Entity, e => e.Chief, w => w.Subject).InitializeFromSource();
			yTVChildSubdivisions.ColumnsConfig = ColumnsConfigFactory.Create<Subdivision>()
				.AddColumn("Код")
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.AddTextRenderer(n => n.Name)
				.AddColumn("Руководитель")
					.AddTextRenderer(n => n.Chief.ShortName)
				.Finish();
			yTVChildSubdivisions.ItemsDataSource = SubdivisionsRepository.GetChildDepartments(UoW, Entity);
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QSValidator<Subdivision>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}