using System;
using System.Collections.Generic;
using Autofac;
using QS.Commands;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using QS.Project.Domain;
using QS.Services;
using QS.DomainModel.UoW;
using QS.Navigation;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class PaymentFromBankClientViewModel : EntityTabViewModelBase<Payment>
	{
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IOrganizationParametersProvider _organizationParametersProvider;
		private const int _paymentNumForUpdateBalance = 120820;
		private const int _defaultPaymentNum = 1;
		private const string _updateBalanceTag = "Ввод остатков";
		
		private bool _isPaymentForUpdateBalance;
		private DelegateCommand _saveAndOpenManualPaymentMatchingCommand;
		private DelegateCommand _changePaymentNumAndPaymentPurposeCommand;
		
		public PaymentFromBankClientViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IProfitCategoryRepository profitCategoryRepository,
			IProfitCategoryProvider profitCategoryProvider,
			IOrganizationRepository organizationRepository,
			IOrganizationParametersProvider organizationParametersProvider,
			ILifetimeScope scope) : base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(profitCategoryRepository == null)
			{
				throw new ArgumentNullException(nameof(profitCategoryRepository));
			}
			if(profitCategoryProvider == null)
			{
				throw new ArgumentNullException(nameof(profitCategoryProvider));
			}

			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_organizationParametersProvider =
				organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));

			Configure(profitCategoryRepository, profitCategoryProvider);
		}

		public bool IsPaymentForUpdateBalance
		{
			get => _isPaymentForUpdateBalance;
			set => SetField(ref _isPaymentForUpdateBalance, value);
		}
		
		public IEnumerable<ProfitCategory> ProfitCategories { get; private set; }
		public ILifetimeScope Scope { get; }

		public DelegateCommand SaveAndOpenManualPaymentMatchingCommand =>
			_saveAndOpenManualPaymentMatchingCommand ?? (_saveAndOpenManualPaymentMatchingCommand = new DelegateCommand(
					() =>
					{
						if(Save(true))
						{
							NavigationManager.OpenViewModel<ManualPaymentMatchingViewModel, IEntityUoWBuilder>(
								this, EntityUoWBuilder.ForOpen(Entity.Id));
						}
					}
				)
			);
		
		public DelegateCommand ChangePaymentNumAndPaymentPurposeCommand =>
			_changePaymentNumAndPaymentPurposeCommand ?? (_changePaymentNumAndPaymentPurposeCommand = new DelegateCommand(
					() =>
					{
						if(IsPaymentForUpdateBalance)
						{
							Entity.PaymentPurpose = _updateBalanceTag;
							Entity.PaymentNum = _paymentNumForUpdateBalance;
						}
						else
						{
							Entity.PaymentPurpose = string.Empty;
							Entity.PaymentNum = _defaultPaymentNum;
						}
					}
				)
			);

		protected override void BeforeSave()
		{
			Entity.FillPropertiesFromCounterparty();
		}

		private void Configure(IProfitCategoryRepository profitCategoryRepository, IProfitCategoryProvider profitCategoryProvider)
		{
			Entity.PaymentNum = _defaultPaymentNum;
			ProfitCategories = profitCategoryRepository.GetAllProfitCategories(UoW);
			Entity.Date = DateTime.Today;
			Entity.Organization = _organizationRepository.GetOrganizationById(UoW, _organizationParametersProvider.VodovozOrganizationId);
			Entity.ProfitCategory = profitCategoryRepository.GetProfitCategory(UoW, profitCategoryProvider.GetDefaultProfitCategory());
			Entity.Status = PaymentState.undistributed;
			Entity.IsManualCreated = true;
		}
	}
}
