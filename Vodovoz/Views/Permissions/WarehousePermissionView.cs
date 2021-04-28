using System.Linq;
using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.Views.Permissions
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class WarehousePermissionView : WidgetViewBase<WarehousePermissionsViewModel>
    {
        public string Title => "Права на склад";
        private NullableCheckButton[,] checkButtons;

        public WarehousePermissionView(WarehousePermissionsViewModel viewModel) : base(viewModel)
        {
            this.Build();
            ConfigureView();
        }

        void ConfigureView()
        {
            tablePermissionMatrix.NRows = 2 + (uint) ViewModel.AllPermissionTypes.Count();
            tablePermissionMatrix.NColumns = 2 + (uint) ViewModel.AllWarehouses.Count();

            checkButtons = new NullableCheckButton[tablePermissionMatrix.NRows, tablePermissionMatrix.NColumns];
            
            var labelAllWarehouse = new yLabel();
            labelAllWarehouse.Binding.AddBinding(ViewModel.AllPermissions, vm => vm.Title, x => x.Text).InitializeFromSource();
            labelAllWarehouse.Angle = 90;
            labelAllWarehouse.SetAlignment(0.5f, 1);
            InsertCheckBoxAll();
            tablePermissionMatrix.Attach(labelAllWarehouse, 1, 2, 0, 1);
            
            uint col = 2;
            foreach (var warehouseAllNode in ViewModel.AllWarehouses)
            {
                var labelColumn = new yLabel();
                var checkButton = new NullableCheckButton();
                uint count = 0;
                
                labelColumn.Angle = 90;
                labelColumn.SetAlignment(0.5f,1);
                labelColumn.Binding.AddBinding(warehouseAllNode, 
                    wm => wm.Title, x => x.Text).InitializeFromSource();
                
                checkButton.SetAlignment(0, 0);
                checkButton.Binding.AddBinding(warehouseAllNode, 
                    vm=> vm.PermissionValue,x=>x.Active).InitializeFromSource();
                checkButton.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
                
                foreach (var warehousePermissionNode in warehouseAllNode.SubNodeViewModel)
                {
                    var subCheckButton = new NullableCheckButton();
                    subCheckButton.SetAlignment(0, 0);
                    subCheckButton.Binding.AddBinding(warehousePermissionNode, 
                        vm => vm.PermissionValue, x => x.Active).InitializeFromSource();
                    subCheckButton.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

                    checkButtons[count, col - 2] = subCheckButton;
                    tablePermissionMatrix.Attach(subCheckButton, col, col + 2, count + 2, count + 3);
                    count++;
                }
                
                tablePermissionMatrix.Attach(checkButton, col, col + 2, 1, 2);
                tablePermissionMatrix.Attach(labelColumn, col, col + 1, 0, 1);
                col++;
            }

            var labelAll = new yLabel();
            labelAll.Binding.AddBinding(ViewModel.AllPermissions, vm => vm.Title, x => x.Text).InitializeFromSource();
            labelAll.SetAlignment(1, 0.5f);
            
            tablePermissionMatrix.Attach(labelAll, 0, 1, 1, 2);
            
            
            uint row = 2;
            foreach (var permissionTypeAllNode in ViewModel.AllPermissionTypes)
            {
                var labelColumn = new yLabel();
                var checkButton = new NullableCheckButton();
                uint count = 0;
                labelColumn.SetAlignment(1,0.5f);
                labelColumn.Binding.AddBinding(permissionTypeAllNode, wm => wm.Title, x => x.Text).InitializeFromSource();

                foreach (var permission in permissionTypeAllNode.SubNodeViewModel)
                {
                    checkButtons[row - 2, count].Binding.AddBinding(permission, 
                        vm => vm.PermissionValue, x=>x.Active).InitializeFromSource();
                    checkButtons[row - 2, count].Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
                    count++;
                }
                
                checkButton.SetAlignment(0, 0);
                checkButton.Binding.AddBinding(permissionTypeAllNode, vm=> vm.PermissionValue,
                    x=>x.Active).InitializeFromSource();
                checkButton.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

                tablePermissionMatrix.Attach(checkButton, 1, 2, row, row + 2);
                tablePermissionMatrix.Attach(labelColumn, 0,  1, row, row + 1);
                row++;
            }
        }

        void InsertCheckBoxAll()
        {
            var checkButton = new NullableCheckButton();
            checkButton.SetAlignment(0, 0);
            checkButton.Binding.AddBinding(ViewModel.AllPermissions, vm=> vm.PermissionValue,
                                                          x=>x.Active).InitializeFromSource();
            checkButton.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

            tablePermissionMatrix.Attach(checkButton, 1, 2, 1, 2);
        }
    }
}
