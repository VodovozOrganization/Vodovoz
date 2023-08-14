using Gamma.Utilities;
using QS.Views.GtkUI;
using QSProjectsLib;
using System.ComponentModel;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class TransferIncomeView : TabViewBase<TransferIncomeViewModel>
	{
		public TransferIncomeView(TransferIncomeViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			ylabelDate.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Date.ToShortDateString(), w => w.LabelProp)
				.InitializeFromSource();

			ylabelCashier.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Casher.GetPersonNameWithInitials(), w => w.LabelProp)
				.InitializeFromSource();

			ylabelTypeOperation.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.TypeOperation.GetEnumTitle(), w => w.LabelProp)
				.InitializeFromSource();

			ylabelIncomeCategory.Binding
				.AddFuncBinding(ViewModel.Entity, e => ViewModel.FinancialIncomeCategoryNodeInMemoryCacheRepository.GetTitleById(e.ExpenseCategoryId ?? -1), w => w.LabelProp)
				.InitializeFromSource();

			ylabelCashSubdivions.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.RelatedToSubdivision.Name, w => w.LabelProp)
				.InitializeFromSource();

			ylabelTransferDocument.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.CashTransferDocument.Title, w => w.LabelProp)
				.InitializeFromSource();

			ylabelSum.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Money.ToShortCurrencyString(), w => w.LabelProp)
				.InitializeFromSource();

			ylabelDescription.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Description, w => w.LabelProp)
				.InitializeFromSource();

			buttonClose.Clicked += (_, _2) => ViewModel.CloseCommand.Execute();
		}
	}
}
