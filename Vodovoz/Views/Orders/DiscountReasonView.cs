using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Views.GtkUI;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Representations;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class DiscountReasonView : TabViewBase<DiscountReasonViewModel>
	{
		public DiscountReasonView(DiscountReasonViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryName.Binding.AddBinding(ViewModel.Entity, dr => dr.Name, w => w.Text).InitializeFromSource();
			checkIsArchive.Binding.AddBinding(ViewModel.Entity, dr => dr.IsArchive, w => w.Active).InitializeFromSource();
			spinDiscount.Binding.AddBinding(ViewModel.Entity, dr => dr.Value, w => w.ValueAsDecimal).InitializeFromSource();
			enumDiscountValueType.ItemsEnum = typeof(DiscountValueType);
			enumDiscountValueType.Binding.AddBinding(ViewModel.Entity, dr => dr.ValueType, w => w.SelectedItem).InitializeFromSource();

			ybuttonAddGroup.Clicked += YbuttonAddGroup_Clicked;

			ytreeview1.ColumnsConfig = ColumnsConfigFactory.Create<DiscountNomenclatureGroup>()
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
			ytreeview1.ItemsDataSource = ViewModel.Entity.ObservableDiscountNomenclatureGroups;
			ytreeview1.RowActivated += Ytreeview1_RowActivated;

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
		}

		private void Ytreeview1_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			RemoveGroup((DiscountNomenclatureGroup)ytreeview1.SelectedRow);
		}

		private void YbuttonAddGroup_Clicked(object sender, System.EventArgs e)
		{
			ViewModel.OpenGroupSelector();
		}

		private void RemoveGroup(DiscountNomenclatureGroup group)
		{
			ViewModel.Entity.RemoveGroup(group);
		}
	}
}
