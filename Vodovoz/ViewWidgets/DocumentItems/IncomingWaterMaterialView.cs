using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using Gtk.DataBindings;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingWaterMaterialView : Gtk.Bin
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		private IUnitOfWorkGeneric<IncomingWater> documentUoW;

		public IUnitOfWorkGeneric<IncomingWater> DocumentUoW {
			get {
				return documentUoW;
			}
			set {if (documentUoW == value)
				return;
				documentUoW = value;
				if (DocumentUoW.Root.Materials == null)
					DocumentUoW.Root.Materials = new List<IncomingWaterMaterial> ();
				items = DocumentUoW.Root.ObservableMaterials;
				items.ElementChanged += Items_ElementChanged; 
				treeMaterialsList.ItemsDataSource = items;
				var OneProductCol = treeMaterialsList.GetColumnByMappedProp (PropertyUtil.GetName<IncomingWaterMaterial> (item => item.OneProductAmount));
				if (OneProductCol != null) {
					//OneProductCol.SetCellDataFunc (OneProductCol.Cells [0], new TreeCellDataFunc (RenderSumColumnFunc));
				}
				var amountCol = treeMaterialsList.GetColumnByMappedProp (PropertyUtil.GetName<IncomingWaterMaterial> (item => item.Amount));
				if (amountCol != null) {
					//amountCol.SetCellDataFunc (amountCol.Cells [0], new TreeCellDataFunc (RenderAmountCol));
				}

				CalculateTotal ();
			}
		}

		void Items_ElementChanged (object aList, int[] aIdx)
		{
			CalculateTotal ();
		}

		public IncomingWaterMaterialView ()
		{
			this.Build ();
			treeMaterialsList.Selection.Changed += OnSelectionChanged;
		}

		void RenderAmountCol (TreeViewColumn tree_column, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var col = treeMaterialsList.Columns.First (c => c.Title == "CanEditAmount");
			(cell as CellRendererText).Editable = (bool)tree_model.GetValue (
				iter, 
				treeMaterialsList.Columns.ToList<TreeViewColumn> ().FindIndex (m => m == col));
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonDelete.Sensitive = treeMaterialsList.Selection.CountSelectedRows () > 0;
		}

		GenericObservableList<IncomingWaterMaterial> items;

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeMaterialsList.GetSelectedObjects () [0] as IncomingWaterMaterial);
			CalculateTotal ();
		}

		private void RenderPriceColumnFunc (Gtk.TreeViewColumn aColumn, Gtk.CellRenderer aCell, 
		                                    Gtk.TreeModel aModel, Gtk.TreeIter aIter)
		{
			(aCell as CellRendererText).Text = aModel.GetValue (
				aIter,
				treeMaterialsList.Columns.ToList<TreeViewColumn> ().FindIndex (m => m == aColumn)).ToString ();
		}

		private void RenderSumColumnFunc (Gtk.TreeViewColumn aColumn, Gtk.CellRenderer aCell, 
			Gtk.TreeModel aModel, Gtk.TreeIter aIter)
		{
			decimal sum = (((aModel as TreeModelAdapter).Implementor as MappingsImplementor).NodeFromIter (aIter) as IncomingInvoiceItem).Sum;

			(aCell as CellRendererText).Text = CurrencyWorks.GetShortCurrencyString (sum);
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			//ICriteria ItemsCriteria = DocumentUoW.Session.CreateCriteria (typeof(Nomenclature))
			//	.Add (Restrictions.In ("Category", new[] { NomenclatureCategory.additional, NomenclatureCategory.equipment }));

			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.StockBalanceVM());
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.None;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var nomenctature = DocumentUoW.GetById<Nomenclature> (e.ObjectId);
			DocumentUoW.Root.AddMaterial (new IncomingWaterMaterial { 
				Nomenclature = nomenctature,
				Amount = 1
				});
		}

		void CalculateTotal()
		{
			int total = 0;
			foreach(var item in documentUoW.Root.Materials)
			{
				total += item.Amount;
			}

			labelSum.LabelProp = String.Format ("Всего: {0}", total);
		}
	}
}

