using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Security;

namespace Vodovoz.Views.Security
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class RegisteredRMView : TabViewBase<RegisteredRMViewModel>
    {
        public RegisteredRMView(RegisteredRMViewModel viewModel) : base(viewModel)
        {
            this.Build();
        }
    }
}
