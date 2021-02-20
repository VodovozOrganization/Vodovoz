using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Security;
using Vodovoz.Domain.Employees;
using Gamma.GtkWidgets;
using Vodovoz.Journals.FilterViewModels.Employees;
using Vodovoz.Journals;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Project.Journal;
using System.Linq;

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

            ytreeviewUsers.ItemsDataSource = ViewModel.Entity.ObservableUsers;
            ytreeviewUsers.YTreeModel.EmitModelChanged();

            if (string.IsNullOrWhiteSpace(ViewModel.Entity.Domain))
            {
                ViewModel.Entity.Domain = "VODOVOZ";
            }

            buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, e) => ViewModel.Close(false, QS.Navigation.CloseSource.Cancel);
        }

        protected void OnButtonAddUserClicked(object sender, System.EventArgs e)
        {
            var userFilterViewModel = new UserJournalFilterViewModel();
            var userJournalViewModel = new UserJournalViewModel(
                    userFilterViewModel,
                    UnitOfWorkFactory.GetDefaultFactory,
                    ServicesConfig.CommonServices)
            {
                SelectionMode = JournalSelectionMode.Single,
            };

            userJournalViewModel.OnEntitySelectedResult += (s, ea) => {
                var selectedNode = ea.SelectedNodes.FirstOrDefault();
                if (selectedNode == null)
                    return;

                ViewModel.AddUser(ViewModel.UoWGeneric.Session.Get<User>(selectedNode.Id));
            };
            Tab.TabParent.AddSlaveTab(Tab, userJournalViewModel);
        }


        protected void OnButtonDeleteUserClicked(object sender, System.EventArgs e)
        {
            ViewModel.Entity.ObservableUsers.Remove(ytreeviewUsers.GetSelectedObject<User>());
        }
    }
}
