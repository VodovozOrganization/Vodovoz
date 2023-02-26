﻿using System;
using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Journal.GtkUI;
using QS.Permissions;
using QS.Project.Domain;
using QS.Views.GtkUI;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewWidgets.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PresetPermissionsView : WidgetViewBase<PresetPermissionsViewModelBase>, IUserPermissionTab
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorBlue = new Color(0x00, 0x18, 0xf9);
		private static readonly Color _colorDarkGrey = new Color(0x80, 0x80, 0x80);

		private Menu _availablePresetPermissionsPopupMenu;
		private Menu _userPresetPermissionsPopupMenu;
		
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
			var permissionRepository = new PermissionRepository();
			ViewModel =
				new PresetUserPermissionsViewModel(
					uow, permissionRepository, uow.GetById<User>(user.Id),
					new UsersPresetPermissionValuesGetter(permissionRepository, new SubdivisionRepository(new ParametersProvider())),
					new UserPermissionsExporter(new FileDialogService(), new GtkMessageDialogsInteractive()));
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

			CreatePopupMenu();
			
			ytreeviewAvailablePermissions.ButtonReleaseEvent += AvailablePresetPermissionsButtonReleaseEvent;
			ytreeviewSelectedPermissions.ButtonReleaseEvent += UserPresetPermissionsButtonReleaseEvent;
			searchPresetPermissions.TextChanged += SearchPresetPermissionsOnTextChanged;
		}

		private void CreatePopupMenu()
		{
			_availablePresetPermissionsPopupMenu = new Menu();
			var availablePresetPermissionItem = new MenuItem("Выгрузить в Эксель");
			availablePresetPermissionItem.Activated +=
				(sender, eventArgs) => ViewModel.GetUsersWithActiveSelectedAvailablePermissionCommand.Execute();
			availablePresetPermissionItem.Visible = true;

			_availablePresetPermissionsPopupMenu.Add(availablePresetPermissionItem);
			_availablePresetPermissionsPopupMenu.Show();
			
			_userPresetPermissionsPopupMenu = new Menu();
			var userPresetPermissionItem = new MenuItem("Выгрузить в Эксель");
			userPresetPermissionItem.Activated +=
				(sender, eventArgs) => ViewModel.GetUsersWithActiveSelectedCurrentPermissionCommand.Execute();
			userPresetPermissionItem.Visible = true;

			_userPresetPermissionsPopupMenu.Add(userPresetPermissionItem);
			_userPresetPermissionsPopupMenu.Show();
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
		
		void AvailablePresetPermissionsButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}
			
			_availablePresetPermissionsPopupMenu.Popup();
		}
		
		void UserPresetPermissionsButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}
			
			_userPresetPermissionsPopupMenu.Popup();
		}
	}
}
