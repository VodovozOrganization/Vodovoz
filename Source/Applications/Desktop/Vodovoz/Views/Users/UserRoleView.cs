using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain.Permissions;
using Vodovoz.ViewModels.Users;

namespace Vodovoz.Views.Users
{
	public partial class UserRoleView : TabViewBase<UserRoleViewModel>
	{
		public UserRoleView(UserRoleViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveCommand);
			btnCancel.BindCommand(ViewModel.CancelCommand);
			btnAddToRoleAvailableDataBase.BindCommand(ViewModel.AddAvailableDatabaseCommand);
			btnRemoveFromRoleAvailableDatabase.BindCommand(ViewModel.RemoveAvailableDatabaseCommand);
			enumMenuBtnAddPrivilege.EnumItemClicked += OnEnumMenuBtnAddPrivilegeEnumItemClicked; 
			btnRemovePrivilege.BindCommand(ViewModel.RemovePrivilegeCommand);

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, e => e.CanEditRoleName, w => w.IsEditable)
				.InitializeFromSource();
			
			btnAddToRoleAvailableDataBase.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanAddAvailableDatabase, w => w.Sensitive)
				.InitializeFromSource();
			btnRemoveFromRoleAvailableDatabase.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanRemoveAvailableDatabase, w => w.Sensitive)
				.InitializeFromSource();

			enumMenuBtnAddPrivilege.ItemsEnum = typeof(PrivilegeType);
			btnRemovePrivilege.Binding
				.AddBinding(ViewModel, vm => vm.HasSelectedPrivileges, w => w.Sensitive)
				.InitializeFromSource();

			var copyPrivilegesBtn = new yButton();
			copyPrivilegesBtn.Label = "Скопировать привилегии";
			copyPrivilegesBtn.BindCommand(ViewModel.CopyPrivilegesCommand);
			copyPrivilegesBtn.Binding
				.AddBinding(ViewModel, vm => vm.HasSelectedPrivileges, w => w.Sensitive)
				.InitializeFromSource();
			copyPrivilegesBtn.Show();

			hboxPrivilegesBtns.Add(copyPrivilegesBtn);
			var addDocumentButtonBox = (Box.BoxChild)hboxPrivilegesBtns[copyPrivilegesBtn];
			addDocumentButtonBox.Expand = false;
			addDocumentButtonBox.Fill = false;
			
			ConfigureTreeViews();
			
			txtViewDescription.Binding
				.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text)
				.InitializeFromSource();
		}

		private void OnEnumMenuBtnAddPrivilegeEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			ViewModel.AddPrivilegeCommand.Execute(e.ItemEnum);
		}

		private void ConfigureTreeViews()
		{
			#region Доступные базы данных

			treeViewAllAvailableDataBases.ColumnsConfig = FluentColumnsConfig<AvailableDatabase>.Create()
				.AddColumn("База данных")
					.AddTextRenderer(x => x.Name)
				.Finish();

			treeViewAllAvailableDataBases.Binding
				.AddBinding(ViewModel, vm => vm.SelectedAvailableDatabase, w => w.SelectedRow)
				.InitializeFromSource();
			treeViewAllAvailableDataBases.RowActivated += (o, args) => ViewModel.AddAvailableDatabaseCommand.Execute();
			treeViewAllAvailableDataBases.ItemsDataSource = ViewModel.AllAvailableDatabases;
			
			treeViewAvailableDataBases.ColumnsConfig = FluentColumnsConfig<AvailableDatabase>.Create()
				.AddColumn("База данных")
					.AddTextRenderer(x => x.Name)
				.Finish();

			treeViewAvailableDataBases.Binding
				.AddBinding(ViewModel, vm => vm.SelectedEntityAvailableDatabase, w => w.SelectedRow)
				.InitializeFromSource();
			treeViewAvailableDataBases.RowActivated += (o, args) => ViewModel.RemoveAvailableDatabaseCommand.Execute();
			treeViewAvailableDataBases.ItemsDataSource = ViewModel.Entity.AvailableDatabases;

			#endregion

			#region Привилегии

			treeViewPrivileges.ColumnsConfig = FluentColumnsConfig<PrivilegeBase>.Create()
				.AddColumn("Тип привилегии")
					.AddTextRenderer(n => n.PrivilegeType.ToString())
				.AddColumn("Название")
					.AddComboRenderer(n => n.PrivilegeName)
					.Editing()
					.SetDisplayFunc(n => n.Name)
					.DynamicFillListFunc(n =>
						ViewModel.PrivilegesNames.Where(x => n.PrivilegeType == x.PrivilegeType).ToList())
				.AddColumn("База данных")
					.AddComboRenderer(x => x.DatabaseName)
					.DynamicFillListFunc(n =>
						ViewModel.Entity.AvailableDatabases.Where(x => !n.PrivilegeName.UnavailableDatabases.Contains(x))
							.Select(x => x.Name).ToList())
					.AddSetter((c, n) =>
					{
						if(n.PrivilegeType == PrivilegeType.GlobalPrivilege || n.PrivilegeType == PrivilegeType.SpecialPrivilege)
						{
							c.Editable = false;
						}
						else
						{
							c.Editable = true;
						}
					})
				.AddColumn("Имя таблицы")
					.AddTextRenderer(x => x.TableName)
					.AddSetter((c, n) => { c.Editable = n.PrivilegeType == PrivilegeType.TablePrivilege; })
				.AddColumn("")
				.Finish();

			treeViewPrivileges.ItemsDataSource = ViewModel.Entity.Privileges;
			treeViewPrivileges.Selection.Mode = SelectionMode.Multiple;
			treeViewPrivileges.Binding
				.AddBinding(ViewModel, vm => vm.SelectedPrivileges, w => w.SelectedRows)
				.InitializeFromSource();

			#endregion
		}
	}
}
