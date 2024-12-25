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
			comboRoleChooser.SetRenderTextFunc<PayoutRequestUserRole>(ur => ur.GetEnumTitle());
			comboRoleChooser.ItemsList = ViewModel.UserRoles;
			comboRoleChooser.Binding
				.AddBinding(ViewModel, vm => vm.UserRole, w => w.SelectedItem)
				.InitializeFromSource();

			comboRoleChooser.Sensitive = ViewModel.IsRoleChooserSensitive;

			labelStatus.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.PayoutRequestState.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();

			entryAuthorEmployee.ViewModel = ViewModel.AuthorViewModel;

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			ViewModel.CounterpartyViewModel = new LegacyEEVMBuilderFactory<CashlessRequest>(
					ViewModel,
					ViewModel.Entity,
					ViewModel.UoW,
					ViewModel.NavigationManager,
					_lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			entryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			entryCounterparty.Binding
				.AddFuncBinding(
					ViewModel,
					vm => vm.IsNotClosed && !vm.IsSecurityServiceRole,
					w => w.Sensitive)
				.InitializeFromSource();

			//spinSum.Binding
			//	.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
			//	.AddBinding(ViewModel.Entity, e => e.Sum, w => w.ValueAsDecimal)
			//	.InitializeFromSource();

			//checkNotToReconcile.Binding
			//.AddBinding(ViewModel, vm => vm.CanSeeNotToReconcile, w => w.Visible)
			//.AddBinding(ViewModel.Entity, e => e.PossibilityNotToReconcilePayments, w => w.Active)
			//.InitializeFromSource();

			ViewModel.OrganizationViewModel = new LegacyEEVMBuilderFactory<CashlessRequest>(
					ViewModel,
					ViewModel.Entity,
					ViewModel.UoW,
					ViewModel.NavigationManager,
					_lifetimeScope)
				.ForProperty(x => x.Organization)
				.UseTdiDialog<OrganizationDlg>()
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.Finish();

			entryOrganization.ViewModel = ViewModel.OrganizationViewModel;

			entryOrganization.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSetOrganisaton && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanSeeOrganisation, w => w.Visible)
				.InitializeFromSource();

			labelExpenceCategory.Binding.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible).InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryExpenseFinancialCategory.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSetExpenseCategory && !vm.IsSecurityServiceRole, w => w.ViewModel.IsEditable)
				.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible)
				.InitializeFromSource();

			//entryBasis.Binding
			//	.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
			//	.AddBinding(ViewModel.Entity, e => e.Basis, w => w.Buffer.Text)
			//	.InitializeFromSource();
			//entryExplanation.Binding
			//.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
			//.AddBinding(ViewModel.Entity, e => e.Explanation, w => w.Buffer.Text)
			//.InitializeFromSource();

			//eventBoxReasonsSeparator.Binding
			//	.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
			//	.InitializeFromSource();
			//eventBoxCancelReason.Binding
			//	.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
			//	.InitializeFromSource();
			//labelCancelReason.Binding
			//	.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
			//	.InitializeFromSource();
			//entryCancelReason.Binding
			//	.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
			//	.AddBinding(ViewModel.Entity, e => e.CancelReason, w => w.Buffer.Text)
			//	.InitializeFromSource();

			//eventBoxWhySentToReapproval.Binding
			//	.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
			//	.InitializeFromSource();
			//labelWhySentToReapproval.Binding
			//	.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
			//	.InitializeFromSource();
			//entryWhySentToReapproval.Binding
			//.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
			//.AddBinding(ViewModel.Entity, e => e.ReasonForSendToReappropriate, w => w.Buffer.Text)
			//.InitializeFromSource();

			smallfileinformationsview2.ViewModel = ViewModel.AttachedFileInformationsViewModel;

			InitializeComments();

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
