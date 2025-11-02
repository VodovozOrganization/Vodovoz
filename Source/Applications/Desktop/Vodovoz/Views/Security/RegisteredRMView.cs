using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Security;
using Gamma.GtkWidgets;
using Vodovoz.Core.Domain.Users;

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

            ytreeviewUsers.ColumnsConfig = ColumnsConfigFactory.Create<User>()
                .AddColumn("Номер").AddTextRenderer(x => x.Id.ToString())
                .AddColumn("Имя").AddTextRenderer(x => x.Name)
                .AddColumn("")
                .Finish();

            ytreeviewUsers.ItemsDataSource = ViewModel.Entity.Users;
            ytreeviewUsers.YTreeModel.EmitModelChanged();

            if (string.IsNullOrWhiteSpace(ViewModel.Entity.Domain))
            {
                ViewModel.Entity.Domain = "VODOVOZ";
            }

            buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
        }

        protected void OnButtonAddUserClicked(object sender, System.EventArgs e)
        {
            ViewModel.AddUserCommand.Execute();
        }

        protected void OnButtonDeleteUserClicked(object sender, System.EventArgs e)
        {
            ViewModel.RemoveUserCommand.Execute(ytreeviewUsers.GetSelectedObject<User>());
        }
    }
}
