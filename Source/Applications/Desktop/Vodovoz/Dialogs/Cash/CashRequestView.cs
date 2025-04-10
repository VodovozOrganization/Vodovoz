using Gamma.Utilities;
using Gtk;
using QS.Utilities;
using QS.Utilities.Text;
using QS.Views.GtkUI;
using System;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Dialogs.Cash
{
	public partial class CashRequestView : TabViewBase<CashRequestViewModel>
	{
		public CashRequestView(CashRequestViewModel viewModel) : base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			//Автор
			entryAuthor.ViewModel = ViewModel.AuthorViewModel;
			entryAuthor.Sensitive = false;

			//Подразделение

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;
			entrySubdivision.Sensitive = false;

			//Причина расхода

			entryFinancialExpenceCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			#region Combo

			//Организация
			speccomboOrganization.SetRenderTextFunc<Organization>(s => s.Name);
			var orgList = ViewModel.UoW.Session.QueryOver<Organization>().List();
			speccomboOrganization.ItemsList = orgList;
			speccomboOrganization.Binding
				.AddBinding(ViewModel.Entity, x => x.Organization, x => x.SelectedItem)
				.InitializeFromSource();

			if(speccomboOrganization.SelectedItem == null)
			{
				speccomboOrganization.SelectedItem = orgList.First();
			}

			//Смена ролей
			comboRoleChooser.SetRenderTextFunc<PayoutRequestUserRole>(ur => ur.GetEnumTitle());
			comboRoleChooser.ItemsList = ViewModel.UserRoles;
			comboRoleChooser.Binding
				.AddBinding(ViewModel, vm => vm.UserRole, w => w.SelectedItem)
				.InitializeFromSource();
			comboRoleChooser.Sensitive = ViewModel.IsRoleChooserSensitive;

			#endregion

			#region TextEntry

			//Пояснение
			yentryExplanation.Binding
				.AddBinding(ViewModel.Entity, e => e.Explanation, (widget) => widget.Text)
				.InitializeFromSource();

			//Основание
			yentryGround.Binding
				.AddBinding(ViewModel.Entity, e => e.Basis, (widget) => widget.Buffer.Text)
				.InitializeFromSource();
			yentryGround.WrapMode = WrapMode.Word;

			//Причина отмены
			yentryCancelReason.Binding
				.AddBinding(ViewModel.Entity, e => e.CancelReason, (widget) => widget.Buffer.Text)
				.InitializeFromSource();
			yentryCancelReason.WrapMode = WrapMode.Word;

			//Причина отправки на пересогласование
			yentryReasonForSendToReapproval.Binding
				.AddBinding(ViewModel.Entity, e => e.ReasonForSendToReappropriate, (widget) => widget.Buffer.Text)
				.InitializeFromSource();
			yentryReasonForSendToReapproval.WrapMode = WrapMode.Word;

			#endregion TextEntry

			#region Buttons

			ybtnAccept.BindCommand(ViewModel.AcceptCommand);
			ybtnApprove.BindCommand(ViewModel.ApproveCommand);
			ybtnSubdivisionChiefApprove.BindCommand(ViewModel.SubdivisionChiefApproveCommand);
			ybtnFinancialResponsibilityCenterApprove.BindCommand(ViewModel.AgreeByFinancialResponsibilityCenterCommand);
			ybtnFinancialResponsibilityCenterApprove.Binding
				.AddBinding(ViewModel, vm => vm.CanAgreeByFinancialResponsibilityCenter, w => w.Visible)
				.InitializeFromSource();
			ybtnCancel.BindCommand(ViewModel.CancelRequestCommand);
			//Передать на выдачу
			ybtnConveyForResults.BindCommand(ViewModel.ConveyForResultsCommand);
			//Отправить на пересогласование
			ybtnReturnForRenegotiation.BindCommand(ViewModel.ReturnToRenegotiationCommand);

			ybtnGiveSumm.BindCommand(ViewModel.GiveSumCommand);

			ybtnGiveSumm.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSeeGiveSum, w => w.Visible)
				.InitializeFromSource();

			ybtnGiveSummPartially.BindCommand(ViewModel.GiveSumPartiallyCommand);

			ybtnGiveSummPartially.Binding
				.AddBinding(ViewModel, vm => vm.CanSeeGiveSum, w => w.Visible)
				.InitializeFromSource();

			yspinGivePartially.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SumForPartiallyGive, w => w.ValueAsDecimal)
				.AddBinding(vm => vm.CanSeeGiveSum, w => w.Visible)
				.InitializeFromSource();

			ybtnAddSumm.Binding
				.AddBinding(ViewModel, vm => vm.CanAddSum, w => w.Sensitive)
				.InitializeFromSource();

			ybtnAddSumm.BindCommand(ViewModel.AddSumCommand);
			ybtnEditSum.BindCommand(ViewModel.EditSumCommand);
			ybtnDeleteSumm.BindCommand(ViewModel.RemoveSumCommand);

			ybtnEditSum.Binding
				.AddBinding(ViewModel, vm => vm.CanEditSumSensitive, w => w.Sensitive)
				.InitializeFromSource();

			//Visible
			ybtnAccept.Binding
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Visible)
				.InitializeFromSource();

			ybtnSubdivisionChiefApprove.Binding
				.AddBinding(ViewModel, vm => vm.CanSubdivisionChiefApprove, w => w.Visible)
				.InitializeFromSource();

			ybtnApprove.Binding
				.AddBinding(ViewModel, vm => vm.CanApprove, w => w.Visible)
				.InitializeFromSource();

			ybtnCancel.Binding
				.AddBinding(ViewModel, vm => vm.CanCancel, w => w.Visible)
				.InitializeFromSource();

			ybtnConveyForResults.Binding
				.AddBinding(ViewModel, vm => vm.CanConveyForResults, w => w.Visible)
				.InitializeFromSource();

			ybtnReturnForRenegotiation.Binding
				.AddBinding(ViewModel, vm => vm.CanReturnToRenegotiation, w => w.Visible)
				.InitializeFromSource();

			ybtnDeleteSumm.Binding
				.AddBinding(ViewModel, vm => vm.CanDeleteSum, w => w.Visible)
				.InitializeFromSource();

			ybtnEditSum.Visible = false;
			buttonSave.BindCommand(ViewModel.AfterSaveCommand);
			buttonSave.Sensitive = !ViewModel.IsSecurityServiceRole;

			buttonCancel.BindCommand(ViewModel.CloseCommand);

			ycheckPossibilityNotToReconcilePayments.Binding
				.AddBinding(ViewModel.Entity, e => e.PossibilityNotToReconcilePayments, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanConfirmPossibilityNotToReconcilePayments, w => w.Visible)
				.InitializeFromSource();

			ylabelPossibilityNotToReconcilePayments.Binding
				.AddBinding(ViewModel, vm => vm.CanConfirmPossibilityNotToReconcilePayments, w => w.Visible)
				.InitializeFromSource();

			#endregion Buttons

			#region Editibility

			yentryCancelReason.Binding
				.AddBinding(ViewModel, vm => vm.CanEditOnlyCoordinator, w => w.Sensitive)
				.InitializeFromSource();

			entryFinancialExpenceCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryFinancialExpenceCategory.Binding
				.AddBinding(ViewModel, vm => vm.ExpenseCategorySensitive, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.ExpenseCategoryVisibility, w => w.Visible)
				.InitializeFromSource();

			speccomboOrganization.Binding
				.AddBinding(ViewModel, vm => vm.SensitiveForFinancier, w => w.Sensitive)
				.InitializeFromSource();

			#endregion Editibility

			#region Visibility

			labelBalansOrganizations.Binding
				.AddBinding(ViewModel, vm => vm.VisibleOnlyForFinancer, w => w.Visible)
				.InitializeFromSource();
			ylabelBalansOrganizations.Binding
				.AddBinding(ViewModel, vm => vm.VisibleOnlyForFinancer, w => w.Visible)
				.InitializeFromSource();

			labelcomboOrganization.Binding
				.AddBinding(ViewModel, vm => vm.VisibleOnlyForFinancer, w => w.Visible)
				.InitializeFromSource();
			speccomboOrganization.Binding
				.AddBinding(ViewModel, vm => vm.VisibleOnlyForFinancer, w => w.Visible)
				.InitializeFromSource();

			labelCategoryEntityviewmodelentry.Binding
				.AddBinding(ViewModel, vm => vm.ExpenseCategoryVisibility, w => w.Visible)
				.InitializeFromSource();

			yentryReasonForSendToReapproval.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;
			labelReasonForSendToReapproval.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;

			yentryCancelReason.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;
			labelCancelReason.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;

			if(ViewModel.Entity.PayoutRequestState == PayoutRequestState.New)
			{
				hseparator1.Visible = false;
				hseparator2.Visible = false;
				hseparator3.Visible = false;
			}

			#endregion Visibility

			ConfigureTreeView();

			ycheckHaveReceipt.Binding
				.AddBinding(ViewModel.Entity, e => e.HaveReceipt, w => w.Active)
				.InitializeFromSource();

			ylabelBalansOrganizations.Text = ViewModel.LoadOrganizationsSums();

			ylabelStatus.Binding
				.AddBinding(ViewModel, vm => vm.StateName, w => w.Text)
				.InitializeFromSource();

			if(ViewModel.Entity.PayoutRequestState == PayoutRequestState.Closed
				|| ViewModel.IsSecurityServiceRole)
			{
				ytreeviewSums.Sensitive = false;
				ybtnAccept.Sensitive = false;
				ybtnApprove.Sensitive = false;
				ybtnCancel.Sensitive = false;
				ybtnDeleteSumm.Sensitive = false;
				ybtnEditSum.Sensitive = false;
				ybtnConveyForResults.Sensitive = false;
				ybtnReturnForRenegotiation.Sensitive = false;
				speccomboOrganization.Sensitive = false;
				yentryExplanation.Sensitive = false;
				yentryGround.Sensitive = false;
				yentryCancelReason.Sensitive = false;
				yentryReasonForSendToReapproval.Sensitive = false;
			}
		}

		private void ConfigureTreeView()
		{
			ytreeviewSums.CreateFluentColumnsConfig<CashRequestSumItem>()
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => CurrencyWorks.GetShortCurrencyString(n.Sum))
					.XAlign(0.5f)
				.AddColumn("Остаток на выдачу")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n =>
						CurrencyWorks.GetShortCurrencyString(n.Sum - n.Expenses.Sum(e => e.Money)))
					.XAlign(0.5f)
				.AddColumn("Дата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Date.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Подотчетное лицо")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.AccountableEmployee != null
						? PersonHelper.PersonNameWithInitials(
							n.AccountableEmployee.LastName,
							n.AccountableEmployee.Name,
							n.AccountableEmployee.Patronymic)
						: "")
					.XAlign(0.5f)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Comment)
					.XAlign(0.5f)
				.RowCells().AddSetter<CellRenderer>((c, n) => c.Sensitive = ViewModel.CanExecuteGive)
				.Finish();

			ytreeviewSums.ItemsDataSource = ViewModel.Entity.ObservableSums;
			ytreeviewSums.Selection.Changed += OnyTreeViewSumsSelectionChanged;
			ytreeviewSums.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.SelectedCashRequestSumItemObject, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void OnyTreeViewSumsSelectionChanged(object sender, EventArgs e)
		{
			var isSensetive = ytreeviewSums.Selection.CountSelectedRows() > 0;
			if(isSensetive)
			{
				ViewModel.SelectedItem = ytreeviewSums.GetSelectedObject<CashRequestSumItem>();
				ybtnDeleteSumm.Sensitive = isSensetive;
				//Редактировать можно только невыданные
				ybtnEditSum.Visible = ViewModel.SelectedItem != null && !ViewModel.SelectedItem.ObservableExpenses.Any();
				yspinGivePartially.SetRange(
					0,
					(double)(ViewModel.SelectedItem.Sum - ViewModel.SelectedItem?.ObservableExpenses.Sum(x => x.Money) ?? 0));
			}
		}
	}
}
