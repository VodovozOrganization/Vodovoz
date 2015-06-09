using System;
using Gtk;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyContractsView : Gtk.Bin
	{

		/*
		public IContractOwner ContractOwner {
			get { return contractOwner; }
			set {
				contractOwner = value;
				if (ContractOwner.CounterpartyContracts == null)
					ContractOwner.CounterpartyContracts = new List<CounterpartyContract> ();
				CounterpartyContracts = new GenericObservableList<CounterpartyContract> (contractOwner.CounterpartyContracts);
				//FIXME treeCounterpartyContracts.ItemsDataSource = CounterpartyContracts;
				if (typeof(ISpecialRowsRender).IsAssignableFrom (typeof(CounterpartyContract))) {
					foreach (TreeViewColumn col in treeCounterpartyContracts.Columns)
						col.SetCellDataFunc (col.Cells [0], new TreeCellDataFunc (RenderCell));
				}
			}
		} */

		private IUnitOfWorkGeneric<Counterparty> counterpartyUoW;

		public IUnitOfWorkGeneric<Counterparty> CounterpartyUoW {
			get {
				return counterpartyUoW;
			}
			set {if (counterpartyUoW == value)
					return;
				counterpartyUoW = value;
				treeCounterpartyContracts.RepresentationModel = new ViewModel.ContractsVM(value);
				treeCounterpartyContracts.RepresentationModel.UpdateNodes ();
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

			ITdiDialog dlg = new CounterpartyContractDlg (CounterpartyUoW.Root);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = new CounterpartyContractDlg (treeCounterpartyContracts.GetSelectedId ());
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnTreeCounterpartyContractsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (OrmMain.DeleteObject (typeof(CounterpartyContract),
				treeCounterpartyContracts.GetSelectedId())) 
			{
				treeCounterpartyContracts.RepresentationModel.UpdateNodes ();
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

