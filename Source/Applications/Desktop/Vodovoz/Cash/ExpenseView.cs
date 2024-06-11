using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class ExpenseView : TabViewBase<ExpenseViewModel>
	{
		public ExpenseView(ExpenseViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			if(!accessfilteredsubdivisionselectorwidget.Configure(ViewModel.UoW, false, typeof(Expense)))
			{
				ViewModel.InitializationFailed("Ошибка", accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				return;
			}

			accessfilteredsubdivisionselectorwidget.OnSelected += (_, _2) => UpdateSubdivision();

			if(ViewModel.Entity.RelatedToSubdivision != null)
			{
				accessfilteredsubdivisionselectorwidget.SelectIfPossible(ViewModel.Entity.RelatedToSubdivision);
			}

			if(!ViewModel.CanEdit)
			{
				accessfilteredsubdivisionselectorwidget.Sensitive = false;
			}

			enumcomboOperation.ItemsEnum = typeof(ExpenseType);
			enumcomboOperation.Binding
				.AddBinding(ViewModel.Entity, s => s.TypeOperation, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			entryCashier.ViewModel = ViewModel.CashierViewModel;

			entryRouteList.ViewModel = ViewModel.RouteListViewModel;
			entryRouteList.Binding
				.AddBinding(ViewModel, vm => vm.IsAdvance, w => w.Visible)
				.InitializeFromSource();

			ylabelRouteList.Binding
				.AddBinding(ViewModel, vm => vm.IsAdvance, w => w.Visible)
				.InitializeFromSource();

			entryEmployee.ViewModel = ViewModel.EmployeeViewModel;

			entryEmployee.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeEmployee, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			ydateDocument.Binding
				.AddBinding(ViewModel.Entity, s => s.Date, w => w.Date)
				.AddBinding(ViewModel, vm => vm.CanEditDate, w => w.Sensitive)
				.InitializeFromSource();

			ydateDdr.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DdrDate, w => w.Date)
				.AddBinding(vm => vm.CanEditDdrDate, w => w.IsEditable)
				.InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			specialListCmbOrganisation.ShowSpecialStateNot = true;

			specialListCmbOrganisation.Binding
				.AddBinding(ViewModel, vm => vm.CachedOrganizations, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.OrganisationVisible, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.Organisation, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			ylabel1.Binding
				.AddBinding(ViewModel, vm => vm.OrganisationVisible, w => w.Visible)
				.InitializeFromSource();

			yspinMoney.CurrencyFormat = true;
			yspinMoney.Binding
				.AddBinding(ViewModel, vm => vm.Money, w => w.ValueAsDecimal)
				.InitializeFromSource();

			currencylabel1.Binding
				.AddBinding(ViewModel, vm => vm.CurrencySymbol, w => w.Text)
				.InitializeFromSource();

			ytextviewDescription.Binding
				.AddBinding(ViewModel.Entity, s => s.Description, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Editable)
				.InitializeFromSource();

			labelEmployee.Binding
				.AddBinding(ViewModel, vm => vm.EmployeeTypeString, w => w.Text)
				.InitializeFromSource();

			ylabelEmployeeWageBalance.Binding
				.AddBinding(ViewModel, vm => vm.CurrentEmployeeWageBalanceLabelString, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.EmployeeBalanceVisible, w => w.Visible)
				.InitializeFromSource();

			UpdateSubdivision();

			table1.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += (_, _2) => ViewModel.SaveCommand.Execute();

			buttonCancel.Clicked += (_, _2) => ViewModel.CloseCommand.Execute();

			buttonPrint.Binding.InitializeFromSource();

			buttonPrint.Clicked += (_, _2) => ViewModel.PrintCommand.Execute();
		}

		private void UpdateSubdivision()
		{
			if(accessfilteredsubdivisionselectorwidget.SelectedSubdivision != null
				&& accessfilteredsubdivisionselectorwidget.NeedChooseSubdivision)
			{
				ViewModel.Entity.RelatedToSubdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
			}
		}
	}
}
