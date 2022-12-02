using System;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Domain;
using QS.Views.GtkUI;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewWidgets.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PresetPermissionsView : WidgetViewBase<PresetPermissionsViewModelBase>, IUserPermissionTab
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorBlue = new Color(0x00, 0x18, 0xf9);
		private static readonly Color _colorDarkGrey = new Color(0x80, 0x80, 0x80);
		
		public string Title => "Предустановленные права";

		public PresetPermissionsView(PresetPermissionsViewModelBase viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		public PresetPermissionsView()
		{
			Build();
		}

		public void ConfigureDlg(IUnitOfWork uow, UserBase user)
		{
			ViewModel = new PresetUserPermissionsViewModel(uow, new PermissionRepository(), uow.GetById<User>(user.Id));
			Configure();
		}

		public void Save()
		{
			ViewModel.SaveCommand.Execute();
		}

		protected void Configure()
		{
			ytreeviewAvailablePermissions.ColumnsConfig = ColumnsConfigFactory.Create<PresetUserPermissionSource>()
						.AddColumn("Право").AddTextRenderer(x => x.DisplayName)
						.Finish();
			ytreeviewAvailablePermissions.ItemsDataSource = ViewModel.ObservablePermissionsSourceList;
			ytreeviewAvailablePermissions.Binding
				.AddBinding(ViewModel, vm => vm.SelectedPresetUserPermissionSource, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeviewSelectedPermissions.ColumnsConfig = ColumnsConfigFactory.Create<HierarchicalPresetPermissionBase>()
				.AddColumn("Право").AddTextRenderer(x => x.DisplayName)
				.AddColumn("Значение").AddToggleRenderer(x => x.Value)
					.AddSetter((c, n) => c.Activatable = !n.IsLostPermission)
				.RowCells()
					.AddSetter((CellRendererText cell, HierarchicalPresetPermissionBase node) =>
					{
						if(node.IsLostPermission)
						{
							cell.ForegroundGdk = _colorDarkGrey;
						}
						else
						{
							cell.ForegroundGdk = node.Id > 0 ? _colorBlack : _colorBlue;
						}
					})
				.Finish();
			ytreeviewSelectedPermissions.ItemsDataSource = ViewModel.ObservablePermissionsList;
			ytreeviewSelectedPermissions.Binding
				.AddBinding(ViewModel, vm => vm.SelectedHierarchicalPresetPermissionBase, w => w.SelectedRow)
				.InitializeFromSource();
			
			searchPresetPermissions.TextChanged += SearchPresetPermissionsOnTextChanged;
		}
		
		protected void OnYtreeviewAvailablePermissionsRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.AddPermissionCommand.Execute();
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddPermissionCommand.Execute();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			ViewModel.RemovePermissionCommand.Execute();
		}

		private void SearchPresetPermissionsOnTextChanged(object sender, EventArgs e)
		{ 
			ViewModel.StartSearch(searchPresetPermissions.Text);
		}
	}
}
