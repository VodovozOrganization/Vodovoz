using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.ViewModels.PermissionNode;

namespace Vodovoz.Views.Permissions
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class WarehousePermissionView : WidgetViewBase<WarehousePermissionsViewModel>
    {
        private NullableCheckButton[,] _checkButtons;
		private IList<yLabel> _labels = new List<yLabel>();

        public WarehousePermissionView(WarehousePermissionsViewModel viewModel) : base(viewModel)
        {
            Build();
            ConfigureView();
        }

		private void ConfigureView()
		{
			uint col = 2;
			uint row = 2;

			tablePermissionMatrix.NRows = row + (uint)ViewModel.AllPermissionTypes.Count;
			tablePermissionMatrix.NColumns = col + (uint)ViewModel.AllWarehouses.Count;

			_checkButtons = new NullableCheckButton[tablePermissionMatrix.NRows, tablePermissionMatrix.NColumns];

			//Лэйбл 'Все' и кнопка, проставляющая все права на все склады
			_labels.Add(InsertLabel(ViewModel.AllPermissions, col - 1, row - 2, 0.5f, 1));
			InsertNullableCheckBtn(ViewModel.AllPermissions, col - 1, row - 1);

			foreach(var warehouseAllNode in ViewModel.AllWarehouses)
			{
				uint subRow = 0;

				//Верхние подписи названия складов и кнопки, включающие все права для конкретного склада
				_labels.Add(InsertLabel(warehouseAllNode, col, row - 2, 0.5f, 1));
				InsertNullableCheckBtn(warehouseAllNode, col, row - 1);

				//Внутренние, отвечающие за конкретное право конкретного склада
				foreach(var warehousePermissionNode in warehouseAllNode.SubNodeViewModel)
				{
					_checkButtons[subRow, col - 2] = InsertNullableCheckBtn(warehousePermissionNode, col, subRow + 2);
					subRow++;
				}

				col++;
			}

			_labels.Add(InsertLabel(ViewModel.AllPermissions, 0, 1, 1, 0.5f, 0));

			foreach(var permissionTypeAllNode in ViewModel.AllPermissionTypes)
			{
				uint subCol = 0;

				//Лэйбл с названием права для кнопки по его установке для всех складов
				_labels.Add(InsertLabel(permissionTypeAllNode, 0, row, 1, 0.5f, 0));

				foreach(var permission in permissionTypeAllNode.SubNodeViewModel)
				{
					_checkButtons[row - 2, subCol].Binding
						.AddBinding(permission, vm => vm.PermissionValue, x => x.Active)
						.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
						.InitializeFromSource();
					subCol++;
				}

				//Кнопка, устанавливающая конкретное право для всех складов
				InsertNullableCheckBtn(permissionTypeAllNode, 1, row);
				row++;
			}
		}

		private yLabel InsertLabel<T>(T permissionNodeVM, uint column, uint row, float xAlign, float yAlign, double angle = 90)
			where T : PropertyChangedBase, IPermissionNodeViewModel
		{
			var label = new yLabel();

			label.Angle = angle;
			label.SetAlignment(xAlign, yAlign);
			label.Binding
				.AddBinding(permissionNodeVM, wm => wm.Title, x => x.Text)
				.InitializeFromSource();

			tablePermissionMatrix.Attach(label, column, column + 1, row, row + 1);

			return label;
		}
		
		private NullableCheckButton InsertNullableCheckBtn<T>(T permissionNodeVM, uint column, uint row)
			where T : PropertyChangedBase, IPermissionNodeViewModel
		{
			var checkButton = new NullableCheckButton();

			checkButton.SetAlignment(0, 0);
			checkButton.Binding
				.AddBinding(permissionNodeVM, vm => vm.PermissionValue, x => x.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			tablePermissionMatrix.Attach(checkButton, column, column + 1, row, row + 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			return checkButton;
		}

		public override void Destroy()
		{
			foreach(var btn in _checkButtons)
			{
				btn?.Binding.CleanSources();
			}
			foreach(var label in _labels)
			{
				label?.Binding.CleanSources();
			}
			base.Destroy();
		}
	}
}
