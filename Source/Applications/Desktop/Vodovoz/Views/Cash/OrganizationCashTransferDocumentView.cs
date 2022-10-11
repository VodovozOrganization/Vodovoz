using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
    public partial class OrganizationCashTransferDocumentView : TabViewBase<OrganizationCashTransferDocumentViewModel>
    {
        public OrganizationCashTransferDocumentView(OrganizationCashTransferDocumentViewModel viewModel) :
            base(viewModel)
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            ylabelAuthor.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.Author.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();

            ydatepickerDocumentDate.Binding.AddBinding(ViewModel.Entity, e => e.DocumentDate, w => w.Date).InitializeFromSource();
            ydatepickerDocumentDate.Binding.AddBinding(ViewModel, vm => vm.CanEditRectroactively, w => w.Sensitive).InitializeFromSource();

            speciallistCmbOrganisationsFrom.ShowSpecialStateAll = false;
            speciallistCmbOrganisationsFrom.ItemsList = ViewModel.Organizations;
            speciallistCmbOrganisationsFrom.Binding.AddBinding(ViewModel.Entity, e => e.OrganizationFrom, w => w.SelectedItem).InitializeFromSource();
            speciallistCmbOrganisationsFrom.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

            speciallistCmbOrganisationsTo.ShowSpecialStateAll = false;
            speciallistCmbOrganisationsTo.ItemsList = ViewModel.Organizations;
            speciallistCmbOrganisationsTo.Binding.AddBinding(ViewModel.Entity, e => e.OrganizationTo, w => w.SelectedItem).InitializeFromSource();
            speciallistCmbOrganisationsTo.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

            yspinMoney.Binding.AddBinding(ViewModel.Entity, e => e.TransferedSum, w => w.ValueAsDecimal).InitializeFromSource();
            yspinMoney.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

            ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
            ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
            
            buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
            
            buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
            buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
        }
    }
}
