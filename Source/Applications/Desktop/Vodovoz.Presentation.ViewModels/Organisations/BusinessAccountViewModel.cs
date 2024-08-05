using System;
using System.ComponentModel;
using Autofac;
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
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class BusinessAccountViewModel : EntityDialogViewModelBase<BusinessAccount>
	{
		private readonly IPermissionResult _permissionResult;
		private readonly ViewModelEEVMBuilder<BusinessActivity> _businessActivityViewModelBuilder;
		private readonly ViewModelEEVMBuilder<Funds> _fundsViewModelBuilder;
		private Subdivision _subdivision;

		public BusinessAccountViewModel(
			IEntityUoWBuilder uowBuilder,
			ILifetimeScope lifetimeScope,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			ViewModelEEVMBuilder<BusinessActivity> businessActivityViewModelBuilder,
			ViewModelEEVMBuilder<Funds> fundsViewModelBuilder,
			IStringHandler stringHandler,
			IValidator validator) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			_permissionResult =
				(currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService)))
				.ValidateEntityPermission(typeof(Funds));
			_businessActivityViewModelBuilder =
				businessActivityViewModelBuilder ?? throw new ArgumentNullException(nameof(businessActivityViewModelBuilder));
			_fundsViewModelBuilder = fundsViewModelBuilder ?? throw new ArgumentNullException(nameof(fundsViewModelBuilder));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));

			Initialize();
		}

		public Subdivision Subdivision
		{
			get => _subdivision;
			set
			{
				if(SetField(ref _subdivision, value))
				{
					Entity.SubdivisionId = _subdivision?.Id;
				}
			}
		}
		
		public bool CanShowSubdivision
		{
			get
			{
				if(Entity.AccountFillType == AccountFillType.CashSubdivision)
				{
					return true;
				}

				Subdivision = null;
				return false;
			}
		}

		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }

		public IEntityEntryViewModel BusinessActivityViewModel { get; private set; }
		public IEntityEntryViewModel FundsViewModel { get; private set; }
		public ILifetimeScope LifetimeScope { get; }
		public IStringHandler StringHandler { get; }

		public bool CanEdit => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;
		public string IdString => Entity.Id.ToString();
		public bool CanShowId => Entity.Id > 0;
		public bool CanShowAccountFillType => Entity.Funds != null;

		private void Initialize()
		{
			if(Entity.SubdivisionId.HasValue)
			{
				_subdivision = UoW.GetById<Subdivision>(Entity.SubdivisionId.Value);
			}
			
			CreateCommands();
			InitializeEntryViewModels();
			Entity.PropertyChanged += OnEntityPropertyChanged;
		}
		
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
			
			if(e.PropertyName == nameof(Entity.AccountFillType))
			{
				OnPropertyChanged(nameof(CanShowSubdivision));
			}
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= OnEntityPropertyChanged;
			base.Dispose();
		}
	}
}
