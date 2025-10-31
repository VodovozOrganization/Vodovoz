using System;
using System.Windows.Input;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class NotAllocatedCounterpartyViewModel : EntityTabViewModelBase<NotAllocatedCounterparty>, IAskSaveOnCloseViewModel
	{
		private readonly ViewModelEEVMBuilder<ProfitCategory> _profitCategoryViewModelBuilder;

		public NotAllocatedCounterpartyViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ViewModelEEVMBuilder<ProfitCategory> profitCategoryViewModelBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_profitCategoryViewModelBuilder =
				profitCategoryViewModelBuilder ?? throw new ArgumentNullException(nameof(profitCategoryViewModelBuilder));

			Configure();
		}

		public string IdString { get; private set; }
		public bool CanEdit => (Entity.Id == 0 && PermissionResult.CanCreate) || PermissionResult.CanUpdate; 
		public bool CanShowId => Entity.Id != 0;
		public bool AskSaveOnClose => CanEdit;
		
		public ICommand SaveCommand { get; private set; }
		public ICommand CancelCommand { get; private set; }
		
		public IEntityEntryViewModel ProfitCategoryEntryViewModel { get; private set; }

		private void Configure()
		{
			IdString = Entity.Id.ToString();
			InitializeCommands();
			ConfigureEntryViewModels();
		}
		
		private void InitializeCommands()
		{
			SaveCommand = new DelegateCommand(SaveAndClose);
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}
		
		private void ConfigureEntryViewModels()
		{
			var profitCategoryEntryViewModel = _profitCategoryViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.ProfitCategory)
				.UseViewModelJournalAndAutocompleter<ProfitCategoriesJournalViewModel>()
				.UseViewModelDialog<ProfitCategoryViewModel>()
				.Finish();

			profitCategoryEntryViewModel.CanViewEntity = false;
			profitCategoryEntryViewModel.IsEditable = CanEdit;
			ProfitCategoryEntryViewModel = profitCategoryEntryViewModel;
		}
	}
}
