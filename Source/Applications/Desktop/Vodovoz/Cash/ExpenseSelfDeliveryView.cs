using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Cash;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class ExpenseSelfDeliveryView : TabViewBase<ExpenseSelfDeliveryViewModel>
	{
		public ExpenseSelfDeliveryView(ExpenseSelfDeliveryViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			if(!accessfilteredsubdivisionselectorwidget.Configure(ViewModel.UoW, false, typeof(Expense)))
			{
				ViewModel.InitializationFailed("Ошибка",
					accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				return;
			}

			accessfilteredsubdivisionselectorwidget.OnSelected += (_, _2) => UpdateSubdivision();

			UpdateSubdivision();

			accessfilteredsubdivisionselectorwidget.Sensitive = ViewModel.CanEdit;

			ViewModel.Entity.RelatedToSubdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;

			entryCashier.ViewModel = ViewModel.CashierViewModel;

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			var clientEntryViewModelBuilder = new LegacyEEVMBuilderFactory<Expense>(
				Tab,
				ViewModel.Entity,
				ViewModel.UoW,
				ViewModel.NavigationManager,
				ViewModel.Scope);

			ViewModel.OrderViewModel = clientEntryViewModelBuilder.ForProperty(x => x.Order)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<OrderJournalViewModel>()
				.Finish();

			entryOrder.ViewModel = ViewModel.OrderViewModel;

			ydateDocument.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();

			currencylabel1.Binding
				.AddBinding(ViewModel, vm => vm.CurrencySymbol, w => w.Text)
				.InitializeFromSource();

			enumcomboOperation.ItemsEnum = typeof(ExpenseType);
			enumcomboOperation.Binding
				.AddBinding(ViewModel.Entity, s => s.TypeOperation, w => w.SelectedItem)
				.InitializeFromSource();
			
			enumcomboOperation.Sensitive = false;

			yspinMoney.CurrencyFormat = true;

			yspinMoney.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Money, w => w.ValueAsDecimal)
				.InitializeFromSource();

			table1.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ytextviewDescription.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += (_, _2) => ViewModel.SaveCommand.Execute();
			buttonCancel.Clicked += (_, _2) => ViewModel.CloseCommand.Execute();
			buttonPrint.Clicked += (_, _2) => ViewModel.PrintCommand.Execute();
		}

		private void UpdateSubdivision()
		{
			if(accessfilteredsubdivisionselectorwidget.SelectedSubdivision != null && accessfilteredsubdivisionselectorwidget.NeedChooseSubdivision)
			{
				ViewModel.Entity.RelatedToSubdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
			}
		}
	}
}
