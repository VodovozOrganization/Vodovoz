using Autofac;
using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozBusiness.Domain.Cash.CashRequest;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Views.Cash
{
	public partial class CashlessRequestView : TabViewBase<CashlessRequestViewModel>
	{
		private readonly ILifetimeScope _lifetimeScope;

		public CashlessRequestView(CashlessRequestViewModel viewModel,
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

			entryFinancialResponsibilityCenter.ViewModel = ViewModel.FinancialResponsibilityCenterViewModel;

			datepickerPaymentDatePlanned.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentDatePlanned, w => w.DateOrNull)
				.AddBinding(ViewModel, vm => vm.CanEditPaymentDatePlanned, w => w.IsEditable)
				.InitializeFromSource();

			entryOrganization.ViewModel = ViewModel.OrganizationViewModel;

			entryOrganization.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSetOrganisaton && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanSeeOrganisation, w => w.Visible)
				.InitializeFromSource();

			entryOrganizationBankAccount.ViewModel = ViewModel.OurOrganizationBankAccountViewModel;

			entryOrganizationBankAccount.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSetOrganisaton && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanSeeOrganisation, w => w.Visible)
				.InitializeFromSource();

			ViewModel.CounterpartyViewModel = new LegacyEEVMBuilderFactory<CashlessRequest>(
					ViewModel,
					ViewModel.Entity,
					ViewModel.UoW,
					ViewModel.NavigationManager,
					_lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(filter =>
				{
					filter.RestrictCounterpartyType = Domain.Client.CounterpartyType.Supplier;
				})
				.Finish();

			entryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			entryCounterparty.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.IsNotClosed && !vm.IsSecurityServiceRole,
					w => w.Sensitive)
				.InitializeFromSource();

			entryCounterpartyBankAccount.ViewModel = ViewModel.SupplierBankAccountViewModel;

			labelExpenceCategory.Binding.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible).InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryExpenseFinancialCategory.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSetExpenseCategory && !vm.IsSecurityServiceRole, w => w.ViewModel.IsEditable)
				.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible)
				.InitializeFromSource();

			yentryBillNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.BillNumber, w => w.Text)
				.InitializeFromSource();

			datepickerBillDate.Binding
				.AddBinding(ViewModel.Entity, e => e.BillDate, w => w.DateOrNull)
				.InitializeFromSource();

			datepickerBillDate.IsEditable = true;

			spinBillSum.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Sum, w => w.ValueAsDecimal)
				.InitializeFromSource();

			speciallistcomboboxBillVat.ItemsList = Enum.GetValues(typeof(VAT));
			speciallistcomboboxBillVat.SetRenderTextFunc<VAT>(node => node.GetEnumTitle());

			speciallistcomboboxBillVat.Binding
				.AddBinding(ViewModel.Entity, e => e.VatType, w => w.SelectedItem)
				.InitializeFromSource();

			ytextviewPurpose.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentPurpose, w => w.Buffer.Text)
				.InitializeFromSource();

			smallfileinformationsview2.ViewModel = ViewModel.AttachedFileInformationsViewModel;

			InitializeComments();

			// Правая колонка

			InitializePayments();

			spinBillSum1.IsEditable = false;
			spinBillSum1.Binding
				.AddBinding(ViewModel, vm => vm.SumGiven, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinBillSum2.IsEditable = false;
			spinBillSum2.Binding
				.AddBinding(ViewModel, vm => vm.SumRemaining, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ycheckbuttonImidiatelyBill.Binding
				.AddBinding(ViewModel.Entity, e => e.IsImidiatelyBill, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonCreateGiveOutSchedule.Binding
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Active)
				.InitializeFromSource();

			speciallistcomboboxRepeatIntervalType.ItemsList = Enum.GetValues(typeof(RepeatIntervalTypes));
			speciallistcomboboxRepeatIntervalType.SetRenderTextFunc<RepeatIntervalTypes>(node => node.GetEnumTitle());

			labelRepeatIntervalType.Binding
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			speciallistcomboboxRepeatIntervalType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.RepeatIntervalType, w => w.SelectedItem)
				.AddBinding(vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			ylabelRepeatsCount.Binding
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			spinRepeatsCount.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.RepeatsCount, w => w.ValueAsInt)
				.AddBinding(vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			ylabelRepeatInterval.Binding
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			spinIntervals.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Intervals, w => w.ValueAsInt)
				.AddBinding(vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			buttonSave1.Binding
				.AddBinding(ViewModel, vm => vm.CreateGiveOutSchedule, w => w.Visible)
				.InitializeFromSource();

			// Кнопки

			buttonSave.Clicked += (s, a) => ViewModel.Save(true);
			buttonSave.Sensitive = !ViewModel.IsSecurityServiceRole;
			buttonCancel.Clicked += (s, a) => ViewModel.Close(ViewModel.AskSaveOnClose, CloseSource.Cancel);

			buttonPayout.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanPayout, w => w.Visible)
				.InitializeFromSource();
			buttonPayout.Clicked += (s, a) => ViewModel.Payout();

			btnAccept.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Visible)
				.InitializeFromSource();
			btnAccept.Clicked += (s, a) => ViewModel.Accept();

			btnApprove.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanApprove, w => w.Visible)
				.InitializeFromSource();
			btnApprove.Clicked += (s, a) => ViewModel.Approve();

			btnCancel.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanCancel, w => w.Visible)
				.InitializeFromSource();
			btnCancel.Clicked += (s, a) => ViewModel.Cancel();

			btnReapprove.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanReapprove, w => w.Visible)
				.InitializeFromSource();
			btnReapprove.Clicked += (s, a) => ViewModel.Reapprove();

			btnConveyForPayout.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanConveyForPayout, w => w.Visible)
				.InitializeFromSource();
			btnConveyForPayout.Clicked += (s, a) => ViewModel.ConveyForPayout();
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
			ytreeviewComments.RowActivated += YtreeviewComments_RowActivated;

			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.NewCommentText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAddComment.Clicked += (sender, e) => ViewModel.AddCommentCommand.Execute();
			ybuttonAddComment.Binding.AddBinding(ViewModel, vm => vm.CanAddComment, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void InitializePayments()
		{
			ytreeview1.CreateFluentColumnsConfig<Payment>()
				.AddColumn("Номер")
				.AddNumericRenderer(node => node.PaymentNum)
				.AddColumn("Время")
				.AddDateRenderer(node => node.Date)
				.AddColumn("Сумма")
				.AddNumericRenderer(node => node.PaymentItems.Sum(pi => pi.Sum))
				.Finish();

			ytreeview1.ItemsDataSource = ViewModel.Entity.Payments;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.AttachedFileInformationsViewModel))
			{
				smallfileinformationsview2.ViewModel = ViewModel.AttachedFileInformationsViewModel;
			}
		}

		private void YtreeviewComments_RowActivated(object o, RowActivatedArgs args)
		{
			if(!(ytreeviewComments.GetSelectedObject() is ComplaintDiscussionCommentFileInformation complaintDiscussionCommentFileInformation))
			{
				return;
			}
			ViewModel.OpenFileCommand.Execute(complaintDiscussionCommentFileInformation);
		}

		private string GetNodeName(object node)
		{
			if(node is ComplaintDiscussionComment complaintDiscussionComment)
			{
				return complaintDiscussionComment.Comment;
			}
			if(node is ComplaintDiscussionCommentFileInformation complaintDiscussionCommentFileInformation)
			{
				return complaintDiscussionCommentFileInformation.FileName;
			}
			return "";
		}

		private string GetTime(object node)
		{
			if(node is ComplaintDiscussionComment)
			{
				return (node as ComplaintDiscussionComment).CreationTime.ToShortDateString() + "\n" + (node as ComplaintDiscussionComment).CreationTime.ToShortTimeString();
			}

			return "";
		}

		private string GetAuthor(object node)
		{
			if(node is ComplaintDiscussionComment)
			{
				var author = (node as ComplaintDiscussionComment).Author;
				var subdivisionName = author.Subdivision != null && !string.IsNullOrWhiteSpace(author.Subdivision.ShortName) ? "\n" + author.Subdivision.ShortName : "";
				var result = $"{author.GetPersonNameWithInitials()}{subdivisionName}";
				return result;
			}
			return "";
		}

		private void SetColor(CellRenderer cell, object node)
		{
			if(node is ComplaintDiscussionComment)
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
			ytreeviewComments.RowActivated -= YtreeviewComments_RowActivated;

			base.Destroy();
		}
	}
}
