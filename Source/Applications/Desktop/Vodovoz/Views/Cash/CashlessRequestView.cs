using Autofac;
using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Complaints;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozBusiness.Domain.Cash.CashRequest;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.Views.Cash
{
	public partial class CashlessRequestView : TabViewBase<CashlessRequestViewModel>
	{
		private readonly ILifetimeScope _lifetimeScope;

		public CashlessRequestView(
			CashlessRequestViewModel viewModel,
			ILifetimeScope lifetimeScope)
			: base(viewModel)
		{
			_lifetimeScope = lifetimeScope
				?? throw new ArgumentNullException(nameof(lifetimeScope));

			Build();
			Initialize();
		}

		private void Initialize()
		{
			// Шапка

			comboRoleChooser.SetRenderTextFunc<PayoutRequestUserRole>(ur => ur.GetEnumTitle());
			comboRoleChooser.ItemsList = ViewModel.UserRoles;
			comboRoleChooser.Binding
				.AddBinding(ViewModel, vm => vm.UserRole, w => w.SelectedItem)
				.InitializeFromSource();

			comboRoleChooser.Sensitive = ViewModel.IsRoleChooserSensitive;

			labelStatus.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.PayoutRequestState.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();

			// Левая колонка

			entryAuthorEmployee.ViewModel = ViewModel.AuthorViewModel;

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			entrySubdivision.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryFinancialResponsibilityCenter.ViewModel = ViewModel.FinancialResponsibilityCenterViewModel;

			entryFinancialResponsibilityCenter.Binding
				.AddBinding(
					ViewModel,
					vm => vm.CanChangeFinancialResponsibilityCenter,
					w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			datepickerPaymentDatePlanned.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentDatePlanned, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditPaymentDatePlanned, w => w.IsEditable)
				.InitializeFromSource();

			entryOrganization.ViewModel = ViewModel.OrganizationViewModel;

			entryOrganizationBankAccount.ViewModel = ViewModel.OurOrganizationBankAccountViewModel;

			var legacyCounterpartyViewModel = new LegacyEEVMBuilderFactory<CashlessRequest>(
					ViewModel,
					ViewModel.Entity,
					ViewModel.UoW,
					ViewModel.NavigationManager,
					_lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(filter =>
				{
					filter.RestrictCounterpartyType = CounterpartyType.Supplier;
				})
				.Finish();

			legacyCounterpartyViewModel.SetPropertyValue(nameof(legacyCounterpartyViewModel.CanViewEntity), false);

			ViewModel.CounterpartyViewModel = legacyCounterpartyViewModel;
			entryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			entryCounterparty.Binding
				.AddBinding(
					ViewModel,
					vm => vm.CanEditPlainProperties,
					w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryCounterpartyBankAccount.ViewModel = ViewModel.SupplierBankAccountViewModel;

			labelExpenceCategory.Binding
				.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible)
				.InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryExpenseFinancialCategory.Binding
				.AddBinding(ViewModel, vm => vm.CanSetExpenseCategory, w => w.ViewModel.IsEditable)
				.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible)
				.InitializeFromSource();

			yentryBillNumber.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.IsEditable)
				.AddBinding(ViewModel.Entity, e => e.BillNumber, w => w.Text)
				.InitializeFromSource();

			datepickerBillDate.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.IsEditable)
				.AddBinding(ViewModel.Entity, e => e.BillDate, w => w.DateOrNull)
				.InitializeFromSource();

			datepickerBillDate.IsEditable = true;

			spinBillSum.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Sum, w => w.ValueAsDecimal)
				.InitializeFromSource();

			speciallistcomboboxBillVat.ItemsList = ViewModel.VatValues.Keys;

			speciallistcomboboxBillVat.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.Sensitive)
				.AddBinding(ViewModel, e => e.SelectedVatValue, w => w.SelectedItem)
				.InitializeFromSource();

			spinCustomVatValue.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.ShowCustomVat, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.VatValue, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ytextviewPurpose.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.PaymentPurpose, w => w.Buffer.Text)
				.InitializeFromSource();

			InitializeComments();

			// Правая колонка

			InitializePayments();

			ybuttonAddBill.BindCommand(ViewModel.AddOutgoingPaymentCommand);
			ybuttonRemoveBill.BindCommand(ViewModel.RemoveOutgoingPaymentCommand);

			spinBillSumGived.Sensitive = false;
			spinBillSumGived.Binding
				.AddBinding(ViewModel, vm => vm.SumGiven, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinBillSumRemainingToGive.Sensitive = false;
			spinBillSumRemainingToGive.Binding
				.AddBinding(ViewModel, vm => vm.SumRemaining, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ycheckbuttonImidiatelyBill.Binding
				.AddBinding(ViewModel, vm => vm.CanEditPlainProperties, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.IsImidiatelyBill, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonCreateGiveOutSchedule.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateGiveOutSchedule, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Active)
				.InitializeFromSource();

			speciallistcomboboxRepeatIntervalType.ItemsList = Enum.GetValues(typeof(RepeatIntervalTypes));
			speciallistcomboboxRepeatIntervalType.SetRenderTextFunc<RepeatIntervalTypes>(node => node.GetEnumTitle());

			labelRepeatIntervalType.Binding
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			speciallistcomboboxRepeatIntervalType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCreateGiveOutSchedule, w => w.Sensitive)
				.AddBinding(vm => vm.RepeatIntervalType, w => w.SelectedItem)
				.AddBinding(vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			ylabelRepeatsCount.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			spinRepeatsCount.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCreateGiveOutSchedule, w => w.Sensitive)
				.AddBinding(vm => vm.RepeatsCount, w => w.ValueAsInt)
				.AddBinding(vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			ylabelRepeatInterval.Binding
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.AddBinding(vm => vm.ShowDaysBetween, w => w.Visible)
				.InitializeFromSource();

			spinIntervals.Binding
				.AddSource(ViewModel)
				.AddBinding(ViewModel, vm => vm.CanCreateGiveOutSchedule, w => w.Sensitive)
				.AddBinding(vm => vm.DaysBetween, w => w.ValueAsInt)
				.AddBinding(vm => vm.ShowDaysBetween, w => w.Visible)
				.InitializeFromSource();

			buttonSave1.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateGiveOutSchedule, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			// Кнопки

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseTabCommand);

			buttonPayout.BindCommand(ViewModel.PayoutCommand);
			buttonPayout.Binding
				.AddBinding(ViewModel, vm => vm.CanPayout, w => w.Visible)
				.InitializeFromSource();

			btnAccept.BindCommand(ViewModel.AcceptCommand);
			btnAccept.Binding
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Visible)
				.InitializeFromSource();

			btnApprove.BindCommand(ViewModel.ApproveCommand);
			btnApprove.Binding
				.AddBinding(ViewModel, vm => vm.CanApprove, w => w.Visible)
				.InitializeFromSource();
			
			btnSendToWaitingForAgreedByExecutiveDirector.BindCommand(ViewModel.SendToWaitingForAgreedByExecutiveDirectorCommand);
			btnSendToWaitingForAgreedByExecutiveDirector.Binding
				.AddBinding(ViewModel, vm => vm.CanSendToWaitingForAgreedByExecutiveDirector, w => w.Visible)
				.InitializeFromSource();

			btnCancel.BindCommand(ViewModel.CancelRequestCommand);
			btnCancel.Binding
				.AddBinding(ViewModel, vm => vm.CanCancel, w => w.Visible)
				.InitializeFromSource();

			btnReapprove.BindCommand(ViewModel.SendToClarificationCommand);
			btnReapprove.Binding
				.AddBinding(ViewModel, vm => vm.CanSendToClarification, w => w.Visible)
				.InitializeFromSource();

			btnConveyForPayout.BindCommand(ViewModel.ConveyForPayoutCommand);
			btnConveyForPayout.Binding
				.AddBinding(ViewModel, vm => vm.CanConveyForPayout, w => w.Visible)
				.InitializeFromSource();
		}

		private void InitializeComments()
		{
			ytreeviewComments.ShowExpanders = false;
			ytreeviewComments.ColumnsConfig = FluentColumnsConfig<object>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetTime(x))
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetAuthor(x))
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetNodeName(x))
						.WrapWidth(300)
						.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();

			var levels = LevelConfigFactory
				.FirstLevel<CashlessRequestComment, CashlessRequestCommentFileInformation>(x => x.AttachedFileInformations)
				.LastLevel(afi => ViewModel.Entity.Comments.FirstOrDefault(c => c.Id == afi.CashlessRequestCommentId))
				.EndConfig();

			ytreeviewComments.YTreeModel = new LevelTreeModel<CashlessRequestComment>(ViewModel.Entity.Comments, levels);

			ViewModel.Entity.Comments.CollectionChanged += (sender, e) => {
				ytreeviewComments.YTreeModel.EmitModelChanged();
				ytreeviewComments.ExpandAll();
			};

			ytreeviewComments.ExpandAll();
			ytreeviewComments.RowActivated += OnCommentNoeActivated;

			ytextviewComment.Binding
				.AddBinding(ViewModel, vm => vm.NewCommentText, w => w.Buffer.Text)
				.InitializeFromSource();

			ytextviewComment.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAddComment.BindCommand(ViewModel.AddCommentCommand);

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			smallfileinformationsview2.ViewModel = ViewModel.AttachedFileInformationsViewModel;
		}

		private void InitializePayments()
		{
			ytreeview1.CreateFluentColumnsConfig<OutgoingPayment>()
				.AddColumn("Номер")
				.AddNumericRenderer(node => node.PaymentNumber)
				.AddColumn("Время")
				.AddDateRenderer(node => node.PaymentDate)
				.AddColumn("Сумма")
				.AddNumericRenderer(node => node.Sum)
				.Finish();

			ytreeview1.ItemsDataSource = ViewModel.Entity.OutgoingPayments;

			ytreeview1.Binding
				.AddBinding(ViewModel, vm => vm.SelectedOutgoingPaymentObject, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeview1.RowActivated += OnOutgoingPaymentNodeActivated;
		}

		private void OnOutgoingPaymentNodeActivated(object o, RowActivatedArgs args)
		{
			if(!(ytreeview1.GetSelectedObject<OutgoingPayment>() is OutgoingPayment node))
			{
				return;
			}

			ViewModel.OpenOutgoingPaymentsCommand?.Execute();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.AttachedFileInformationsViewModel))
			{
				smallfileinformationsview2.ViewModel = ViewModel.AttachedFileInformationsViewModel;
			}
		}

		private void OnCommentNoeActivated(object o, RowActivatedArgs args)
		{
			if(!(ytreeviewComments.GetSelectedObject() is CashlessRequestCommentFileInformation cashlessRequestCommentFileInformation))
			{
				return;
			}

			ViewModel.OpenFileCommand.Execute(cashlessRequestCommentFileInformation);
		}

		private string GetNodeName(object node)
		{
			if(node is ComplaintDiscussionComment complaintDiscussionComment)
			{
				return complaintDiscussionComment.Comment;
			}

			if(node is CashlessRequestCommentFileInformation cashlessRequestCommentFileInformation)
			{
				return cashlessRequestCommentFileInformation.FileName;
			}

			return "";
		}

		private string GetTime(object node)
		{
			if(node is CashlessRequestComment cashlessRequestComment)
			{
				return cashlessRequestComment.CreatedAt.ToShortDateString() + "\n" + cashlessRequestComment.CreatedAt.ToShortTimeString();
			}

			return "";
		}

		private string GetAuthor(object node)
		{
			if(node is CashlessRequestComment cashlessRequestComment)
			{
				return ViewModel.CachedAuthorsCommentTitles[cashlessRequestComment.AuthorId];
			}

			return "";
		}

		private void SetColor(CellRenderer cell, object node)
		{
			if(node is CashlessRequestComment)
			{
				cell.CellBackgroundGdk = GdkColors.DiscussionCommentBase;
			}
			else
			{
				cell.CellBackgroundGdk = GdkColors.PrimaryBase;
			}
		}

		public override void Destroy()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			ytreeviewComments.RowActivated -= OnCommentNoeActivated;

			base.Destroy();
		}
	}
}
