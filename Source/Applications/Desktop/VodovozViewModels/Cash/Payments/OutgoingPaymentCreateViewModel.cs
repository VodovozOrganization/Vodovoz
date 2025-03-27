using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using ReactiveUI;
using System;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Organizations;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.Cash.Payments
{
	public class OutgoingPaymentCreateViewModel : EntityTabViewModelBase<OutgoingPayment>
	{
		private FinancialExpenseCategory _financialExpenseCategory;
		private Organization _ourOrganization;
		private Counterparty _counterparty;
		private DateTime? _paymentDate;

		public OutgoingPaymentCreateViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ViewModelEEVMBuilder<FinancialExpenseCategory> financialExpenseCategoryViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Organization> ourOrganizationViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			FinancialExpenseCategoryViewModel = financialExpenseCategoryViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(filter =>
				{
					filter.RestrictFinancialSubtype = FinancialSubType.Expense;
					filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
				})
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.Finish();

			OurOrganizationViewModel = ourOrganizationViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.OurOrganization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			SaveCommand = new DelegateCommand(SaveAndClose, () => PermissionResult.CanCreate);
			CancelCommand = new DelegateCommand(() => Close(PermissionResult.CanCreate && HasChanges, CloseSource.Cancel));
		}

		public DateTime? PaymentDate
		{
			get => _paymentDate;
			set => SetField(ref _paymentDate, value);
		}

		public IEntityEntryViewModel CounterpartyViewModel { get; set; }
		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }
		public IEntityEntryViewModel OurOrganizationViewModel { get; }

		public Counterparty Counterparty
		{
			get => this.GetIdRefField(ref _counterparty, Entity.CounterpartyId);
			set => this.SetIdRefField(SetField, ref _counterparty, () => Entity.CounterpartyId, value);
		}

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.FinancialExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.FinancialExpenseCategoryId, value);
		}

		public Organization OurOrganization
		{
			get => this.GetIdRefField(ref _ourOrganization, Entity.OrganizationId);
			set => this.SetIdRefField(SetField, ref _ourOrganization, () => Entity.OrganizationId, value);
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public override bool Save(bool close)
		{
			if(!PaymentDate.HasValue)
			{
				CommonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Error, "Необходимо заполнить дату платежа перед сохранением");
				return false;
			}

			Entity.CreatedAt = DateTime.Now;
			Entity.PaymentDate = PaymentDate.Value;

			var cashlessRequestCandidatToAdd = UoW.Session
				.Query<CashlessRequest>()
				.Where(x => (x.PayoutRequestState == PayoutRequestState.PartiallyClosed
					|| x.PayoutRequestState == PayoutRequestState.GivenForTake)
					&& x.Counterparty.Id == Entity.CounterpartyId
					&& x.Organization.Id == Entity.OrganizationId
					&& x.Sum >= Entity.Sum
					&& x.PaymentPurpose == Entity.PaymentPurpose)
				.OrderBy(x => x.BillDate)
				.FirstOrDefault();

			if(cashlessRequestCandidatToAdd is CashlessRequest cashlessRequest)
			{
				if(cashlessRequest.Sum
					- cashlessRequest.OutgoingPayments.Sum(x => x.Sum)
					- Entity.Sum == 0)
				{
					cashlessRequest.ChangeState(PayoutRequestState.Closed);
				}
				else if(cashlessRequest.PayoutRequestState != PayoutRequestState.PartiallyClosed)
				{
					cashlessRequest.ChangeState(PayoutRequestState.PartiallyClosed);
				}
					
				Entity.CashlessRequestId = cashlessRequest.Id;

				UoW.Save(cashlessRequest);
			}

			return base.Save(close);
		}
	}
}
