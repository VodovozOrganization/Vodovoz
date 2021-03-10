using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class EmployeePostView : TabViewBase<EmployeePostViewModel>
    {
        public EmployeePostView(EmployeePostViewModel viewModel) : base(viewModel) => this.Build();
    }
}
