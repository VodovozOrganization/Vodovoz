using System;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ReadyForShipmentView : TdiTabBase
	{
		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
				var vm = new ViewModel.ReadyForShipmentVM (value);
				readyforshipmentfilter1.UoW = value;
				vm.Filter = readyforshipmentfilter1;
				tableReadyForShipment.RepresentationModel = vm;
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


		protected void OndatatreeviewBalanceRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonOpen.Click ();
		}

		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

