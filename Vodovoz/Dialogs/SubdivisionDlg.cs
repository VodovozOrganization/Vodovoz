using System.Linq;
using Gamma.Binding;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Sale;
using Vodovoz.Representations;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class SubdivisionDlg : QS.Dialog.Gtk.EntityDialogBase<Subdivision>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		SubdivisionsVM subdivisionsVM;
		ScheduleRestrictedDistrict selectedDistrict;

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

			subdivisionsVM = new SubdivisionsVM(UoW, Entity);
			repTreeChildSubdivisions.RepresentationModel = subdivisionsVM;
			repTreeChildSubdivisions.YTreeModel = new RecursiveTreeModel<SubdivisionVMNode>(subdivisionsVM.Result, x => x.Parent, x => x.Children);
			//repTreeChildSubdivisions.ExpandAll();

			var scheduleRestrictedDistrictVM = new ScheduleRestrictedDistrictVM(UoW);
			repTreeDistricts.RepresentationModel = scheduleRestrictedDistrictVM;
			repTreeDistricts.SetItemsSource<ScheduleRestrictedDistrict>(Entity.ObservableServicingDistricts);
			repTreeDistricts.Selection.Changed += (sender, e) => {
				selectedDistrict = repTreeDistricts.GetSelectedObject<ScheduleRestrictedDistrict>();
				btnRemoveDistrict.Sensitive = selectedDistrict != null;
			};
			btnRemoveDistrict.Sensitive = selectedDistrict != null;
			lblWarehouses.LineWrapMode = Pango.WrapMode.Word;
			if(Entity.Id > 0)
				lblWarehouses.Text = Entity.GetWarehousesNames(UoW);
			else
				frmWarehoses.Visible = false;
		}

		protected void OnBtnAddDistrictClicked(object sender, System.EventArgs e)
		{
			var refWin = new ReferenceRepresentation(new ScheduleRestrictedDistrictVM(UoW));
			refWin.Mode = OrmReferenceMode.MultiSelect;
			refWin.ButtonMode = ReferenceButtonMode.None;
			refWin.ObjectSelected += RefWin_ObjectSelected;
			TabParent.AddSlaveTab(this, refWin);
		}

		void RefWin_ObjectSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var districts = e.Selected.Select(o => o.VMNode as ScheduleRestrictedDistrict);
			foreach(var d in districts) {
				if(d != null && !Entity.ObservableServicingDistricts.Any(x => x == d))
					Entity.ObservableServicingDistricts.Add(d);
			}
		}

		protected void OnBtnRemoveDistrictClicked(object sender, System.EventArgs e)
		{
			if(selectedDistrict != null)
				Entity.ObservableServicingDistricts.Remove(selectedDistrict);
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