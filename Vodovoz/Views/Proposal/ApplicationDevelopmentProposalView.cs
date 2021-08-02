using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Proposal;
using Gamma.Utilities;

namespace Vodovoz.Views.Proposal
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class ApplicationDevelopmentProposalView : TabViewBase<ApplicationDevelopmentProposalViewModel>
    {
        public ApplicationDevelopmentProposalView(
        ApplicationDevelopmentProposalViewModel viewModel) : base(viewModel)
        {
            this.Build();
            ConfigureView();
        }

        private void ConfigureView()
        {
            ybtnSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
            ybtnSend.Clicked += (sender, args) => ViewModel.SendCommand.Execute();
            ybtnCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
            ybtnEdit.Clicked += (sender, args) => { 
                ytextviewProposalResponse.Sensitive = true; // Binding проверяется только при первом заходе на форму
                ViewModel.EditCommand.Execute();
            };
            ybtnRejectProposal.Clicked += (sender, args) => ViewModel.RejectCommand.Execute();
            ybtnToTheNextStatus.Clicked += (sender, args) => ViewModel.ChangeStatusCommand.Execute();
            ybtnToTheNextStatus.Binding.AddBinding(ViewModel, vm => vm.NextStateName, w => w.Label).InitializeFromSource();

            ybtnSend.Binding.AddBinding(ViewModel, vm => vm.IsViewElementSensitive, w => w.Sensitive).InitializeFromSource();
            ybtnEdit.Binding.AddBinding(ViewModel, vm => vm.IsEditBtnSensitive, w => w.Sensitive).InitializeFromSource();
            ybtnRejectProposal.Binding.AddBinding(ViewModel, vm => vm.IsBtnRejectSensitive, w => w.Sensitive).InitializeFromSource();
            ybtnRejectProposal.Binding.AddBinding(ViewModel, vm => vm.UserCanManageProposal, w => w.Visible).InitializeFromSource();
            ybtnToTheNextStatus.Binding.AddBinding(ViewModel, vm => vm.IsBtnChangeStatusSensitive, w => w.Sensitive).InitializeFromSource();
            ybtnToTheNextStatus.Binding.AddBinding(ViewModel, vm => vm.UserCanManageProposal, w => w.Visible).InitializeFromSource();

            yentryTitle.Binding.AddBinding(ViewModel.Entity, e => e.Title, w => w.Text).InitializeFromSource();
            yentryTitle.Binding.AddBinding(ViewModel, vm => vm.IsViewElementSensitive, w => w.Sensitive).InitializeFromSource();
            yentryLocation.Binding.AddBinding(ViewModel.Entity, e => e.Location, w => w.Text).InitializeFromSource();
            yentryLocation.Binding.AddBinding(ViewModel, vm => vm.IsViewElementSensitive, w => w.Sensitive).InitializeFromSource();
            
            ytextviewDescription.Binding.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text).InitializeFromSource();
            ytextviewDescription.Binding.AddBinding(ViewModel, vm => vm.IsViewElementSensitive, w => w.Sensitive).InitializeFromSource();
            ytextviewProposalResponse.Binding.AddBinding(ViewModel.Entity, e => e.ProposalResponse, w => w.Buffer.Text).InitializeFromSource();
            ytextviewProposalResponse.Binding.AddBinding(ViewModel, vm => vm.ProposalResponseSensetive, w => w.Sensitive).InitializeFromSource();

            GtkScrolledWindowProposalResponse.Visible = ViewModel.IsProposalResponseVisible;

            ylblProposalResponse.Binding.AddBinding(ViewModel, vm => vm.IsProposalResponseVisible, w => w.Visible).InitializeFromSource();
            ylblStatus.Binding.AddFuncBinding(ViewModel.Entity, e => e.Status.GetEnumTitle(), w => w.Text).InitializeFromSource();
        }
    }
}
