using System;
using QSOrmProject;
using Vodovoz.ViewModel;
using System.Collections.Generic;

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

