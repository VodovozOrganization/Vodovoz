using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gtk;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;
using Gtk.DataBindings;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProductSpecificationMaterialsView : Gtk.Bin
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		GenericObservableList<ProductSpecificationMaterial> items;

		private IUnitOfWorkGeneric<ProductSpecification> specificationUoW;

		public IUnitOfWorkGeneric<ProductSpecification> SpecificationUoW {
			get {
				return specificationUoW;
			}
			set {if (specificationUoW == value)
					return;
				specificationUoW = value;
				if (specificationUoW.Root.Materials == null)
					specificationUoW.Root.Materials = new List<ProductSpecificationMaterial> ();
				items = new GenericObservableList<ProductSpecificationMaterial> (specificationUoW.Root.Materials);
				items.ElementChanged += Items_ElementChanged;

				treeMaterialsList.ColumnMappingConfig = MappingConfigure<ProductSpecificationMaterial>.Create()
					.AddColumn ("Наименование").SetDataProperty (p => p.NomenclatureName)
					.AddColumn ("Количество").SetDataProperty(p => p.Amount).Editing ()
					.Finish();
				
				treeMaterialsList.ItemsDataSource = items;
				var amountCol = treeMaterialsList.GetColumnByMappedProp (PropertyUtil.GetName<ProductSpecificationMaterial> (item => item.Amount));
				if (amountCol != null) {
					//amountCol.SetCellDataFunc (amountCol.Cells [0], new TreeCellDataFunc (RenderAmountCol));
				}
					
				CalculateTotal ();
			}
		}

		public ProductSpecificationMaterialsView ()
		{
			this.Build ();
			treeMaterialsList.Selection.Changed += OnSelectionChanged;
		}

		void Items_ElementChanged (object aList, int[] aIdx)
		{
			CalculateTotal ();
		}

	/*	void RenderAmountCol (TreeViewColumn tree_column, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var col = treeItemsList.Columns.First (c => c.Title == "CanEditAmount");
			(cell as CellRendererText).Editable = (bool)tree_model.GetValue (
				iter, 
				treeItemsList.Columns.ToList<TreeViewColumn> ().FindIndex (m => m == col));
		} */

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeMaterialsList.Selection.CountSelectedRows () > 0;
			buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature), SpecificationUoW.Session, Repository.NomenclatureRepository.NomenclatureForProductMaterialsQuery ().GetExecutableQueryOver (SpecificationUoW.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			//SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			items.Add (new ProductSpecificationMaterial { 
				Material = e.Subject as Nomenclature, 
				Amount = 1
			});
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeMaterialsList.GetSelectedObjects () [0] as ProductSpecificationMaterial);
			CalculateTotal ();
		}

		void CalculateTotal()
		{
			decimal totalAmount = 0;
			foreach(var item in SpecificationUoW.Root.Materials)
			{
				totalAmount += item.Amount;
			}

			labelSum.LabelProp = String.Format ("Всего: {0}", (totalAmount));
		}
	}
}

