using Gamma.Binding;
using Gamma.GtkWidgets;
using Gtk;
using QS.Project.Domain;
using QS.Views.GtkUI;
using QSOrmProject;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Views.Organization
{
	[ToolboxItem(true)]
	public partial class SubdivisionView : TabViewBase<SubdivisionViewModel>
	{
		private ViewModelWidgetResolver _widgetResolver = ViewModelWidgetResolver.Instance;
		private Widget _warehousePermissionsView;

		public SubdivisionView(
			SubdivisionViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			subdivisionentitypermissionwidget.ConfigureDlg(ViewModel.EntitySubdivisionPermissionViewModel);
			subdivisionentitypermissionwidget.Sensitive = ViewModel.CanEdit;

			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			yentryShortName.Binding.AddBinding(ViewModel.Entity, e => e.ShortName, w => w.Text).InitializeFromSource();
			yentryShortName.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entrySubdivision.ViewModel = ViewModel.ParentSubdivisionViewModel;
			entrySubdivision.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryFinancialResponsibilityCenter.ViewModel = ViewModel.FinancialResponsibilityCenterViewModel;
			entryFinancialResponsibilityCenter.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryChief.ViewModel = ViewModel.ChiefViewModel;
			entryChief.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			ytreeviewChildSubdivisions.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			ytreeviewChildSubdivisions.CreateFluentColumnsConfig<SubdivisionJournalNode>()
				.AddColumn("Название").AddTextRenderer(node => node.Name).AddSetter((cell, node) =>
				{
					var color = GdkColors.PrimaryText;
					if(node.IsArchive)
					{
						color = GdkColors.InsensitiveText;
					}

					cell.ForegroundGdk = color;
				})
				.AddColumn("Руководитель").AddTextRenderer(node => node.ChiefName).AddSetter((cell, node) =>
				{
					var color = GdkColors.PrimaryText;
					if(node.IsArchive)
					{
						color = GdkColors.InsensitiveText;
					}

					cell.ForegroundGdk = color;
				})
				.AddColumn("Код").AddNumericRenderer(node => node.Id).AddSetter((cell, node) =>
				{
					var color = GdkColors.PrimaryText;
					if(node.IsArchive)
					{
						color = GdkColors.InsensitiveText;
					}

					cell.ForegroundGdk = color;
				})
				.Finish();

			ViewModel.SubdivisionsJournalViewModel.DataLoader.ItemsListUpdated += ChildSubdivisionsReloaded;

			speciallistcomboboxGeoGrpoup.ItemsList = ViewModel.UoW.Session.QueryOver<GeoGroup>().List();
			speciallistcomboboxGeoGrpoup.Binding.AddBinding(ViewModel, e => e.GeographicGroup, w => w.SelectedItem).InitializeFromSource();
			speciallistcomboboxGeoGrpoup.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			speciallistcomboboxGeoGrpoup.Binding.AddBinding(ViewModel, vm => vm.GeographicGroupVisible, w => w.Visible).InitializeFromSource();
			lblGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeographicGroupVisible, w => w.Visible).InitializeFromSource();

			yenumcomboType.ItemsEnum = typeof(SubdivisionType);
			yenumcomboType.Binding.AddBinding(ViewModel.Entity, e => e.SubdivisionType, w => w.SelectedItem).InitializeFromSource();
			yenumcomboType.Sensitive = false;

			ytreeviewDocuments.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();
			ytreeviewDocuments.Binding.AddBinding(ViewModel.Entity, e => e.ObservableDocumentTypes, w => w.ItemsDataSource).InitializeFromSource();

			lblWarehouses.LineWrapMode = Pango.WrapMode.Word;
			lblWarehouses.LineWrap = true;

			if(ViewModel.Entity.Id > 0)
			{
				lblWarehouses.Text = ViewModel.Entity.GetWarehousesNames(ViewModel.UoW, ViewModel.SubdivisionRepository);
			}
			else
			{
				frmWarehoses.Visible = false;
			}

			vboxDocuments.Visible = ViewModel.CurrentUser.IsAdmin;

			buttonAddDocument.Clicked += ButtonAddDocument_Clicked;
			buttonAddDocument.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			buttonDeleteDocument.Clicked += (sender, e) => ViewModel.DeleteDocumentCommand.Execute(ytreeviewDocuments.GetSelectedObject() as TypeOfEntity);
			ytreeviewDocuments.Selection.Changed += (sender, e) => buttonDeleteDocument.Sensitive = ViewModel.DeleteDocumentCommand.CanExecute(ytreeviewDocuments.GetSelectedObject() as TypeOfEntity);

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			permissionsPresetContainerView.Binding
				.AddBinding(ViewModel, vm => vm.PresetSubdivisionPermissionVM, w => w.WidgetViewModel)
				.InitializeFromSource();
			permissionsPresetContainerView.Visible = ViewModel.CurrentUser.IsAdmin;

			entryDefaultSalesPlan.ViewModel = ViewModel.DefaultSalesPlanViewModel;
			entryDefaultSalesPlan.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryDefaultSalesPlan.ViewModel.IsEditable = false;

			ycheckArchieve.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanArchive, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAddSubdiviionPermissions.BindCommand(ViewModel.AddSubdivisionPermissionsCommand);
			ybuttonReplaceSubdivisionPermissions.BindCommand(ViewModel.ReplaceSubdivisionPermissionsCommand);

			ybuttonAddSubdiviionPermissions.Binding
				.AddBinding(ViewModel, vm => vm.CanAddOrReplacePermissions, w => w.Visible)
				.InitializeFromSource();

			ybuttonReplaceSubdivisionPermissions.Binding
				.AddBinding(ViewModel, vm => vm.CanAddOrReplacePermissions, w => w.Visible)
				.InitializeFromSource();

			CreateWarehousePermissionsView();
			ViewModel.UpdateWarehousePermissionsAction += OnUpdateWarehousePermissionsViewAction;
		}

		private void ChildSubdivisionsReloaded(object sender, EventArgs e)
		{
			ytreeviewChildSubdivisions.YTreeModel = new RecursiveTreeModel<SubdivisionJournalNode>(ViewModel.SubdivisionsJournalViewModel.Items.Cast<SubdivisionJournalNode>(), ViewModel.SubdivisionsJournalViewModel.RecuresiveConfig);
		}

		void ButtonAddDocument_Clicked(object sender, EventArgs e)
		{
			var docTypesJournal = new OrmReference(typeof(TypeOfEntity), ViewModel.UoW)
			{
				Mode = OrmReferenceMode.Select
			};
			docTypesJournal.ObjectSelected += DocTypesJournal_ObjectSelected;
			ViewModel.TabParent.AddSlaveTab(ViewModel, docTypesJournal);
		}

		private void DocTypesJournal_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			ViewModel.AddDocumentTypeCommand.Execute(e.Subject as TypeOfEntity);
		}

		private void OnUpdateWarehousePermissionsViewAction()
		{
			yvboxWarehousesPermissionContainer.Remove(_warehousePermissionsView);
			_warehousePermissionsView.Destroy();
			CreateWarehousePermissionsView();
		}

		private void CreateWarehousePermissionsView()
		{
			_warehousePermissionsView = _widgetResolver.Resolve(ViewModel.WarehousePermissionsVM);
			yvboxWarehousesPermissionContainer.Add(_warehousePermissionsView);
			_warehousePermissionsView.ShowAll();

			_warehousePermissionsView.Visible = ViewModel.CurrentUser.IsAdmin;
		}

		public override void Destroy()
		{
			ViewModel.UpdateWarehousePermissionsAction -= OnUpdateWarehousePermissionsViewAction;

			base.Destroy();
		}
	}
}
