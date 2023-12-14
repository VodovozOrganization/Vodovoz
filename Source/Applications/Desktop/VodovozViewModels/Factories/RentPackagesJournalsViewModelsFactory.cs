using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.Factories
{
    public class RentPackagesJournalsViewModelsFactory : IRentPackagesJournalsViewModelsFactory
    {
	    private readonly INavigationManager _navigationManager;

	    public RentPackagesJournalsViewModelsFactory(INavigationManager navigationManager)
        {
           _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        public PaidRentPackagesJournalViewModel CreatePaidRentPackagesJournalViewModel(
            bool multipleSelect = false, bool isCreateVisible = true, bool isEditVisible = true, bool isDeleteVisible = true)
        {
            var journal = new PaidRentPackagesJournalViewModel(
	            UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService, _navigationManager)
            {
                SelectionMode = multipleSelect
                    ? JournalSelectionMode.Multiple
                    : JournalSelectionMode.Single,
                VisibleCreateAction = isCreateVisible,
                VisibleEditAction = isEditVisible,
                VisibleDeleteAction = isDeleteVisible
            };

            return journal;
        }
        
        public FreeRentPackagesJournalViewModel CreateFreeRentPackagesJournalViewModel(
            bool multipleSelect = false, bool isCreateVisible = true, bool isEditVisible = true, bool isDeleteVisible = true)
        {
            var journal = new FreeRentPackagesJournalViewModel(
	            UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService, _navigationManager, new FreeRentPackagesFilterViewModel())
            {
                SelectionMode = multipleSelect
                    ? JournalSelectionMode.Multiple
                    : JournalSelectionMode.Single,
                VisibleCreateAction = isCreateVisible,
                VisibleEditAction = isEditVisible,
                VisibleDeleteAction = isDeleteVisible
            };

            return journal;
        }
    }
}
