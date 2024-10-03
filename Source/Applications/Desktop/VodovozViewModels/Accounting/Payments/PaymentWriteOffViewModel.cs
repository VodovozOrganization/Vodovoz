﻿using DocumentFormat.OpenXml.Wordprocessing;
using QS.Commands;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Windows.Input;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.PermissionExtensions;
using Vodovoz.Settings.Common;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.Accounting.Payments
{
	public class PaymentWriteOffViewModel : EntityTabViewModelBase<PaymentWriteOff>
	{
		private readonly IPermissionResult _permissionResult;
		private readonly IGeneralSettings _generalSettings;
		private readonly IPaymentsRepository _paymentsRepository;
		private Counterparty _counterparty;
		private Organization _organization;
		private FinancialExpenseCategory _financialExpenseCategory;
		private decimal _maxSum;
		private int _maxLength;

		public PaymentWriteOffViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			IGeneralSettings generalSettings,
			IPaymentsRepository paymentsRepository,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			ViewModelEEVMBuilder<FinancialExpenseCategory> financialExpenseCategoryViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(entityExtendedPermissionValidator is null)
			{
				throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			}

			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));

			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			_permissionResult = currentPermissionService.ValidateEntityPermission(typeof(PaymentWriteOff));

			if(!_permissionResult.CanRead)
			{
				throw new AbortCreatingPageException("У вас нет доступа к этому документу", "Доступ запрещён");
			}

			if(financialExpenseCategoryViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(financialExpenseCategoryViewModelEEVMBuilder));
			}

			CanEditDate = entityExtendedPermissionValidator.Validate(typeof(PaymentWriteOff), CommonServices.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));

			FinancialExpenseCategoryViewModel = financialExpenseCategoryViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(filter =>
				{
					filter.RestrictFinancialSubtype = FinancialSubType.Expense;
					filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));

					foreach(var includedId in _generalSettings.PaymentWriteOffAllowedFinancialExpenseCategories)
					{
						filter.IncludeExpenseCategoryIds.Add(includedId);
					}
				})
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.Finish();

			SaveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			CancelCommand = new DelegateCommand(CancelHandler);

			SetPropertyChangeRelation(
				x => x.CounterpartyId,
				() => Counterparty);

			SetPropertyChangeRelation(
				x => x.OrganizationId,
				() => Organization);

			SetPropertyChangeRelation(
				x => x.FinancialExpenseCategoryId,
				() => FinancialExpenseCategory);

			SetPropertyChangeRelation(
				x => x.Sum,
				() => Sum);

			if(Entity.Id == 0)
			{
				Entity.Date = DateTime.Now;
			}
		}

		public bool CanSave => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;

		public bool CanEditDate { get; }

		public Counterparty Counterparty
		{
			get => this.GetIdRefField(ref _counterparty, Entity.CounterpartyId);
			set
			{
				if(this.SetIdRefField(SetField, ref _counterparty, () => Entity.CounterpartyId, value))
				{
					UpdateSum();
				}
			}
		}

		public Organization Organization
		{
			get => this.GetIdRefField(ref _organization, Entity.OrganizationId);
			set
			{
				if(this.SetIdRefField(SetField, ref _organization, () => Entity.OrganizationId, value))
				{
					UpdateSum();
				}
			}
		}

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.FinancialExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.FinancialExpenseCategoryId, value);
		}

		public decimal Sum
		{
			get => Entity.Sum;
			set => Entity.Sum = Math.Min(value, MaxSum);
		}

		public decimal MaxSum
		{
			get => _maxSum;
			private set
			{
				if(SetField(ref _maxSum, value))
				{
					MaxLength = value.ToString().Length;
				}
			}
		}

		public int MaxLength
		{
			get => _maxLength;
			private set => SetField(ref _maxLength, value);
		}

		public IEntityEntryViewModel CounterpartyViewModel { get; set; }
		public IEntityEntryViewModel OrganizationViewModel { get; set; }
		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }
		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }

		private void UpdateSum()
		{
			if(Entity.CounterpartyId.HasValue && Entity.OrganizationId.HasValue)
			{
				var balance = _paymentsRepository.GetCounterpartyLastBalance(UoW, Entity.CounterpartyId.Value, Entity.OrganizationId.Value);

				MaxSum = balance + Sum;

				if(balance > 0)
				{
					Sum = balance;
				}
			}
		}

		protected override bool BeforeValidation()
		{
			if(ValidationContext.ServiceContainer.GetService(typeof(IUnitOfWork)) == null)
			{
				ValidationContext.ServiceContainer.AddService(typeof(IUnitOfWork), UoW);
			}
			return base.BeforeValidation();
		}

		private void SaveHandler()
		{
			SaveAndClose();
		}

		private void CancelHandler()
		{
			Close(true, CloseSource.Cancel);
		}
	}
}
