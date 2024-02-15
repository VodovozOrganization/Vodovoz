﻿using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Tdi;

namespace Vodovoz
{

	public partial class ReadyForReceptionView : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork uow;

		ViewModel.ReadyForReceptionVM viewModel;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
				viewModel = new ViewModel.ReadyForReceptionVM (value);
				readyforreceptionfilter1.UoW = value;
				viewModel.Filter = readyforreceptionfilter1;
				tableReadyForReception.RepresentationModel = viewModel;
				tableReadyForReception.RepresentationModel.UpdateNodes ();
			}
		}


		public ReadyForReceptionView ()
		{
			this.Build ();
			this.TabName = "Готовые к разгрузке";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			tableReadyForReception.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonOpen.Sensitive = tableReadyForReception.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			var node = tableReadyForReception.GetSelectedNode () as ViewModel.ReadyForReceptionVMNode;
			var dlg = new CarUnloadDocumentDlg (node.Id, viewModel.Filter.RestrictWarehouse?.Id);
			TabParent.AddTab (dlg, this);
		}

		protected void OnTableReadyForReceptionRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonOpen.Click ();
		}

		protected void OnSearchentity1TextChanged(object sender, EventArgs e)
		{
			tableReadyForReception.SearchHighlightText = searchentity1.Text;
			tableReadyForReception.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			tableReadyForReception.RepresentationModel.UpdateNodes();
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

