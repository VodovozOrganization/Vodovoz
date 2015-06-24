using System;
using NLog;
using QSOrmProject;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Documents;
using NHibernate;
using NHibernate.Criterion;
using Vodovoz.Domain;
using Gtk;
using System.Collections.Generic;
using QSTDI;
using Gtk.DataBindings;
using QSProjectsLib;
using System.Linq;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MovementDocumentItemsView : Gtk.Bin
	{
		GenericObservableList<MovementDocumentItem> items;

		static Logger logger = LogManager.GetCurrentClassLogger ();

		public MovementDocumentItemsView ()
		{
			this.Build ();
			treeItemsList.Selection.Changed += OnSelectionChanged;
		}

		private IUnitOfWorkGeneric<MovementDocument> documentUoW;

		public IUnitOfWorkGeneric<MovementDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<MovementDocumentItem> ();
				items = DocumentUoW.Root.ObservableItems;
				treeItemsList.ItemsDataSource = items;
				var amountCol = treeItemsList.GetColumnByMappedProp (PropertyUtil.GetName<MovementDocumentItem> (item => item.Amount));
				if (amountCol != null) {
					amountCol.SetCellDataFunc (amountCol.Cells [0], new TreeCellDataFunc (RenderAmountCol));
				}
				treeItemsList.Columns.First (c => c.Title == "CanEditAmount").Visible = false;
			}
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as MovementDocumentItem);
		}

		void RenderAmountCol (TreeViewColumn tree_column, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var col = treeItemsList.Columns.First (c => c.Title == "CanEditAmount");
			(cell as CellRendererText).Editable = (bool)tree_model.GetValue (
				iter, 
				treeItemsList.Columns.ToList<TreeViewColumn> ().FindIndex (m => m == col));
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			//TODO FIXME Adding Item logic here;
		}
	}
}

