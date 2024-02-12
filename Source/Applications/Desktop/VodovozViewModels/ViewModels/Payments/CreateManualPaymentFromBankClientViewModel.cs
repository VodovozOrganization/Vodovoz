using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autofac;
using QS.Commands;
using QS.ViewModels;
using Vodovoz.Domain.Payments;
using QS.Project.Domain;
using QS.Services;
using QS.DomainModel.UoW;
using QS.Navigation;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class CreateManualPaymentFromBankClientViewModel : EntityTabViewModelBase<Payment>
	{
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IOrganizationParametersProvider _organizationParametersProvider;
		private const int _paymentNumForUpdateBalance = 120820;
		private const string _updateBalanceTag = "Ввод остатков";
		private int _defaultPaymentNum = 1;

		private bool _isPaymentForUpdateBalance;
		private DelegateCommand _saveAndOpenManualPaymentMatchingCommand;
		private DelegateCommand _changePaymentNumAndPaymentPurposeCommand;

		public CreateManualPaymentFromBankClientViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IPaymentsRepository paymentsRepository,
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

			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_organizationParametersProvider =
				organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));

			Configure(profitCategoryRepository, profitCategoryProvider);
			Entity.PropertyChanged += OnEntityPropertyChanged;
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

		protected override bool BeforeSave()
		{
			Entity.FillPropertiesFromCounterparty();
			return base.BeforeSave();
		}

		private void Configure(IProfitCategoryRepository profitCategoryRepository, IProfitCategoryProvider profitCategoryProvider)
		{
			Entity.PaymentNum = _defaultPaymentNum;
			ProfitCategories = profitCategoryRepository.GetAllProfitCategories(UoW);
			Entity.Date = DateTime.Today;
			Entity.Organization = _organizationRepository.GetOrganizationById(UoW, _organizationParametersProvider.VodovozOrganizationId);
			Entity.ProfitCategory = profitCategoryRepository.GetProfitCategoryById(UoW, profitCategoryProvider.GetDefaultProfitCategory());
			Entity.Status = PaymentState.undistributed;
			Entity.IsManuallyCreated = true;
		}
		
		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Counterparty))
			{
				UpdatePaymentNum();
				UpdateParameters();
			}
		}

		private void UpdateParameters()
		{
			var defaultAccount = Entity.Counterparty?.Accounts.SingleOrDefault(x => x.IsDefault);

			if(defaultAccount is null)
			{
				return;
			}

			Entity.CounterpartyBank = defaultAccount.InBank?.Name;
			Entity.CounterpartyBik = defaultAccount.InBank?.Bik;
			Entity.CounterpartyCurrentAcc = defaultAccount.Number;
			Entity.CounterpartyAcc = defaultAccount.Number;
			Entity.CounterpartyCorrespondentAcc = defaultAccount.BankCorAccount?.CorAccountNumber;
		}

		private void UpdatePaymentNum()
		{
			if(Entity.Counterparty != null)
			{
				Entity.PaymentNum =
					_paymentsRepository.GetMaxPaymentNumFromManualPayments(
						UoW, Entity.Counterparty.Id, _organizationParametersProvider.VodovozOrganizationId)
					+ 1;
				_defaultPaymentNum = Entity.PaymentNum;
			}
		}
	}
}
