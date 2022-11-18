using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EmployeePostView : TabViewBase<EmployeePostViewModel>
    {
        public EmployeePostView(EmployeePostViewModel viewModel) : base(viewModel)
        {
            this.Build();
            ConfigureView();
        }

        private void ConfigureView()
        {
            yentryEmployeePost.Binding.AddBinding(ViewModel, e => e.PostName, w => w.Text).InitializeFromSource();
            yentryEmployeePost.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

            buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
            buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };
        }
    }
}
