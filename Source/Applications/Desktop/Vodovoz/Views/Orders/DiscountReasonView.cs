using Gamma.Utilities;
using Gtk;
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
			btnAddNomenclature.Clicked += (sender, args) => ViewModel.AddNomenclatureCommand.Execute();
			btnRemoveNomenclature.Clicked += (sender, args) => ViewModel.RemoveNomenclatureCommand.Execute();
			
			entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryName.Binding.AddBinding(ViewModel, vm => vm.CanChangeDiscountReasonName, w => w.Sensitive).InitializeFromSource();
			checkIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			spinDiscount.Binding.AddBinding(ViewModel.Entity, e => e.Value, w => w.ValueAsDecimal).InitializeFromSource();
			enumDiscountValueType.ItemsEnum = typeof(DiscountUnits);
			enumDiscountValueType.Binding.AddBinding(ViewModel.Entity, e => e.ValueType, w => w.SelectedItem).InitializeFromSource();
			chkBtnPremiumDiscount.Binding.AddBinding(ViewModel.Entity, e => e.IsPremiumDiscount, w => w.Active).InitializeFromSource();
			chkBtnSelectAll.Toggled += (s, e) => ViewModel.UpdateSelectedCategoriesCommand.Execute(chkBtnSelectAll.Active);
			
			ConfigureTreeViews();

			btnRemoveNomenclature.Binding.AddBinding(ViewModel, vm => vm.IsNomenclatureSelected, w => w.Sensitive).InitializeFromSource();
			btnRemoveProductGroup.Binding.AddBinding(ViewModel, vm => vm.IsProductGroupSelected, w => w.Sensitive).InitializeFromSource();
		}

		private void ConfigureTreeViews()
		{
			ConfigureNomenclatureCategoriesTree();
			ConfigureNomenclaturesTree();
			ConfigureProductGroupsTree();
		}

		private void ConfigureNomenclatureCategoriesTree()
		{
			treeViewNomenclatureCategories.CreateFluentColumnsConfig<SelectableNomenclatureCategoryNode>()
				.AddColumn("")
					.AddTextRenderer(x =>
						x.DiscountReasonNomenclatureCategory.NomenclatureCategory.GetEnumTitle())
				.AddColumn("")
					.AddToggleRenderer(x => x.IsSelected)
					.ToggledEvent(OnDiscountNomenclatureCategorySelected)
				.AddColumn("")
				.Finish();

			treeViewNomenclatureCategories.HeadersVisible = false;
			treeViewNomenclatureCategories.ItemsDataSource = ViewModel.SelectableNomenclatureCategoryNodes;
		}

		private void OnDiscountNomenclatureCategorySelected(object o, ToggledArgs args)
		{
			Gtk.Application.Invoke((s, e) =>
			{
				var selectedCategory = treeViewNomenclatureCategories.GetSelectedObject<SelectableNomenclatureCategoryNode>();

				if(selectedCategory == null)
				{
					return;
				}
				
				ViewModel.UpdateNomenclatureCategories(selectedCategory);
			});
		}

		private void ConfigureNomenclaturesTree()
		{
			treeViewNomenclatures.CreateFluentColumnsConfig<Nomenclature>()
				.AddColumn("")
					.AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();
			
			treeViewNomenclatures.HeadersVisible = false;
			treeViewNomenclatures.ItemsDataSource = ViewModel.Entity.ObservableNomenclatures;
			treeViewNomenclatures.Binding.AddBinding(ViewModel, vm => vm.SelectedNomenclature, w => w.SelectedRow).InitializeFromSource();
		}

		private void ConfigureProductGroupsTree()
		{
			treeViewProductGroups.CreateFluentColumnsConfig<ProductGroup>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => ViewModel.Entity.ProductGroups.IndexOf(node) + 1)
				.AddColumn("Группа товаров")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Name)
				.AddColumn("")
				.Finish();

			treeViewProductGroups.ItemsDataSource = ViewModel.Entity.ObservableProductGroups;
			treeViewProductGroups.Binding.AddBinding(ViewModel, vm => vm.SelectedProductGroup, w => w.SelectedRow).InitializeFromSource();
		}
	}
}
