using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Permissions;
using QS.Views.GtkUI;
using Vodovoz.Domain.Permissions;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewWidgets.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PresetPermissionsView : WidgetViewBase<PresetPermissionsViewModelBase>
	{
		public PresetPermissionsView(PresetPermissionsViewModelBase viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
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
	}
}
