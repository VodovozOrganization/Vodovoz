using System;
using System.Collections.Generic;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Tools;
using VodovozBusiness.Domain.Payments;
using static Vodovoz.Filters.ViewModels.PaymentsJournalFilterViewModel;

namespace Vodovoz.Filters.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsJournalFilterView : FilterViewBase<PaymentsJournalFilterViewModel>
	{
		public PaymentsJournalFilterView(PaymentsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		void Configure()
		{
			dateRangeFilter.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm=> vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			yenumcomboPaymentState.Binding
				.AddBinding(ViewModel, vm => vm.PaymentState, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			yenumcomboPaymentState.ItemsEnum = typeof(PaymentState);
			ycheckbtnHideCompleted.Binding
				.AddBinding(ViewModel, vm => vm.HideCompleted, w => w.Active)
				.InitializeFromSource();
			chkHideAllocatedPayments.Binding
				.AddBinding(ViewModel, vm => vm.HideAllocatedPayments, w => w.Active)
				.InitializeFromSource();
			isMannualyCreatedBtn.RenderMode = RenderMode.Symbol;
			isMannualyCreatedBtn.Binding
				.AddBinding(ViewModel, vm => vm.IsManuallyCreated, w => w.Active)
				.InitializeFromSource();
			chkPaymentsWithoutCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.HidePaymentsWithoutCounterparty, w => w.Active)
				.InitializeFromSource();
			chkHideCancelledPayments.Binding
				.AddBinding(ViewModel, vm => vm.HideCancelledPayments, w => w.Active)
				.InitializeFromSource();
			yenumcmbSortType.Binding
				.AddBinding(ViewModel, vm => vm.SortType, w => w.SelectedItem)
				.InitializeFromSource();
			yenumcmbSortType.ItemsEnum = typeof(PaymentJournalSortType);

			slcbDocumentType.ItemsList = new List<Type>
			{
				null,
				typeof(Payment),
				typeof(PaymentWriteOff),
				typeof(OutgoingPayment)
			};

			slcbDocumentType.ShowSpecialStateAll = true;

			slcbDocumentType.SetRenderTextFunc<Type>(x => x != null
				? x.GetClassUserFriendlyName()
					.Nominative.CapitalizeSentence()
				: "Все");

			slcbDocumentType.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeDocumentType, w=> w.Sensitive)
				.AddBinding(ViewModel, vm => vm.DocumentTypeObject, w => w.SelectedItem)
				.InitializeFromSource();

			ConfigureEntityEntries();
		}

		private void ConfigureEntityEntries()
		{
			var builder = new LegacyEEVMBuilderFactory<PaymentsJournalFilterViewModel>(
				ViewModel.JournalTab, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);

			counterpartyEntry.ViewModel = builder.ForProperty(x => x.Counterparty)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			organizationEntry.ViewModel = ViewModel.OrganizationEntryViewModel;
			organizationBankEntry.ViewModel = ViewModel.OrganizationBankEntryViewModel;
			organizationAccountEntry.ViewModel = ViewModel.OrganizationAccountEntryViewModel;
		}
	}
}
