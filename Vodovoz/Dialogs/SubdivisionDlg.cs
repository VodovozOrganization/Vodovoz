using System.Linq;
using Gamma.Binding;
using Gamma.GtkWidgets;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QSOrmProject;
using QSProjectsLib;
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
			subdivisionentitypermissionwidget.ConfigureDlg(UoW, Entity);
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryrefParentSubdivision.SubjectType = typeof(Subdivision);
			yentryrefParentSubdivision.Binding.AddBinding(Entity, e => e.ParentSubdivision, w => w.Subject).InitializeFromSource();
			yentryreferenceChief.RepresentationModel = new EmployeesVM(new EmployeeFilter(UoW));
			yentryreferenceChief.Binding.AddBinding(Entity, e => e.Chief, w => w.Subject).InitializeFromSource();

			subdivisionsVM = new SubdivisionsVM(UoW, Entity);
			repTreeChildSubdivisions.RepresentationModel = subdivisionsVM;
			repTreeChildSubdivisions.YTreeModel = new RecursiveTreeModel<SubdivisionVMNode>(subdivisionsVM.Result, x => x.Parent, x => x.Children);

			ySpecCmbGeographicGroup.ItemsList = UoW.Session.QueryOver<GeographicGroup>().List();
			ySpecCmbGeographicGroup.Binding.AddBinding(Entity, e => e.GeographicGroup, w => w.SelectedItem).InitializeFromSource();
			ySpecCmbGeographicGroup.ItemSelected += YSpecCmbGeographicGroup_ItemSelected;
			SetControlsAccessibility();

			ytreeviewDocuments.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();
			ytreeviewDocuments.ItemsDataSource = Entity.ObservableDocumentTypes;

			lblWarehouses.LineWrapMode = Pango.WrapMode.Word;
			if(Entity.Id > 0)
				lblWarehouses.Text = Entity.GetWarehousesNames(UoW);
			else
				frmWarehoses.Visible = false;
			vboxDocuments.Visible = QSMain.User.Admin;
		}

		void YSpecCmbGeographicGroup_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			SetControlsAccessibility();
			if(Entity.ParentSubdivision != null || Entity.ChildSubdivisions.Any())
				foreach(var s in Entity.ChildSubdivisions) {
					s.GeographicGroup = Entity.GeographicGroup;
				}
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

		protected void OnButtonAddDocumentClicked(object sender, System.EventArgs e)
		{
			var docTypesJournal = new OrmReference(typeof(TypeOfEntity), UoW) {
				Mode = OrmReferenceMode.Select
			};
			docTypesJournal.ObjectSelected += DocTypesJournal_ObjectSelected;
			TabParent.AddSlaveTab(this, docTypesJournal);
		}

		protected void OnButtonDeleteDocumentClicked(object sender, System.EventArgs e)
		{
			if(ytreeviewDocuments.GetSelectedObject() is TypeOfEntity selectedObject)
				Entity.ObservableDocumentTypes.Remove(selectedObject);
		}

		void DocTypesJournal_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(e.Subject is TypeOfEntity selectedObject)
				Entity.ObservableDocumentTypes.Add(selectedObject);
		}

		void SetControlsAccessibility()
		{
			lblGeographicGroup.Visible = ySpecCmbGeographicGroup.Visible
				= Entity.ParentSubdivision != null && Entity.ChildSubdivisions.Any();
		}
	}
}