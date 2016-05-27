using System;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	public partial class ReadyForShipmentView : TdiTabBase
	{
		private IUnitOfWork uow;

		ViewModel.ReadyForShipmentVM viewModel;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
				viewModel = new ViewModel.ReadyForShipmentVM (value);
				readyforshipmentfilter1.UoW = value;
				viewModel.Filter = readyforshipmentfilter1;
				tableReadyForShipment.RepresentationModel = viewModel;
				tableReadyForShipment.RepresentationModel.UpdateNodes ();
			}
		}

		public ReadyForShipmentView ()
		{
			this.Build ();
			this.TabName = "Готовые к отправке";
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			tableReadyForShipment.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonOpen.Sensitive = tableReadyForShipment.Selection.CountSelectedRows () > 0;
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			var node = tableReadyForShipment.GetSelectedNode () as ViewModel.ReadyForShipmentVMNode;
			var dlg = new ReadyForShipmentDlg ( node.Id, viewModel.Filter.RestrictWarehouse);
			TabParent.AddTab (dlg, this);
		}

		protected void OnTableReadyForShipmentRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonOpen.Click ();
		}

		protected void OnSearchentity2TextChanged(object sender, EventArgs e)
		{
			tableReadyForShipment.SearchHighlightText = searchentity2.Text;
			tableReadyForShipment.RepresentationModel.SearchString = searchentity2.Text;
		}
	}
}

