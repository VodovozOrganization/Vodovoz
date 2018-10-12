using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class BottleReceptionView : Gtk.Bin
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
				viewModel = new BottleReceptionVM (value);
				ytreeBottles.RepresentationModel = viewModel;
				ytreeBottles.RepresentationModel.UpdateNodes ();
			}
		}

		public bool Sensitive {
			set {
				ytreeBottles.Sensitive = value;
			}
		}

		BottleReceptionVM viewModel;

		public BottleReceptionView ()
		{
			this.Build ();
		}

		public IList<BottleReceptionVMNode> Items{get { return viewModel.ItemsList as IList<BottleReceptionVMNode>; }}
	}
}

