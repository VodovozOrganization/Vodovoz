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

            ConfigureDlg();
        }

        void ConfigureDlg()
        {
            yentryUsername.Binding.AddBinding(ViewModel.Entity, e => e.Username, w => w.Text).InitializeFromSource();
            yentryDomainame.Binding.AddBinding(ViewModel.Entity, e => e.Domain, w => w.Text).InitializeFromSource();
            yentrySID.Binding.AddBinding(ViewModel.Entity, e => e.SID, w => w.Text).InitializeFromSource();
            ycheckIsActive.Binding.AddBinding(ViewModel.Entity, e => e.IsActive, w => w.Active).InitializeFromSource();

            buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, e) => ViewModel.Close(false, QS.Navigation.CloseSource.Cancel);
        }

        protected void OnButtonAddUserClicked(object sender, System.EventArgs e)
        {
        }

        protected void OnButtonDeleteUserClicked(object sender, System.EventArgs e)
        {
        }
    }
}
