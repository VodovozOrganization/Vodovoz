using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class AdvanceReportView : TabViewBase<AdvanceReportViewModel>
	{
		public AdvanceReportView(AdvanceReportViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{

			if(!accessfilteredsubdivisionselectorwidget.Configure(
				ViewModel.UoW,
				false,
				typeof(AdvanceReport)))
			{
				ViewModel.InitializationFailed(
					"Ошибка",
					accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				return;
			}

			accessfilteredsubdivisionselectorwidget.OnSelected += (_, _2) => UpdateSubdivision();

			entryEmployee.ViewModel = ViewModel.EmployeeViewModel;

			entryEmployee.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsNew, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			entryCashier.ViewModel = ViewModel.CashierViewModel;

			ydateDocument.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenceCategoryViewModel;
			entryExpenseFinancialCategory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsNew, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			yspinMoney.Binding
				.AddBinding(ViewModel, vm => vm.Money, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			specialListCmbOrganisation.ShowSpecialStateNot = true;
			specialListCmbOrganisation.ItemsList = ViewModel.CachedOrganizaions;
			specialListCmbOrganisation.Binding
				.AddBinding(ViewModel.Entity, e => e.Organisation, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			ytextviewDescription.Binding
				.AddBinding(ViewModel.Entity, s => s.Description, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Editable)
				.InitializeFromSource();

			ytextviewRouteList.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.HasRouteList, w => w.Visible)
				.AddBinding(vm => vm.RouteListTitle, w => w.Buffer.Text)
				.InitializeFromSource();

			ylabelRouteList.Binding
				.AddBinding(ViewModel, vm => vm.HasRouteList, w => w.Visible)
				.InitializeFromSource();

			ConfigureTreeViewDebts();

			if(ViewModel.Entity.RelatedToSubdivision != null)
			{
				accessfilteredsubdivisionselectorwidget
					.SelectIfPossible(ViewModel.Entity.RelatedToSubdivision);
			}

			UpdateSubdivision();

			ylabelCurrency.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CurrencySymbol, w => w.Text)
				.AddBinding(vm => vm.ClosingSumNotEqualsMoney, w => w.Visible)
				.InitializeFromSource();

			ylabelDebtTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Visible)
				.InitializeFromSource();

			labelCurrentDebt.Binding
				.AddBinding(ViewModel, vm => vm.DebtString, w => w.Text)
				.InitializeFromSource();

			ylabelLastGivenAdvances.Binding
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Visible)
				.InitializeFromSource();

			ylabelCreating.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsNew, w => w.Visible)
				.InitializeFromSource();

			ylabelCreating.UseMarkup = true;

			ylabelChangeSum.Binding.AddBinding(ViewModel, vm => vm.ChangeSumWarning ? $"<span foreground=\"{GdkColors.Red.ToHtmlColor()}\">{vm.ChangeSumMessage}</span>" : vm.ChangeSumMessage, w => w.LabelProp)
				.InitializeFromSource();

			ylabelChangeSum.UseMarkup = true;

			ylabelChangeType.Binding.AddBinding(ViewModel, vm => vm.ChangeTypeMessage, w => w.LabelProp)
				.InitializeFromSource();

			ylabelChangeType.UseMarkup = true;

			ylabelClosingSum.Binding.AddBinding(ViewModel, vm => vm.ClosingSumString, w => w.LabelProp)
				.InitializeFromSource();

			table1.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
				
			accessfilteredsubdivisionselectorwidget.Sensitive = ViewModel.CanEdit;

			hboxDebt.Binding
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Visible)
				.InitializeFromSource();

			GtkScrolledWindow1.Visible = ViewModel.IsNew;

			buttonSave.Clicked += (_, _2) => ViewModel.SaveCommand.Execute();
			buttonCancel.Clicked += (_, _2) => ViewModel.CloseCommand.Execute();
		}

		private void ConfigureTreeViewDebts()
		{
			ytreeviewDebts.ColumnsConfig = ColumnsConfigFactory.Create<SelectableNode<Expense>>()
				.AddColumn("Закрыть").AddToggleRenderer(a => a.Selected).Editing()
				.AddColumn("Дата").AddTextRenderer(a => a.Value.Date.ToString())
				.AddColumn("Получено").AddTextRenderer(a => a.Value.Money.ToString("C"))
				.AddColumn("Непогашено").AddTextRenderer(a => $"{a.Value.UnclosedMoney:C}")
				.AddColumn("Статья").AddTextRenderer(a => a.Value.ExpenseCategoryId != null
					? ViewModel.GetCachedExpenseCategoryTitle(a.Value.ExpenseCategoryId.Value)
					: "")
				.AddColumn("Основание").AddTextRenderer(a => a.Value.Description)
				.RowCells().AddSetter<CellRenderer>(
					(cell, node) =>
					{
						cell.Sensitive =
							node.Value.RouteListClosing == ViewModel.Entity.RouteList
							|| !ViewModel.SelectableAdvances.Any(s => s.Selected);
					})
				.Finish();

			ytreeviewDebts.Binding
				.AddBinding(ViewModel, vm => vm.SelectableAdvances, w => w.ItemsDataSource)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ViewModel.OnDebtsChanged += (_) => ytreeviewDebts.YTreeModel.EmitModelChanged();

			ViewModel.PropertyChanged += OnViewModelPropertyCHanged;
		}

		private void OnViewModelPropertyCHanged(object sender, PropertyChangedEventArgs eventArgs)
		{
			if(eventArgs.PropertyName == nameof(ViewModel.CreatingMessageState)
				|| eventArgs.PropertyName == nameof(ViewModel.CreatingMessage))
			{
				var message = ViewModel.CreatingMessage;

				string formattedMessage;

				switch(ViewModel.CreatingMessageState)
				{
					case CreatingMessageState.ClosingSumZero:
						formattedMessage = $"<span foreground=\"{GdkColors.CadetBlue.ToHtmlColor()}\"{message}</span>";
						break;
					case CreatingMessageState.BalanceZero:
						formattedMessage = $"<span foreground=\"{GdkColors.Green.ToHtmlColor()}\"{message}</span>";
						break;
					case CreatingMessageState.BalanceLessThanZero:
						formattedMessage = $"<span foreground=\"{GdkColors.Blue.ToHtmlColor()}\"{message}</span>";
						break;
					case CreatingMessageState.BalanceGreaterThanZero:
						formattedMessage = $"<span foreground=\"{GdkColors.Blue.ToHtmlColor()}\"{message}</span>";
						break;
					default:
						formattedMessage = $"<span foreground=\"{GdkColors.PrimaryText.ToHtmlColor()}\"{message}</span>";
						break;
				}

				ylabelCreating.LabelProp = formattedMessage;
			}
		}

		public void UpdateSubdivision()
		{
			if(accessfilteredsubdivisionselectorwidget.SelectedSubdivision != null
				&& accessfilteredsubdivisionselectorwidget.NeedChooseSubdivision)
			{
				ViewModel.Entity.RelatedToSubdivision =
					accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
			}
		}
	}
}
