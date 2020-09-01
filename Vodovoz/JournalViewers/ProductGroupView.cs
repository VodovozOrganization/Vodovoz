using System;
using System.Linq;
using QS.Deletion;
using QS.Dialog.Gtk;
using Vodovoz.Dialogs.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Filters.GtkViews;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Representations;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProductGroupView : TdiTabBase
	{
		public ProductGroupView()
		{
			this.Build();
			this.TabName = "Группы товаров";
			ConfigureWidget();
		}

		private ProductGroupFilterView productGroupFilterView;
		private ProductGroupVM vm;

		private void ConfigureWidget()
		{
			vm = new ProductGroupVM();
			tableProductGroup.ColumnsConfig = vm.ColumnsConfig;
			vm.Filter = new ProductGroupFilterViewModel();
			CreateProductGroupFilterView(vm.Filter, EventArgs.Empty);
			
			tableProductGroup.RepresentationModel = vm;
			tableProductGroup.EnableSearch = true;
			vm.UpdateNodes();
			tableProductGroup.YTreeModel = vm.TreeModel;
			
			#region SignalsConnect
			
			btnAdd.Clicked += OnButtonAddClicked;
			buttonDelete.Clicked += OnButtonDeleteClicked;
			buttonRefresh.Clicked += (sender, args) => { vm.UpdateNodes(); tableProductGroup.YTreeModel = vm.TreeModel; };
			buttonFilter.Clicked += OnButtonFilterClicked;
			tableProductGroup.Selection.Changed += OnSelectionChanged;
			
			buttonEdit.Clicked += OnButtonEditClicked;
			searchentity.TextChanged += OnSearchEntryTextChanged;
			tableProductGroup.RowActivated += (o, args) => { buttonEdit.Click(); };

			vm.PropertyChanged += (sender, args) => { tableProductGroup.YTreeModel = vm.TreeModel; };

			#endregion
		}

		private void OnButtonAddClicked(object sender, EventArgs e)
		{
			TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<ProductGroup>(0),
				() =>
				{
					var productGroupDlg = new ProductGroupDlg();
					productGroupDlg.EntitySaved += (o, args) => {
						vm.UpdateNodes();
						tableProductGroup.YTreeModel = vm.TreeModel;
					};
					return productGroupDlg;
				}, 
				this
			);
		}

		private void OnButtonDeleteClicked(object sender, EventArgs e)
        {
        	var selected = tableProductGroup.GetSelectedObjects().OfType<ProductGroupVMNode>();
        	foreach(var item in selected)
        		DeleteHelper.DeleteEntity(typeof(ProductGroup), item.Id);

        	vm.UpdateNodes();
            tableProductGroup.YTreeModel = vm.TreeModel;
		}

		private void OnButtonFilterClicked(object sender, EventArgs e)
		{
			productGroupFilterView.Visible = !productGroupFilterView.Visible;
		}

		private void OnButtonEditClicked(object sender, EventArgs e)
		{
			var selectedNode = tableProductGroup.GetSelectedObjects().OfType<ProductGroupVMNode>().FirstOrDefault();
			if(selectedNode != null) {
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<ProductGroup>(selectedNode.Id),
					() => {
						var dlg = new ProductGroupDlg(selectedNode.Id);
						dlg.EntitySaved += (s, ea) => {
							vm.UpdateNodes();
							tableProductGroup.YTreeModel = vm.TreeModel;
						};
						return dlg;
					},
					this
				);
			}
		}

		private void OnSearchEntryTextChanged(object sender, EventArgs e)
		{
			tableProductGroup.SearchHighlightText = searchentity.Text;
			tableProductGroup.RepresentationModel.SearchString = searchentity.Text;
			if (string.IsNullOrWhiteSpace(searchentity.Text))
			{
				vm.UpdateNodes();
				tableProductGroup.YTreeModel = vm.TreeModel;		
			}
		}

		private void CreateProductGroupFilterView(object filter, EventArgs e)
		{
			productGroupFilterView?.Destroy();

			productGroupFilterView = new ProductGroupFilterView(vm.Filter);
			hboxFilter.Add(productGroupFilterView);
			hboxFilter.Show();
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			bool isSensitive = tableProductGroup.Selection.CountSelectedRows() > 0;

			buttonEdit.Sensitive = isSensitive;
			buttonDelete.Sensitive = isSensitive;
		}
	}
}
