using System;
using System.ComponentModel;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class BusinessAccountViewModel : EntityDialogViewModelBase<BusinessAccount>
	{
		private readonly IPermissionResult _permissionResult;
		private readonly ViewModelEEVMBuilder<BusinessActivity> _businessActivityViewModelBuilder;
		private readonly ViewModelEEVMBuilder<Funds> _fundsViewModelBuilder;

		public BusinessAccountViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			ViewModelEEVMBuilder<BusinessActivity> businessActivityViewModelBuilder,
			ViewModelEEVMBuilder<Funds> fundsViewModelBuilder,
			IValidator validator) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			_permissionResult =
				(currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService)))
				.ValidateEntityPermission(typeof(Funds));
			_businessActivityViewModelBuilder =
				businessActivityViewModelBuilder ?? throw new ArgumentNullException(nameof(businessActivityViewModelBuilder));
			_fundsViewModelBuilder = fundsViewModelBuilder ?? throw new ArgumentNullException(nameof(fundsViewModelBuilder));

			CreateCommands();
			InitializeEntryViewModels();
			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }

		public IEntityEntryViewModel BusinessActivityViewModel { get; private set; }
		public IEntityEntryViewModel FundsViewModel { get; private set; }

		public bool CanEdit => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;
		public string IdString => Entity.Id.ToString();
		public bool CanShowId => Entity.Id > 0;
		public bool CanShowAccountFillType => Entity.Funds != null;

		private void CreateCommands()
		{
			SaveCommand = new DelegateCommand(() => SaveAndClose());
			SaveCommand.CanExecuteChangedWith(this, x => x.CanEdit);
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}

		private void InitializeEntryViewModels()
		{
			var businessActivityViewModel = _businessActivityViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.BusinessActivity)
				.UseViewModelJournalAndAutocompleter<BusinessActivitiesJournalViewModel>()
				.UseViewModelDialog<BusinessActivityViewModel>()
				.Finish();

			businessActivityViewModel.IsEditable = CanEdit;
			BusinessActivityViewModel = businessActivityViewModel;

			var fundsViewModel = _fundsViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Funds)
				.UseViewModelJournalAndAutocompleter<FundsJournalViewModel>()
				.UseViewModelDialog<FundsViewModel>()
				.Finish();

			fundsViewModel.IsEditable = CanEdit;
			FundsViewModel = fundsViewModel;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Funds))
			{
				if(Entity.Funds != null)
				{
					Entity.AccountFillType = Entity.Funds.DefaultAccountFillType;
				}

				OnPropertyChanged(nameof(CanShowAccountFillType));
			}
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= OnEntityPropertyChanged;
			base.Dispose();
		}
	}
}
