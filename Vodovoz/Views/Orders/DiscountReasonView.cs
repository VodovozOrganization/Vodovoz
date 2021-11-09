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
			entryName.Binding.AddBinding(ViewModel.Entity, dr => dr.Name, w => w.Text).InitializeFromSource();
			checkIsArchive.Binding.AddBinding(ViewModel.Entity, dr => dr.IsArchive, w => w.Active).InitializeFromSource();
			spinDiscount.Binding.AddBinding(ViewModel.Entity, dr => dr.Value, w => w.ValueAsDecimal).InitializeFromSource();
			enumDiscountValueType.ItemsEnum = typeof(DiscountValueType);
			enumDiscountValueType.Binding.AddBinding(ViewModel.Entity, dr => dr.ValueType, w => w.SelectedItem).InitializeFromSource();

			ybuttonAddGroup.Clicked += YbuttonAddGroupClicked;

			ytreeviewProductGroups.ColumnsConfig = ColumnsConfigFactory.Create<DiscountNomenclatureGroup>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => ViewModel.Entity.ProductGroups.IndexOf(node) + 1)
				.AddColumn("Группа товаров")
					.SetTag(nameof(ProductGroup))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ProductGroup.Name)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			ytreeviewProductGroups.ItemsDataSource = ViewModel.Entity.ObservableDiscountNomenclatureGroups;
			ytreeviewProductGroups.RowActivated += YtreeviewProductGroupsRowActivated;

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
		}

		private void YtreeviewProductGroupsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			RemoveGroup((DiscountNomenclatureGroup)ytreeviewProductGroups.SelectedRow);
		}

		private void YbuttonAddGroupClicked(object sender, System.EventArgs e)
		{
			ViewModel.OpenGroupSelector();
		}

		private void RemoveGroup(DiscountNomenclatureGroup group)
		{
			ViewModel.Entity.RemoveGroup(group);
		}
	}
}
