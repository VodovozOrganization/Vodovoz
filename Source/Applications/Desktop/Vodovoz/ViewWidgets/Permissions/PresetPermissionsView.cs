using Autofac;
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
using System;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewWidgets.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PresetPermissionsView : WidgetViewBase<PresetPermissionsViewModelBase>, IUserPermissionTab
	{
		private static readonly Color _colorPrimaryText = GdkColors.PrimaryText;
		private static readonly Color _colorBlue = GdkColors.InfoText;
		private static readonly Color _colorInsensitiveText = GdkColors.InsensitiveText;
		private ISubdivisionRepository _subdivisionRepository;
		private IPermissionRepository _permissionsRepository;
		private Menu _availablePresetPermissionsPopupMenu;
		private Menu _userPresetPermissionsPopupMenu;
		
		public string Title => "Предустановленные права";

		public PresetPermissionsView(PresetPermissionsViewModelBase viewModel) : base(viewModel)
		{
			ResolveDependencies();
			Build();
			Configure();
		}

		public PresetPermissionsView()
		{
			ResolveDependencies();
			Build();
		}

		[Obsolete("Должен быть удален при разрешении проблем с контейнером")]
		private void ResolveDependencies()
		{
			_subdivisionRepository = ScopeProvider.Scope.Resolve<ISubdivisionRepository>();
			_permissionsRepository = ScopeProvider.Scope.Resolve<IPermissionRepository>();
		}

		public void ConfigureDlg(IUnitOfWork uow, UserBase user)
		{
			ViewModel =
				new PresetUserPermissionsViewModel(
					uow, _permissionsRepository, uow.GetById<User>(user.Id),
					new UsersPresetPermissionValuesGetter(_permissionsRepository, _subdivisionRepository),
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
							cell.ForegroundGdk = _colorInsensitiveText;
						}
						else
						{
							cell.ForegroundGdk = node.Id > 0 ? _colorPrimaryText : _colorBlue;
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

		public override void Destroy()
		{
			_subdivisionRepository = null;
			_permissionsRepository = null;
			base.Destroy();
		}
	}
}
