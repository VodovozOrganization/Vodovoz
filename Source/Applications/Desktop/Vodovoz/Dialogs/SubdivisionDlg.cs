using System.Linq;
using Gamma.Binding;
using Gamma.GtkWidgets;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Validation;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Sale;
using Vodovoz.Representations;
using System;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewWidgets.Permissions;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using QS.Project.Services;
using Vodovoz.TempAdapters;
using QS.Project.Journal;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.Views.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz
{
	[Obsolete("Использовать Vodovoz.Views.Organization.SubdivisionView")]
	public partial class SubdivisionDlg : QS.Dialog.Gtk.EntityDialogBase<Subdivision>
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		private SubdivisionsVM _subdivisionsVm;
		private PresetSubdivisionPermissionsViewModel _presetPermissionVm;
		private WarehousePermissionsViewModel _warehousePermissionsViewModel;

		[Obsolete("Использовать Vodovoz.Views.Organization.SubdivisionView")]
		public SubdivisionDlg()
		{
			Build();
			TabName = "Новое подразделение";
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Subdivision>();
			ConfigureDlg();
		}

		[Obsolete("Использовать Vodovoz.Views.Organization.SubdivisionView")]
		public SubdivisionDlg(int id)
		{
			Build();
			_logger.Info("Загрузка информации о подразделении...");
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Subdivision>(id);
			ConfigureDlg();
		}

		[Obsolete("Использовать Vodovoz.Views.Organization.SubdivisionView")]
		public SubdivisionDlg(Subdivision sub) : this(sub.Id) { }

		private void ConfigureDlg()
		{
			var permissionListViewModel = new PermissionListViewModel(PermissionExtensionSingletonStore.GetInstance());
			var entitySubdivisionPermissionModel = new EntitySubdivisionPermissionViewModel(
				UoW, Entity, permissionListViewModel, new PermissionRepository());
			
			subdivisionentitypermissionwidget.ConfigureDlg(entitySubdivisionPermissionModel);
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryShortName.Binding.AddBinding(Entity, e => e.ShortName, w => w.Text).InitializeFromSource();
			yentryrefParentSubdivision.SubjectType = typeof(Subdivision);
			yentryrefParentSubdivision.Binding.AddBinding(Entity, e => e.ParentSubdivision, w => w.Subject).InitializeFromSource();

			var employeeFactory = new EmployeeJournalFactory();
			evmeChief.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeChief.Binding.AddBinding(Entity, e => e.Chief, w => w.Subject).InitializeFromSource();

			yenumcomboType.ItemsEnum = typeof(SubdivisionType);
			yenumcomboType.Binding.AddBinding(Entity, e => e.SubdivisionType, w => w.SelectedItem).InitializeFromSource();
			yenumcomboType.Sensitive = false;

			_subdivisionsVm = new SubdivisionsVM(UoW, Entity);
			repTreeChildSubdivisions.RepresentationModel = _subdivisionsVm;
			repTreeChildSubdivisions.YTreeModel = new RecursiveTreeModel<SubdivisionVMNode>(_subdivisionsVm.Result, x => x.Parent, x => x.Children);

			ySpecCmbGeographicGroup.ItemsList = UoW.Session.QueryOver<GeoGroup>().List();
			ySpecCmbGeographicGroup.Binding.AddBinding(Entity, e => e.GeographicGroup, w => w.SelectedItem).InitializeFromSource();
			ySpecCmbGeographicGroup.ItemSelected += YSpecCmbGeographicGroup_ItemSelected;
			SetControlsAccessibility();

			ytreeviewDocuments.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();
			ytreeviewDocuments.ItemsDataSource = Entity.ObservableDocumentTypes;

			var admin = QSMain.User.Admin;
			
			lblWarehouses.LineWrapMode = Pango.WrapMode.Word;
			lblWarehouses.LineWrap = true;
			if(Entity.Id > 0)
			{
				lblWarehouses.Text = Entity.GetWarehousesNames(UoW, _subdivisionRepository);
			}
			else
			{
				frmWarehoses.Visible = false;
			}

			vboxDocuments.Visible = admin;

			if(admin)
			{
				CreatePresetSubdivisionPermissionsView();
				CreateWarehousePermissionView();
			}
			else
			{
				vboxPresetPermissions.Visible = false;
				vboxSubdivision.Visible = false;
			}

			Entity.ObservableDocumentTypes.ListContentChanged += UpdateChanges;
			subdivisionentitypermissionwidget.ViewModel.ObservableTypeOfEntitiesList.ListContentChanged += UpdateChanges;

			entryDefaultSalesPlan.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<SalesPlanJournalViewModel>(typeof(SalesPlan),
					() => new SalesPlanJournalViewModel(
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						new NomenclatureJournalFactory())
					{
						SelectionMode = JournalSelectionMode.Single
					}
			));
			entryDefaultSalesPlan.Binding.AddBinding(Entity, s => s.DefaultSalesPlan, w => w.Subject).InitializeFromSource();
			entryDefaultSalesPlan.CanEditReference = false;

			Entity.PropertyChanged += UpdateChanges;
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
			if(valid.RunDlgIfNotValid())
				return false;

			UoWGeneric.Save();
			subdivisionentitypermissionwidget?.ViewModel.SavePermissions();
			_presetPermissionVm?.SaveCommand.Execute();
			_warehousePermissionsViewModel?.SaveWarehousePermissions();
			UoW.Commit();
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
			{
				Entity.ObservableDocumentTypes.Remove(selectedObject);
			}
		}

		void DocTypesJournal_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(e.Subject is TypeOfEntity selectedObject)
			{
				Entity.ObservableDocumentTypes.Add(selectedObject);
			}
		}

		void SetControlsAccessibility()
		{
			lblGeographicGroup.Visible = ySpecCmbGeographicGroup.Visible
				= Entity.ParentSubdivision != null && Entity.ChildSubdivisions.Any();
		}

		private void CreatePresetSubdivisionPermissionsView()
		{
			var permissionRepository = new PermissionRepository();
			_presetPermissionVm =
				new PresetSubdivisionPermissionsViewModel(
					UoW, permissionRepository, Entity, new UsersPresetPermissionValuesGetter(permissionRepository, _subdivisionRepository),
					new UserPermissionsExporter(new FileDialogService(), new GtkMessageDialogsInteractive()));
			vboxPresetPermissions.Add(new PresetPermissionsView(_presetPermissionVm));
			vboxPresetPermissions.ShowAll();
			
			_presetPermissionVm.ObservablePermissionsList.ListContentChanged += UpdateChanges;
		}

		private void CreateWarehousePermissionView()
		{
			var warehousePermissionModel = new SubdivisionWarehousePermissionModel(UoW, Entity);
			_warehousePermissionsViewModel = new WarehousePermissionsViewModel(UoW, warehousePermissionModel)
			{
				CanEdit = permissionResult.CanUpdate
			};
			vboxSubdivision.Add(new WarehousePermissionView(_warehousePermissionsViewModel));
			vboxSubdivision.ShowAll();

			foreach(var warehousePermissionNode in _warehousePermissionsViewModel.AllWarehouses)
			{
				warehousePermissionNode.SubNodeViewModel.ListContentChanged += UpdateChanges;
			}
		}

		private void UpdateChanges(object sender, EventArgs args) => HasChanges = true;

		public override void Destroy ()
		{
			Entity.PropertyChanged -= UpdateChanges;
			Entity.ObservableDocumentTypes.ListContentChanged -= UpdateChanges;
			subdivisionentitypermissionwidget.ViewModel.ObservableTypeOfEntitiesList.ListContentChanged -= UpdateChanges;
			
			if(_presetPermissionVm != null)
			{
				_presetPermissionVm.ObservablePermissionsList.ListContentChanged -= UpdateChanges;
			}

			if(_warehousePermissionsViewModel != null)
			{
				foreach(var warehousePermissionNode in _warehousePermissionsViewModel.AllWarehouses)
				{
					warehousePermissionNode.SubNodeViewModel.ListContentChanged -= UpdateChanges;
				}
			}
			base.Destroy ();
		}
	}
}
