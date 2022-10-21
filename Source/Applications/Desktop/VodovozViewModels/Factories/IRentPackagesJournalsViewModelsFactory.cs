using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.Factories
{
    public interface IRentPackagesJournalsViewModelsFactory
    {
        PaidRentPackagesJournalViewModel CreatePaidRentPackagesJournalViewModel(
            bool multipleSelect = false, bool isCreateVisible = true, bool isEditVisible = true, bool isDeleteVisible = true);
        FreeRentPackagesJournalViewModel CreateFreeRentPackagesJournalViewModel(
            bool multipleSelect = false, bool isCreateVisible = true, bool isEditVisible = true, bool isDeleteVisible = true);
    }
}