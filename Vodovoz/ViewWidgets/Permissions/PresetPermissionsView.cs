using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Domain;
using QS.Views.GtkUI;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewWidgets.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PresetPermissionsView : WidgetViewBase<PresetPermissionsViewModelBase>, IUserPermissionTab
	{
		public string Title => "Предустановленные права";

		public PresetPermissionsView(PresetPermissionsViewModelBase viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		public PresetPermissionsView()
		{
			this.Build();
			searchPresetPermissions.TextChanged += SearchPresetPermissionsOnTextChanged;
		}

		private void SearchPresetPermissionsOnTextChanged(object sender, EventArgs e)
		{ 
			ytreeviewAvailablePermissions.ItemsDataSource = null;
			ytreeviewSelectedPermissions.ItemsDataSource = null;
			ViewModel.StartSearch(searchPresetPermissions.Text);
			ytreeviewAvailablePermissions.ItemsDataSource = ViewModel.ObservablePermissionsSourceList;
			ytreeviewSelectedPermissions.ItemsDataSource = ViewModel.ObservablePermissionsList;
		}

		protected void Configure()
		{
			ytreeviewAvailablePermissions.ColumnsConfig = ColumnsConfigFactory.Create<PresetUserPermissionSource>()
						.AddColumn("Право").AddTextRenderer(x => x.DisplayName)
						.Finish();
			ytreeviewAvailablePermissions.ItemsDataSource = ViewModel.ObservablePermissionsSourceList;

			ytreeviewSelectedPermissions.ColumnsConfig = ColumnsConfigFactory.Create<HierarchicalPresetPermissionBase>()
				.AddColumn("Право").AddTextRenderer(x => x.DisplayName)
				.AddColumn("Значение").AddToggleRenderer(x => x.Value)
				.RowCells().AddSetter((CellRenderer cell, HierarchicalPresetPermissionBase node) => cell.Sensitive = !node.IsLostPermission)
				.Finish();
			ytreeviewSelectedPermissions.ItemsDataSource = ViewModel.ObservablePermissionsList;
		}

		private void AddPermission()
		{
			if(ytreeviewAvailablePermissions.GetSelectedObject() is PresetUserPermissionSource selected)
				ViewModel.AddPermissionCommand.Execute(selected);
		}

		private void DeletePermisission()
		{
			if(ytreeviewSelectedPermissions.GetSelectedObject() is HierarchicalPresetPermissionBase selected)
				ViewModel.RemovePermissionCommand.Execute(selected);
		}

		protected void OnYtreeviewAvailablePermissionsRowActivated(object o, RowActivatedArgs args)
		{
			AddPermission();
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			AddPermission();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DeletePermisission();
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
	}
}
