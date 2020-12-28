using System;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Proposal;

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
            ybtnSend.Clicked += (sender, args) => ViewModel.SaveAndClose();
            ybtnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

            yentryTitle.Binding.AddBinding(ViewModel.Entity, e => e.Title, w => w.Text).InitializeFromSource();
            yentryLocation.Binding.AddBinding(ViewModel.Entity, e => e.Location, w => w.Text).InitializeFromSource();
            
            ytextviewDescription.Binding.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text).InitializeFromSource();
        }
    }
}
