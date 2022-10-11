using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels;

namespace Vodovoz.Filters.GtkViews
{
    public partial class OrganizationCashTransferDocumentFilterView : FilterViewBase<OrganizationCashTransferDocumentFilterViewModel>
    {
        public OrganizationCashTransferDocumentFilterView(OrganizationCashTransferDocumentFilterViewModel filterViewModel) : base(filterViewModel)
        {
            this.Build();
            Configure();
        }

        void Configure()
        {
            dateperiodCashTransfer.StartDateOrNull = DateTime.Today;
            dateperiodCashTransfer.EndDateOrNull = DateTime.Today;
            dateperiodCashTransfer.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
            dateperiodCashTransfer.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();

            entityviewmodelentryAuthor.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
            entityviewmodelentryAuthor.Binding.AddBinding(ViewModel, vm => vm.Author, w => w.Subject).InitializeFromSource();

            speciallistCmbOrganisationsFrom.ItemsList = ViewModel.Organizations;
            speciallistCmbOrganisationsFrom.Binding.AddBinding(ViewModel, vm => vm.OrganizationFrom, w => w.SelectedItem).InitializeFromSource();

            speciallistCmbOrganisationsTo.ItemsList = ViewModel.Organizations;
            speciallistCmbOrganisationsTo.Binding.AddBinding(ViewModel, vm => vm.OrganizationTo, w => w.SelectedItem).InitializeFromSource();
        }
    }
}
