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

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows () > 0;
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
				} else
					logger.Warn ("Не найден столбец с ценой.");
			}
			get { return parentReference; }
		}

		GenericObservableList<IncomingInvoiceItem> items;

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = OrmMain.CreateObjectDialog (ParentReference, treeItemsList.GetSelectedObjects () [0]);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as IncomingInvoiceItem);
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			IOrmDialog dlg = OrmMain.FindMyDialog (this);
			if (dlg != null)
				session = dlg.Session;
			else
				session = OrmMain.OpenSession ();
			ICriteria ItemsCriteria = session.CreateCriteria (typeof(Nomenclature))
				.Add (Restrictions.In ("Category", new[] { NomenclatureCategory.additional, NomenclatureCategory.equipment }));

			OrmReference SelectDialog = new OrmReference (typeof(Nomenclature), session, ItemsCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode &= ~(ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanDelete);
			SelectDialog.ObjectSelected += SelectDialog_ObjectSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void SelectDialog_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			items.Add (new IncomingInvoiceItem { Nomenclature = e.Subject as Nomenclature, Amount = 1, Price = 0, Equipment = null });
		}

		protected void OnButtonCreateClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

