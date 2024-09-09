using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gtk;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using Vodovoz.Dialogs.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Filters.GtkViews;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Representations.ProductGroups;
using Vodovoz.ViewModels.Dialogs.Goods;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProductGroupView : TdiTabBase
	{
		private Button _btnGroupEditing;
		private object[] _selectedItems;

		public ProductGroupView()
		{
			Build();
			TabName = "Группы товаров";
			ConfigureWidget();
		}

		private ProductGroupFilterView _productGroupFilterView;
		private ProductGroupVM _vm;

		private void ConfigureWidget()
		{
			_vm = new ProductGroupVM {
				Filter = new ProductGroupFilterViewModel
				{
					HidenByDefault = false,
					HideArchive = true
				}
			};

			_productGroupFilterView = new ProductGroupFilterView(_vm.Filter);
			hboxFilter.Add(_productGroupFilterView);
			hboxFilter.Show();
			
			tableProductGroup.RepresentationModel = _vm;
			tableProductGroup.EnableSearch = true;
			_vm.UpdateNodes();
			tableProductGroup.ColumnsConfig = _vm.ColumnsConfig;
			tableProductGroup.YTreeModel = _vm.YTreeModel;
			tableProductGroup.Selection.Mode = SelectionMode.Multiple;
			buttonDelete.Visible = false;
			CreateBtnGroupEditing();
			
			#region SignalsConnect
			
			btnAdd.Clicked += OnButtonAddClicked;
			buttonRefresh.Clicked += OnRefreshClicked;
			buttonFilter.Clicked += OnButtonFilterClicked;
			tableProductGroup.Selection.Changed += OnSelectionChanged;
			buttonEdit.Clicked += OnButtonEditClicked;
			searchentity.TextChanged += OnSearchEntryTextChanged;
			tableProductGroup.RowActivated += OnProductGroupRowActivated;
			_vm.PropertyChanged += OnViewModelPropertyChanged;

			#endregion
			OnSelectionChanged(null, EventArgs.Empty);
			buttonFilter.Click();
		}

		private void CreateBtnGroupEditing()
		{
			_btnGroupEditing = new Button("Перенос в другую группу");
			_btnGroupEditing.Clicked += OnBtnGroupEditingClicked;
			hbox1.Add(_btnGroupEditing);
			var box = (Box.BoxChild)hbox1[_btnGroupEditing];
			box.Position = 3;
			box.Expand = false;
			_btnGroupEditing.Show();
		}

		private void OnBtnGroupEditingClicked(object sender, EventArgs e)
		{
			var firstSelectedItem = _selectedItems.FirstOrDefault();

			if(firstSelectedItem == null)
			{
				return;
			}

			if(firstSelectedItem.GetType() != typeof(NomenclatureGroupNode))
			{
				var viewModel = new ProductGroupVM(_vm.UoW, new ProductGroupFilterViewModel
				{
					HideArchive = true,
					HidenByDefault = false
				});
				var selectDialog = new PermissionControlledRepresentationJournal(viewModel, Buttons.None);
				selectDialog.Name = "Выбор группы товаров, куда будут переноситься позиции";
				selectDialog.Mode = JournalSelectMode.Single;
				selectDialog.ShowFilter = true;
				selectDialog.ObjectSelected += OnSelectDialogObjectSelected;
				TabParent.AddSlaveTab(this, selectDialog);
			}
		}

		private void OnSelectDialogObjectSelected(object sender, JournalObjectSelectedEventArgs e)
		{
			var selectedProductGroup = e.Selected.FirstOrDefault();

			if(selectedProductGroup == null)
			{
				return;
			}
			if(!(selectedProductGroup is ProductGroupVMNode productGroupNode))
			{
				MessageDialogHelper.RunWarningDialog("Выбрана не товарная группа!");
				return;
			}

			var productGroup = _vm.UoW.GetById<ProductGroup>(productGroupNode.Id);
			var firstSelectedItem = _selectedItems.FirstOrDefault();
			
			if(firstSelectedItem is ProductGroupVMNode)
			{
				var productGroups = _vm.UoW.Session.QueryOver<ProductGroup>()
					.WhereRestrictionOn(pg => pg.Id)
					.IsInG(_selectedItems.Select(x => x.GetId()))
					.List();

				foreach(var item in productGroups)
				{
					item.Parent = productGroup;
					
					if(ProductGroup.CheckCircle(item, productGroup))
					{
						MessageDialogHelper.RunWarningDialog("Обнаружена циклическая ссылка. Операция не возможна");
						return;
					}
					
					_vm.UoW.Save(item);
				}
				_vm.UoW.Commit();
			}
			else if(firstSelectedItem is NomenclatureNode)
			{
				var nomenclatures = _vm.UoW.Session.QueryOver<Nomenclature>()
					.WhereRestrictionOn(n => n.Id)
					.IsInG(_selectedItems.Select(x => x.GetId()))
					.List();

				foreach(var item in nomenclatures)
				{
					item.ProductGroup = productGroup;
					_vm.UoW.Save(item);
				}
				_vm.UoW.Commit();
			}
			
			UpdateData();
		}

		private void UpdateData()
		{
			var selected = _selectedItems;
			_vm.UpdateNodes();
			tableProductGroup.YTreeModel = _vm.YTreeModel;

			if(!(_vm.ItemsList is IList<ProductGroupVMNode> productGroups))
			{
				return;
			}

			foreach(var item in selected)
			{
				if(item is ProductGroupVMNode productGroupNode)
				{
					var node = productGroups.SingleOrDefault(x => x.Id == productGroupNode.Id);
					ExpandAndSelectTreeNode(node);
				}
				else if(item is NomenclatureNode nomenclatureNode)
				{
					var node = productGroups.SelectMany(x => x.ChildNomenclatures)
						.SingleOrDefault(x => x.Id == nomenclatureNode.Id);
					
					ExpandAndSelectTreeNode(node);
				}
			}
		}
		
		private void OnRefreshClicked(object sender, EventArgs e)
		{
			_vm.UpdateNodes();
			tableProductGroup.YTreeModel = _vm.YTreeModel;
		}

		private void OnProductGroupRowActivated(object o, RowActivatedArgs args)
		{
			OnButtonEditClicked(o, args);
		}
		
		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if(args.PropertyName == nameof(_vm.YTreeModel))
			{
				tableProductGroup.YTreeModel = _vm.YTreeModel;
			}
		}

		private void ExpandAndSelectTreeNode(object node)
		{
			if(node == null)
			{
				return;
			}

			var path = tableProductGroup.YTreeModel.PathFromNode(node);
			tableProductGroup.ExpandToPath(path);
			tableProductGroup.Selection.SelectPath(path);
		}

		private void OnButtonAddClicked(object sender, EventArgs e)
		{
			TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<ProductGroup>(0),
				() =>
				{
					var productGroupDlg = new ProductGroupDlg();
					productGroupDlg.EntitySaved += (o, args) => UpdateData();
					return productGroupDlg;
				}, 
				this
			);
		}

		private void OnButtonFilterClicked(object sender, EventArgs e)
		{
			_productGroupFilterView.Visible = !_productGroupFilterView.Visible;
		}

		private void OnButtonEditClicked(object sender, EventArgs e)
		{
			var selectedNode = _selectedItems.FirstOrDefault();

			switch(selectedNode)
			{
				case ProductGroupVMNode productGroupNode:
					var pageProductGroup = Startup.MainWin.NavigationManager.OpenTdiTabOnTdi<ProductGroupDlg, int>(
						this, productGroupNode.Id, OpenPageOptions.AsSlave);
					
					if(pageProductGroup.TdiTab is ProductGroupDlg dlg)
					{
						dlg.EntitySaved += (o, args) => UpdateData();
					}
					break;
				case NomenclatureNode nomenclatureNode:
					var pageNomenclature = Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclatureViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(nomenclatureNode.Id), OpenPageOptions.AsSlave);
					pageNomenclature.ViewModel.EntitySaved += (o, args) => UpdateData();
					break;
				default:
					return;
			}
		}

		private void OnSearchEntryTextChanged(object sender, EventArgs e)
		{
			tableProductGroup.SearchHighlightText = searchentity.Text;
			tableProductGroup.RepresentationModel.SearchString = searchentity.Text;
			if (string.IsNullOrWhiteSpace(searchentity.Text))
			{
				_vm.UpdateNodes();
				tableProductGroup.YTreeModel = _vm.YTreeModel;		
			}
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			_selectedItems = tableProductGroup.GetSelectedObjects();
			var isSelected = _selectedItems.Length > 0;
			buttonEdit.Sensitive = isSelected;
			buttonDelete.Sensitive = isSelected;
			_btnGroupEditing.Sensitive = isSelected && SelectedItemsHasSameType();
		}

		private bool SelectedItemsHasSameType()
		{
			var firstItemType = _selectedItems.First().GetType();

			if(firstItemType == typeof(ProductGroupVMNode))
			{
				return _selectedItems.All(x => x.GetType() == typeof(ProductGroupVMNode));
			}
			return firstItemType == typeof(NomenclatureNode) && _selectedItems.All(x => x.GetType() == typeof(NomenclatureNode));
		}

		public override void Destroy()
		{
			_productGroupFilterView?.Destroy();
			
			if(_vm != null)
			{
				_vm.PropertyChanged -= OnViewModelPropertyChanged;
			}

			base.Destroy();
		}
	}
}
