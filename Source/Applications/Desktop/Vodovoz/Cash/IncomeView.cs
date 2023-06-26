using Gamma.GtkWidgets;
using Gtk;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.JournalViewModels;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class IncomeView : TabViewBase<IncomeViewModel>
	{
		public IncomeView(IncomeViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			if(!accessfilteredsubdivisionselectorwidget.Configure(ViewModel.UoW, false, typeof(Income)))
			{

				ViewModel.InitializationFailed("Ошибка",
					accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				return;
			}

			accessfilteredsubdivisionselectorwidget.OnSelected += (_, _2) => UpdateSubdivision();

			if(!ViewModel.CanEdit)
			{
				accessfilteredsubdivisionselectorwidget.Sensitive = false;
			}

			if(ViewModel.Entity.RelatedToSubdivision != null)
			{
				accessfilteredsubdivisionselectorwidget.SelectIfPossible(ViewModel.Entity.RelatedToSubdivision);
			}

			enumcomboOperation.ItemsEnum = typeof(IncomeType);
			enumcomboOperation.Binding
				.AddBinding(ViewModel.Entity, e => e.TypeOperation, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			entryCashier.ViewModel = ViewModel.CashierViewModel;
			entryCashier.Binding
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			entryEmployee.ViewModel = ViewModel.EmployeeViewModel;

			entryRouteList.ViewModel = ViewModel.RouteListViewModel;

			entryRouteList.Binding
				.AddBinding(ViewModel, vm => vm.ShowRouteList, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanChangeRouteList, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			lblRouteList.Binding
				.AddBinding(ViewModel, vm => vm.ShowRouteList, w => w.Visible)
				.InitializeFromSource();

			var clientEntryViewModelBuilder = new LegacyEEVMBuilderFactory<Income>(
				Tab,
				ViewModel.Entity,
				ViewModel.UoW,
				ViewModel.NavigationManager,
				ViewModel.Scope);

			ViewModel.ClientViewModel = clientEntryViewModelBuilder.ForProperty(x => x.Customer)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			entryClient.ViewModel = ViewModel.ClientViewModel;

			entryClient.Binding
				.AddBinding(ViewModel, vm => vm.IsPayment, w => w.Visible)
				.InitializeFromSource();

			ydateDocument.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.AddBinding(ViewModel, vm => vm.CanEditDate, w => w.Sensitive)
				.InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryExpenseFinancialCategory.Binding
				.AddBinding(ViewModel, vm => vm.IsReturnOperation, w => w.Visible)
				.InitializeFromSource();

			entryIncomeFinancialCategory.ViewModel = ViewModel.FinancialIncomeCategoryViewModel;

			entryIncomeFinancialCategory.Binding
				.AddBinding(ViewModel, vm => vm.IsNotReturnOperation, w => w.Visible)
				.InitializeFromSource();

			specialListCmbOrganisation.ShowSpecialStateNot = true;

			specialListCmbOrganisation.Binding
				.AddBinding(ViewModel, vm => vm.CachedOrganizations, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Organisation, w => w.SelectedItem)
				.InitializeFromSource();

			specialListCmbOrganisation.Binding
				.AddBinding(ViewModel, vm => vm.IsReturnOperation, w => w.Visible)
				.InitializeFromSource();

			checkNoClose.Binding
				.AddBinding(ViewModel, vm => vm.NoClose, w => w.Active)
				.InitializeFromSource();

			checkNoClose.Binding
				.AddBinding(ViewModel, vm => vm.IsReturnOperationOrNew, w => w.Visible)
				.InitializeFromSource();

			yspinMoney.CurrencyFormat = true;

			yspinMoney.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Money, w => w.ValueAsDecimal)
				.AddBinding(vm => vm.NoClose, w => w.Sensitive)
				.AddBinding(vm => vm.IsNotReturnOperation, w => w.Sensitive)
				.InitializeFromSource();

			currencylabel1.Binding
				.AddBinding(ViewModel, vm => vm.CurrencySymbol, w => w.Text)
				.InitializeFromSource();

			ytextviewDescription.Binding.AddBinding(ViewModel.Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource();

			ytreeviewDebts.ColumnsConfig = ColumnsConfigFactory.Create<SelectableNode<Expense>>()
				.AddColumn("Закрыть").AddToggleRenderer(a => a.Selected).Editing()
				.AddColumn("Дата").AddTextRenderer(a => a.Value.Date.ToString())
				.AddColumn("Получено").AddTextRenderer(a => a.Value.Money.ToString("C"))
				.AddColumn("Непогашено").AddTextRenderer(a => a.Value.UnclosedMoney.ToString("C"))
				.AddColumn("Статья").AddTextRenderer(a => a.Value.ExpenseCategoryId != null
					? ViewModel.GetCachedExpenseCategoryTitle(a.Value.ExpenseCategoryId.Value)
					: "")
				.AddColumn("Основание").AddTextRenderer(a => a.Value.Description)
				.RowCells().AddSetter<CellRenderer>(
					(cell, node) =>
					{
						cell.Sensitive =
							node.Value.RouteListClosing == ViewModel.Entity.RouteListClosing
							|| ViewModel.SelectableAdvances.Count(s => s.Selected) == 0;
					})
				.Finish();

			ytreeviewDebts.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.SelectableAdvances, w => w.ItemsDataSource)
				.InitializeFromSource();

			ViewModel.DebtsChanged += OnDebtsChanged;

			UpdateSubdivision();

			table1.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ytextviewDescription.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Clicked += (_, _2) => ViewModel.SaveCommand.Execute();

			buttonCancel.Clicked += (_, _2) => ViewModel.CloseCommand.Execute();

			buttonPrint.Binding
				.AddBinding(ViewModel, vm => vm.IsReturnOperation, w => w.Sensitive)
				.InitializeFromSource();

			buttonPrint.Clicked += (_, _2) => ViewModel.PrintCommand.Execute();

			labelClientTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsPayment, w => w.Visible)
				.InitializeFromSource();

			labelExpenseTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsReturnOperation, w => w.Visible)
				.InitializeFromSource();

			ylabel1.Binding
				.AddBinding(ViewModel, vm => vm.IsReturnOperation, w => w.Visible)
				.InitializeFromSource();

			labelIncomeTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsNotReturnOperation, w => w.Visible)
				.InitializeFromSource();

			vboxDebts.Binding
				.AddBinding(ViewModel, vm => vm.IsReturnOperationOrNew, w => w.Visible)
				.InitializeFromSource();
		}

		private void OnDebtsChanged(bool isListReloaded, bool isSelectionChanged)
		{
			if(isListReloaded)
			{
				ytreeviewDebts.YTreeModel.EmitModelChanged();
			}
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
