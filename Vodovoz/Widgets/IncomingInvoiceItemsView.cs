using System;
using NHibernate;
using QSOrmProject;
using System.Data.Bindings.Collections.Generic;
using QSTDI;
using NLog;
using NHibernate.Criterion;
using System.Linq;
using Gtk;
using QSProjectsLib;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingInvoiceItemsView : Gtk.Bin
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

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

		ISession session;

		public ISession Session {
			get { return session; }
			set { session = value; }
		}

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null)
					Session = parentReference.Session;
				if (!(ParentReference.ParentObject is IncomingInvoice))
					throw new ArgumentException (String.Format ("Родительский объект в parentReference должен являться классом {0}", typeof(IncomingInvoice)));
				items = new GenericObservableList<IncomingInvoiceItem> ((ParentReference.ParentObject as IncomingInvoice).Items);
				treeItemsList.ItemsDataSource = items;
				var priceCol = treeItemsList.Columns.First (c => c.Title == "Цена");
				if (priceCol != null) {
					CellRendererText cell = new CellRendererText ();
					cell.Text = CurrencyWorks.CurrencyShortName;
					priceCol.PackStart (cell, true);
					//FIXME Обход проблемы с отображением decimal
					priceCol.SetCellDataFunc (priceCol.CellRenderers [0], RenderPriceColumnFunc);
				} else
					logger.Warn ("Не найден столбец с ценой.");
				var amountCol = treeItemsList.Columns.First (c => c.Title == "Количество");
				if (amountCol != null) {
					amountCol.SetCellDataFunc (amountCol.Cells [0], new TreeCellDataFunc (RenderAmountCol));
				}
				treeItemsList.Columns.First (c => c.Title == "CanEditAmount").Visible = false;
			}
			get { return parentReference; }
		}

		GenericObservableList<IncomingInvoiceItem> items;

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as IncomingInvoiceItem);
		}

		private void RenderPriceColumnFunc (Gtk.TreeViewColumn aColumn, Gtk.CellRenderer aCell, 
		                                    Gtk.TreeModel aModel, Gtk.TreeIter aIter)
		{
			(aCell as CellRendererText).Text = aModel.GetValue (
				aIter,
				treeItemsList.Columns.ToList<TreeViewColumn> ().FindIndex (m => m == aColumn)).ToString ();
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			ICriteria ItemsCriteria = session.CreateCriteria (typeof(Nomenclature))
				.Add (Restrictions.In ("Category", new[] { NomenclatureCategory.additional, NomenclatureCategory.equipment }));

			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature), session, ItemsCriteria);
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

				var invoices = session.CreateCriteria (typeof(IncomingInvoice)).List<IncomingInvoice> ();
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
				ICriteria ItemsCriteria = session.CreateCriteria (typeof(Equipment))
					.Add (Restrictions.Eq ("Nomenclature", e.Subject))
					.Add (Restrictions.Not (Restrictions.In ("Id", usedItems)));

				OrmReference SelectDialog = new OrmReference (typeof(Equipment), session, ItemsCriteria);
				SelectDialog.Mode = OrmReferenceMode.Select;
				SelectDialog.ButtonMode = ReferenceButtonMode.TreatEditAsOpen | ReferenceButtonMode.CanEdit;

				SelectDialog.ObjectSelected += (s, ev) => {
					items.Add (new IncomingInvoiceItem { 
						Nomenclature = (ev.Subject as Equipment).Nomenclature, 
						Equipment = ev.Subject as Equipment, 
						Amount = 1, Price = 0
					});
				};

				mytab.TabParent.AddSlaveTab (mytab, SelectDialog);

			} else
				items.Add (new IncomingInvoiceItem { 
					Nomenclature = e.Subject as Nomenclature, 
					Equipment = null,
					Amount = 1, Price = 0 
				});

		}

		protected void OnButtonCreateClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}
			EquipmentGenerator dlg = new EquipmentGenerator ();
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}
	}
}

