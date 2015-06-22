using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using Gtk.DataBindings;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingInvoiceItemsView : Gtk.Bin
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		private IUnitOfWorkGeneric<IncomingInvoice> documentUoW;

		public IUnitOfWorkGeneric<IncomingInvoice> DocumentUoW {
			get {
				return documentUoW;
			}
			set {if (documentUoW == value)
				return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<IncomingInvoiceItem> ();
				items = DocumentUoW.Root.ObservableItems;
				items.ElementChanged += Items_ElementChanged; 
				treeItemsList.ItemsDataSource = items;
				var priceCol = treeItemsList.GetColumnByMappedProp (PropertyUtil.GetName<IncomingInvoiceItem> (item => item.Price));
				if (priceCol != null) {
					CellRendererText cell = new CellRendererText ();
					cell.Text = CurrencyWorks.CurrencyShortName;
					priceCol.PackStart (cell, true);
					//FIXME Обход проблемы с отображением decimal
					priceCol.SetCellDataFunc (priceCol.CellRenderers [0], RenderPriceColumnFunc);
				} else
					logger.Warn ("Не найден столбец с ценой.");
				var amountCol = treeItemsList.GetColumnByMappedProp (PropertyUtil.GetName<IncomingInvoiceItem> (item => item.Amount));
				if (amountCol != null) {
					amountCol.SetCellDataFunc (amountCol.Cells [0], new TreeCellDataFunc (RenderAmountCol));
				}
				var sumCol = treeItemsList.GetColumnByMappedProp (PropertyUtil.GetName<IncomingInvoiceItem> (item => item.Sum));
				if (sumCol != null) {
					sumCol.SetCellDataFunc (sumCol.Cells [0], new TreeCellDataFunc (RenderSumColumnFunc));
				}

				treeItemsList.Columns.First (c => c.Title == "CanEditAmount").Visible = false;
				CalculateTotal ();
			}
		}

		void Items_ElementChanged (object aList, int[] aIdx)
		{
			CalculateTotal ();
		}

		public IncomingInvoiceItemsView ()
		{
			this.Build ();
			treeItemsList.Selection.Changed += OnSelectionChanged;
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

		GenericObservableList<IncomingInvoiceItem> items;

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as IncomingInvoiceItem);
			CalculateTotal ();
		}

		private void RenderPriceColumnFunc (Gtk.TreeViewColumn aColumn, Gtk.CellRenderer aCell, 
		                                    Gtk.TreeModel aModel, Gtk.TreeIter aIter)
		{
			(aCell as CellRendererText).Text = aModel.GetValue (
				aIter,
				treeItemsList.Columns.ToList<TreeViewColumn> ().FindIndex (m => m == aColumn)).ToString ();
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

			ICriteria ItemsCriteria = DocumentUoW.Session.CreateCriteria (typeof(Nomenclature))
				.Add (Restrictions.In ("Category", new[] { NomenclatureCategory.additional, NomenclatureCategory.equipment }));

			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature), DocumentUoW.Session, ItemsCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += NomenclatureSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void NomenclatureSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if ((e.Subject as Nomenclature).Category == NomenclatureCategory.equipment) {
				ITdiTab mytab = TdiHelper.FindMyTab (this);
				if (mytab == null) {
					logger.Warn ("Родительская вкладка не найдена.");
					return;
				}

				var invoices = DocumentUoW.Session.CreateCriteria (typeof(IncomingInvoice)).List<IncomingInvoice> ();
				//TODO FIXME !IMPORTANT! В этот фильтр следует добавлять 
				//все возможные списки с оборудованием, которые будут появляться.
				//Чтобы исключить возможность добавления во входящую накладную
				//уже использующегося и зачисленного на предприятие оборудования.
				List<int> usedItems = new List<int> ();
				foreach (IncomingInvoice i in invoices) {
					foreach (IncomingInvoiceItem item in i.Items) {
						if (item.Equipment != null)
							usedItems.Add (item.Equipment.Id);
					}
				}
				ICriteria ItemsCriteria = DocumentUoW.Session.CreateCriteria (typeof(Equipment))
					.Add (Restrictions.Eq ("Nomenclature", e.Subject))
					.Add (Restrictions.Not (Restrictions.In ("Id", usedItems)));

				OrmReference SelectDialog = new OrmReference (typeof(Equipment), DocumentUoW.Session, ItemsCriteria);
				SelectDialog.Mode = OrmReferenceMode.Select;
				SelectDialog.ButtonMode = ReferenceButtonMode.TreatEditAsOpen | ReferenceButtonMode.CanEdit;

				SelectDialog.ObjectSelected += (s, ev) => DocumentUoW.Root.AddItem (new IncomingInvoiceItem {
					Nomenclature = (ev.Subject as Equipment).Nomenclature,
					Equipment = ev.Subject as Equipment,
					Amount = 1,
					Price = 0
				});

				mytab.TabParent.AddSlaveTab (mytab, SelectDialog);

			} else {
				DocumentUoW.Root.AddItem (new IncomingInvoiceItem { 
					Nomenclature = e.Subject as Nomenclature, 
					Equipment = null,
					Amount = 1, Price = 0 
				});
			}
		}

		protected void OnButtonCreateClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}
			EquipmentGenerator dlg = new EquipmentGenerator ();
			dlg.EquipmentCreated += OnSlaveDlgEquipmentCreated;
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		void OnSlaveDlgEquipmentCreated (object sender, EquipmentCreatedEventArgs e)
		{
			foreach(var equ in e.Equipment)
			{
				DocumentUoW.Root.AddItem (new IncomingInvoiceItem{
					Equipment = equ,
					Nomenclature = equ.Nomenclature,
					Amount = 1
				});
			}
		}

		void CalculateTotal()
		{
			decimal total = 0;
			foreach(var item in documentUoW.Root.Items)
			{
				total += item.Sum;
			}

			labelSum.LabelProp = String.Format ("Итого: {0}", CurrencyWorks.GetShortCurrencyString (total));
		}
	}
}

