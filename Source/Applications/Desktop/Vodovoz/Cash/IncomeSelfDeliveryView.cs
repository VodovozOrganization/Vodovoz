using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Cash;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class IncomeSelfDeliveryView : TabViewBase<IncomeSelfDeliveryViewModel>
	{
		public IncomeSelfDeliveryView(IncomeSelfDeliveryViewModel viewModel) : base(viewModel)
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

			permissioncommentview.UoW = ViewModel.UoW;
			permissioncommentview.Title = "Комментарий по проверке закрытия МЛ: ";
			permissioncommentview.Comment = ViewModel.Entity.CashierReviewComment;
			permissioncommentview.PermissionName = "can_edit_cashier_review_comment";
			permissioncommentview.Comment = ViewModel.Entity.CashierReviewComment;
			permissioncommentview.CommentChanged += (comment) => ViewModel.Entity.CashierReviewComment = comment;

			enumcomboOperation.ItemsEnum = typeof(IncomeType);
			enumcomboOperation.Binding
				.AddBinding(ViewModel.Entity, e => e.TypeOperation, w => w.SelectedItem)
				.InitializeFromSource();

			enumcomboOperation.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanEditTypeOperation, w => w.Sensitive)
				.InitializeFromSource();

			entryCashier.ViewModel = ViewModel.CashierViewModel;

			var clientEntryViewModelBuilder = new LegacyEEVMBuilderFactory<Income>(
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

			yspinMoney.CurrencyFormat = true;

			yspinMoney.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Money, w => w.ValueAsDecimal)
				.InitializeFromSource();

			entryIncomeFinancialCategory.ViewModel = ViewModel.FinancialIncomeCategoryViewModel;

			ytextviewDescription.Binding
				.AddBinding(ViewModel.Entity, s => s.Description, w => w.Buffer.Text)
				.InitializeFromSource();

			currencylabel1.Binding
				.AddBinding(ViewModel, vm => vm.CurrencySymbol, w => w.Text)
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
