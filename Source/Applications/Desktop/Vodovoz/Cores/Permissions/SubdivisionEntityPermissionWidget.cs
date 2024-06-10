using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Project.Domain;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.Core.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubdivisionEntityPermissionWidget : Bin
	{
		public SubdivisionEntityPermissionWidget()
		{
			Build();
			Sensitive = false;
		}
		
		public void ConfigureDlg(EntitySubdivisionPermissionViewModel viewViewModel)
		{
			ViewModel = viewViewModel;
			permissionlistview.ViewModel = ViewModel.PermissionListViewModel;
			ConfigureDlg();
		}
		
		public EntitySubdivisionPermissionViewModel ViewModel { get; set; }

		private void ConfigureDlg()
		{
			var extensions = ViewModel.ExtensionStore.PermissionExtensions;

			extensions.OrderBy(x => x.PermissionId);

			foreach(SubdivisionPermissionNode item in ViewModel.PermissionListViewModel.PermissionsList)
				item.EntityPermissionExtended.OrderBy(x => x.PermissionId);

			ytreeviewEntitiesList.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntitiesList.ItemsDataSource = ViewModel.ObservableTypeOfEntitiesList;

			Sensitive = true;
		}

		private void AddPermission()
		{
			var selected = ytreeviewEntitiesList.GetSelectedObject() as TypeOfEntity;
			ViewModel.AddPermission(selected);
		}

		private void OnButtonAddClicked(object sender, EventArgs e)
		{
			AddPermission();
		}

		protected void OnYtreeviewEntitiesListRowActivated(object o, RowActivatedArgs args)
		{
			AddPermission();
		}
	}
}
