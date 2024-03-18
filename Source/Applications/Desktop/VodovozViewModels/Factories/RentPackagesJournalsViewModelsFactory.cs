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
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly INavigationManager _navigationManager;

	    public RentPackagesJournalsViewModelsFactory(IUnitOfWorkFactory uowFactory, INavigationManager navigationManager)
        {
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        public PaidRentPackagesJournalViewModel CreatePaidRentPackagesJournalViewModel(
            bool multipleSelect = false, bool isCreateVisible = true, bool isEditVisible = true, bool isDeleteVisible = true)
        {
            var journal = new PaidRentPackagesJournalViewModel(
				_uowFactory, ServicesConfig.InteractiveService, _navigationManager)
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
				_uowFactory, ServicesConfig.InteractiveService, _navigationManager, new FreeRentPackagesFilterViewModel())
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
