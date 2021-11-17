using Gamma.GtkWidgets;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class DiscountReasonView : TabViewBase<DiscountReasonViewModel>
	{
		public DiscountReasonView(DiscountReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			btnAddProductGroup.Clicked += (sender, args) => ViewModel.AddProductGroupCommand.Execute();
			btnRemoveProductGroup.Clicked += (sender, args) => ViewModel.RemoveProductGroupCommand.Execute();
			
			entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryName.Binding.AddBinding(ViewModel, vm => vm.CanChangeDiscountReasonName, w => w.Sensitive).InitializeFromSource();
			checkIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			spinDiscount.Binding.AddBinding(ViewModel.Entity, e => e.Value, w => w.ValueAsDecimal).InitializeFromSource();
			enumDiscountValueType.ItemsEnum = typeof(DiscountUnits);
			enumDiscountValueType.Binding.AddBinding(ViewModel.Entity, e => e.ValueType, w => w.SelectedItem).InitializeFromSource();
			chkBtnPremiumDiscount.Binding.AddBinding(ViewModel.Entity, e => e.IsPremiumDiscount, w => w.Active).InitializeFromSource();
			
			ConfigureTreeView();

			btnRemoveProductGroup.Binding.AddBinding(ViewModel, vm => vm.IsProductGroupSelected, w => w.Sensitive).InitializeFromSource();
		}

		private void ConfigureTreeView()
		{
			treeViewProductGroups.ColumnsConfig = ColumnsConfigFactory.Create<ProductGroup>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => ViewModel.Entity.ProductGroups.IndexOf(node) + 1)
				.AddColumn("Группа товаров")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Name)
				.RowCells()
				.XAlign(0.5f)
				.Finish();

			treeViewProductGroups.ItemsDataSource = ViewModel.Entity.ObservableProductGroups;
			treeViewProductGroups.Binding.AddBinding(ViewModel, vm => vm.SelectedProductGroup, w => w.SelectedRow).InitializeFromSource();
		}
	}
}
