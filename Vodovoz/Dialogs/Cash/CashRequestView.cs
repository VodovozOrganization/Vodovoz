using System;
using System.Linq;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashRequestView : TabViewBase<CashRequestViewModel>
	{
		public CashRequestView(CashRequestViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			#region EntityViewModelEntry

			//Автор
			var currentEmployee = ViewModel.CurrentEmployee;
			AuthorEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() =>
					{
						var employeeFilter = new EmployeeFilterViewModel{
							Status = EmployeeStatus.IsWorking,
						};
						return new EmployeesJournalViewModel(
							employeeFilter,
							UnitOfWorkFactory.GetDefaultFactory, 
							ServicesConfig.CommonServices);
					})
			);
			AuthorEntityviewmodelentry.Binding.AddBinding(ViewModel.Entity, x => x.Author, w => w.Subject).InitializeFromSource();

			if (ViewModel.IsNewEntity)
			{
				ViewModel.Entity.Author = currentEmployee;
			}
			AuthorEntityviewmodelentry.Sensitive = false;
			
			//Подразделение
			var employeeSelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory
					<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(ServicesConfig.CommonServices);

			var filter = new SubdivisionFilterViewModel() { SubdivisionType = SubdivisionType.Default };
			
			SubdivisionEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<SubdivisionsJournalViewModel>(
					typeof(Subdivision), 
					() => new SubdivisionsJournalViewModel(
						filter,
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						employeeSelectorFactory
					)
				)
			);
			SubdivisionEntityviewmodelentry.Binding
				.AddBinding(
					ViewModel.Entity, 
					s => s.Subdivision, 
					w => w.Subject)
				.InitializeFromSource();
			SubdivisionEntityviewmodelentry.Sensitive = false;
			ViewModel.Entity.Subdivision = currentEmployee.Subdivision;
			
			//Причина расхода
			ExpenseCategoryEntityviewmodelentry
				.SetEntityAutocompleteSelectorFactory(ViewModel.ExpenseCategoryAutocompleteSelectorFactory);
			
			ExpenseCategoryEntityviewmodelentry.Binding.AddBinding(
				ViewModel.Entity, 
				s => s.ExpenseCategory, 
				w => w.Subject)
				.InitializeFromSource();
			
			ExpenseCategoryEntityviewmodelentry.CanEditReference = true;
			
			#endregion EntityViewModelEntry

			#region Combo
			
			//Организация
			speccomboOrganization.SetRenderTextFunc<Organization>(s => s.Name);
			var orgList = ViewModel.UoW.Session.QueryOver<Organization>().List();
			speccomboOrganization.ItemsList = orgList;
			speccomboOrganization.Binding.AddBinding(
					ViewModel.Entity, 
					x => x.Organization, 
					x => x.SelectedItem)
				.InitializeFromSource();
			speccomboOrganization.SelectedItem = orgList.First();
			
			//Смена ролей для админов   
			comboIfAdminRoleChooser.ItemsEnum = typeof(UserRole);
			comboIfAdminRoleChooser.Binding.AddBinding(
				ViewModel,
				e => e.UserRole,
				w => w.SelectedItem);
			comboIfAdminRoleChooser.Visible = ViewModel.IsAdminPanelVisible;
			comboIfAdminRoleChooser.Visible = false;

			#endregion

			#region TextEntry

			//Пояснение
			yentryExplanation.Binding
				.AddBinding(
					ViewModel.Entity, 
					e => e.Explanation, 
					(widget) => widget.Text)
				.InitializeFromSource();

			//Основание
			yentryGround.Binding
				.AddBinding(
					ViewModel.Entity,
					e => e.Basis, 
					(widget) => widget.Text)
				.InitializeFromSource();
			
			//Причина отмены
			yentryCancelReason.Binding
				.AddBinding(
					ViewModel.Entity, 
					e => e.CancelReason, 
					(widget) => widget.Text)
				.InitializeFromSource();
			
			//Причина отправки на пересогласование
			yentryReasonForSendToReapproval.Binding
				.AddBinding(
					ViewModel.Entity, 
					e => e.ReasonForSendToReappropriate, 
					(widget) => widget.Text)
				.InitializeFromSource();

			#endregion TextEntry

			#region Buttons
			ybtnAccept.Clicked += (sender, args) =>
			{
				ViewModel.AcceptCommand.Execute();
				ybtnAccept.Visible = false;
			};
			ybtnApprove.Clicked += (sender, args) => ViewModel.ApproveCommand.Execute();
			ybtnCancel.Clicked += (sender, args) => ViewModel.CancelCommand.Execute();
			//Передать на выдачу
			ybtnConveyForResults.Clicked += (sender, args) => ViewModel.ConveyForResultsCommand.Execute();
			//Отправить на пересогласование
			ybtnReturnForRenegotiation.Clicked += (sender, args) => ViewModel.ReturnToRenegotiationCommand.Execute();
			
			ybtnGiveSumm.Clicked += (sender, args) => ViewModel.GiveSumCommand.Execute();
			ybtnGiveSumm.Binding.AddBinding(ViewModel, vm => vm.CanGiveSum, w => w.Visible).InitializeFromSource();
			ybtnGiveSumm.Sensitive = false;
			
			ybtnAddSumm.Clicked += (sender, args) => ViewModel.AddSumCommand.Execute();
			ybtnEditSum.Clicked += (sender, args) => ViewModel.EditSumCommand.Execute();
			ybtnDeleteSumm.Clicked += (sender, args) => ViewModel.DeleteSumCommand.Execute();
			ybtnEditSum.Binding.AddBinding(ViewModel, vm => vm.CanEditSumSensitive, w => w.Sensitive).InitializeFromSource();
			
			//Visible
			ybtnAccept.Binding.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Visible).InitializeFromSource();
			ybtnApprove.Binding.AddBinding(ViewModel, vm => vm.CanApprove, w => w.Visible).InitializeFromSource();
			ybtnCancel.Binding.AddBinding(ViewModel, vm => vm.CanCancel, w => w.Visible).InitializeFromSource();
			ybtnConveyForResults.Binding.AddBinding(ViewModel, vm => vm.CanConveyForResults, w => w.Visible).InitializeFromSource();
			ybtnReturnForRenegotiation.Binding.AddBinding(ViewModel, vm => vm.CanReturnToRenegotiation, w => w.Visible).InitializeFromSource();
			ybtnDeleteSumm.Binding.AddBinding(ViewModel, vm => vm.CanDeleteSum, w => w.Visible).InitializeFromSource();
			ybtnEditSum.Visible = false;
			buttonSave.Clicked += (sender, args) => ViewModel.AfterSaveCommand.Execute();
			buttonCancel.Clicked += (s, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };
			
			#endregion

			#region Editibility

			yentryCancelReason.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyCoordinator, w => w.Sensitive).InitializeFromSource();
			ExpenseCategoryEntityviewmodelentry.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNAGandRoldFinancier, w => w.Sensitive).InitializeFromSource();
			speccomboOrganization.Binding.AddBinding(ViewModel, vm => vm.CanEditOnlyinStateNAGandRoldFinancier, w => w.Sensitive).InitializeFromSource();

			#endregion Editibility

			#region Visibility

			labelBalansOrganizations.Visible = ViewModel.VisibleOnlyForFinancer;
			ylabelBalansOrganizations.Visible = ViewModel.VisibleOnlyForFinancer;
			
			speccomboOrganization.Visible = ViewModel.VisibleOnlyForFinancer;
			labelcomboOrganization.Visible = ViewModel.VisibleOnlyForFinancer;
			
			ExpenseCategoryEntityviewmodelentry.Visible = ViewModel.VisibleOnlyForFinancer;
			labelCategoryEntityviewmodelentry.Visible = ViewModel.VisibleOnlyForFinancer;
			
			yentryReasonForSendToReapproval.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;
			labelReasonForSendToReapproval.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;
			
			yentryCancelReason.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;
			labelCancelReason.Visible = ViewModel.VisibleOnlyForStatusUpperThanCreated;

			if (ViewModel.UserRole == UserRole.RequestCreator)
			{
				hseparator1.Visible = false;
				hseparator2.Visible = false;
				hseparator3.Visible = false;
			}
			
			#endregion Visibility

			#region TreeView

			ConfigureTreeView();
			
			#endregion

			ycheckHaveReceipt.Binding.AddBinding(
				ViewModel.Entity, 
				e => e.HaveReceipt,
				w => w.Active)
			.InitializeFromSource();

			ylabelBalansOrganizations.Text = ViewModel.LoadOrganizationsSums();

			// ylabelRole.Binding.AddBinding(ViewModel, vm => vm.UserRole.GetEnumTitle(), w => w.Text);
			ylabelRole.Text = ViewModel.UserRole.GetEnumTitle();
			ylabelStatus.Text = ViewModel.Entity.State.GetEnumTitle();
		}

		private void ConfigureTreeView()
		{
			ytreeviewSums.CreateFluentColumnsConfig<CashRequestSumItem>()
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => n.Sum)
					.XAlign(0.5f)
				.AddColumn("Дата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Date.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Подотчетное лицо")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.AccountableEmployee.Name)
					.XAlign(0.5f)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Comment)
					.XAlign(0.5f)
				.AddColumn("Выдано")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(n => n.Expense != null).Editing(false)
					
				.Finish();
			
			ytreeviewSums.ItemsDataSource = ViewModel.Entity.ObservableSums;
			ytreeviewSums.Selection.Changed += OnyTreeViewSumsSelectionChanged;
			ytreeviewSums.Binding.AddBinding(
				ViewModel, 
				vm => vm.CanEdit, 
				w => w.Sensitive
			).InitializeFromSource();
			
			ViewModel.UpdateNodes += ytreeviewSums.YTreeModel.EmitModelChanged;
		}

		public void ModelChanged()
		{
			ytreeviewSums.YTreeModel.EmitModelChanged();
		}

		private void OnyTreeViewSumsSelectionChanged(object sender, EventArgs e)
		{
			bool isSensetive = ytreeviewSums.Selection.CountSelectedRows() > 0;
			if (isSensetive){
				ViewModel.SelectedItem = ytreeviewSums.GetSelectedObject<CashRequestSumItem>();
				ybtnDeleteSumm.Sensitive = isSensetive;
				ybtnGiveSumm.Sensitive = ViewModel.SelectedItem != null && ViewModel.SelectedItem.Expense == null;
				//Редактировать можно только невыданные
				ybtnEditSum.Visible = ViewModel.SelectedItem != null && ViewModel.SelectedItem.Expense == null;
			}
			
		}
	}
}
