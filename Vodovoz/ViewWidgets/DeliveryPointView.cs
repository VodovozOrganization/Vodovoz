using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gtk;
using NHibernate;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DeliveryPointView : Gtk.Bin
	{
		GenericObservableList<DeliveryPoint> deliveryPoints;

		IUnitOfWorkGeneric<Counterparty> deliveryPointUoW;

		public IUnitOfWorkGeneric<Counterparty> DeliveryPointUoW {
			get { return deliveryPointUoW; }
			set {
				if (deliveryPointUoW == value)
					return;
				deliveryPointUoW = value;
				if (DeliveryPointUoW.Root.DeliveryPoints == null)
					DeliveryPointUoW.Root.DeliveryPoints = new List<DeliveryPoint> ();
				deliveryPoints = DeliveryPointUoW.Root.ObservableDeliveryPoints;
				treeDeliveryPoints.RepresentationModel = new ViewModel.DeliveryPointsVM (value);
				treeDeliveryPoints.RepresentationModel.UpdateNodes ();
			}
		}

		public DeliveryPointView ()
		{
			this.Build ();
			treeDeliveryPoints.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeDeliveryPoints.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = new DeliveryPointDlg (DeliveryPointUoW.Root);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = OrmMain.CreateObjectDialog (treeDeliveryPoints.GetSelectedObjects () [0]);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnTreeDeliveryPointsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			deliveryPoints.Remove (treeDeliveryPoints.GetSelectedObjects () [0] as DeliveryPoint);
		}
	}
}

