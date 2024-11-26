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
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;
using System.Linq;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class CreateManualPaymentFromBankClientViewModel : EntityTabViewModelBase<CashlessIncome>
	{
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private const int _paymentNumForUpdateBalance = 120820;
		private const string _updateBalanceTag = "Ввод остатков";
		private int _defaultPaymentNum = 1;

		private bool _isPaymentForUpdateBalance;
		private string _comment;
		private ProfitCategory _profitCategory;
		private Domain.Client.Counterparty _selectedCounterparty;
		private DelegateCommand _saveAndOpenManualPaymentMatchingCommand;
		private DelegateCommand _changePaymentNumAndPaymentPurposeCommand;

		public CreateManualPaymentFromBankClientViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IPaymentsRepository paymentsRepository,
			IProfitCategoryRepository profitCategoryRepository,
			IPaymentSettings profitCategoryProvider,
			IOrganizationSettings organizationSettings,
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
			_organizationSettings =
				organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));

			Configure(profitCategoryRepository, profitCategoryProvider);
		}

		public int Number
		{
			get => Entity.Number;
			set
			{
				if(Entity.Number != value)
				{
					return;
				}

				Entity.Number = value;
				OnPropertyChanged();
			}
		}

		public Domain.Client.Counterparty SelectedCounterparty
		{
			get => _selectedCounterparty;
			set
			{
				if(SetField(ref _selectedCounterparty, value))
				{
					UpdatePaymentNum();
					UpdatePayerDetails();
				}
			}
		}

		public bool IsPaymentForUpdateBalance
		{
			get => _isPaymentForUpdateBalance;
			set => SetField(ref _isPaymentForUpdateBalance, value);
		}

		public string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public ProfitCategory ProfitCategory
		{
			get => _profitCategory;
			set => SetField(ref _profitCategory, value);
		}

		public IEnumerable<ProfitCategory> ProfitCategories { get; private set; }
		public ILifetimeScope Scope { get; }

		public DelegateCommand SaveAndOpenManualPaymentMatchingCommand =>
			_saveAndOpenManualPaymentMatchingCommand ?? (_saveAndOpenManualPaymentMatchingCommand = new DelegateCommand(
					() =>
					{
						if(Save(true))
						{
							NavigationManager.OpenViewModel<ManualCashlessIncomeMatchingViewModel, IEntityUoWBuilder>(
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
							Number = _paymentNumForUpdateBalance;
						}
						else
						{
							Entity.PaymentPurpose = string.Empty;
							Number = _defaultPaymentNum;
						}
					}
				)
			);


		public override bool Save(bool close)
		{
			Entity.UpdateFirstPayment(SelectedCounterparty, ProfitCategory, Comment);
			return base.Save(close);
		}

		private void Configure(IProfitCategoryRepository profitCategoryRepository, IPaymentSettings paymentSettings)
		{
			Entity.DefaultManuallyIncome(
				_defaultPaymentNum,
				_organizationSettings.VodovozOrganizationId,
				PaymentState.undistributed,
				paymentSettings);
			
			ProfitCategories = profitCategoryRepository.GetAllProfitCategories(UoW);
		}

		private void UpdatePayerDetails()
		{
			Entity.UpdatePayerDetails(SelectedCounterparty);
			UpdatePayerAccountDetails();
		}

		private void UpdatePayerAccountDetails()
		{
			var defaultAccount = SelectedCounterparty?.Accounts.SingleOrDefault(x => x.IsDefault);

			if(defaultAccount is null)
			{
				return;
			}

			Entity.UpdatePayerAccountDetails(defaultAccount);
		}

		private void UpdatePaymentNum()
		{
			if(SelectedCounterparty != null)
			{
				Number = _paymentsRepository.GetMaxPaymentNumFromManualPayments(
					UoW, SelectedCounterparty.Id, _organizationSettings.VodovozOrganizationId)
				         + 1;
				
				_defaultPaymentNum = Number;
			}
		}
	}
}
