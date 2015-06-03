using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gtk;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyContractsView : Gtk.Bin
	{
		private IContractOwner contractOwner;
		private GenericObservableList<CounterpartyContract> CounterpartyContracts;

		public IContractOwner ContractOwner {
			get { return contractOwner; }
			set {
				contractOwner = value;
				if (ContractOwner.CounterpartyContracts == null)
					ContractOwner.CounterpartyContracts = new List<CounterpartyContract> ();
				CounterpartyContracts = new GenericObservableList<CounterpartyContract> (contractOwner.CounterpartyContracts);
				treeCounterpartyContracts.ItemsDataSource = CounterpartyContracts;
				if (typeof(ISpecialRowsRender).IsAssignableFrom (typeof(CounterpartyContract))) {
					foreach (TreeViewColumn col in treeCounterpartyContracts.Columns)
						col.SetCellDataFunc (col.Cells [0], new TreeCellDataFunc (RenderCell));
				}
			}
		}

		public CounterpartyContractsView ()
		{
			this.Build ();
			treeCounterpartyContracts.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeCounterpartyContracts.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			var parentDlg = OrmMain.FindMyDialog (this);
			if (parentDlg == null)
				return;

			if(parentDlg.UoW.IsNew)
			{
				if (CommonDialogs.SaveBeforeCreateSlaveEntity (parentDlg.Subject.GetType (), typeof(CounterpartyContract))) {
					parentDlg.UoW.Save ();
				} else
					return;
			}

			ITdiDialog dlg = new CounterpartyContractDlg (contractOwner as Counterparty);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = new CounterpartyContractDlg (treeCounterpartyContracts.GetSelectedObjects () [0] as CounterpartyContract);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnTreeCounterpartyContractsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var contract = treeCounterpartyContracts.GetSelectedObjects () [0] as CounterpartyContract;

			if (OrmMain.DeleteObject (contract)) {
				CounterpartyContracts.Remove (contract);
			}
		}

		private void RenderCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			object o = ((treeCounterpartyContracts.Model as TreeModelAdapter)
				.Implementor as Gtk.DataBindings.MappingsImplementor).NodeFromIter (iter);
			(cell as CellRendererText).Foreground = (o as ISpecialRowsRender).TextColor;
		}
	}
}

