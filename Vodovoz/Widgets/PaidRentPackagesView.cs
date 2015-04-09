using System;
using QSTDI;
using QSOrmProject;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using System.Collections.Generic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaidRentPackagesView : Gtk.Bin
	{
		GenericObservableList<PaidRentEquipment> equipments;

		bool dailyRent;

		public bool DailyRent {
			get { return dailyRent; }
			set {
				dailyRent = value; 
				if (DailyRent)
					treeRentPackages.Columns [2].Title = "Цена аренды (в сутки)";
				else
					treeRentPackages.Columns [2].Title = "Цена аренды (в месяц)";
				UpdateTotalLabels ();
			}
		}


		public ISession Session { get; set; }

		IPaidRentEquipmentOwner equipmentOwner;

		public IPaidRentEquipmentOwner EquipmentOwner {
			get { return equipmentOwner; }
			set {
				equipmentOwner = value;
				if (equipmentOwner.Equipment == null)
					equipmentOwner.Equipment = new List<PaidRentEquipment> ();
				equipments = new GenericObservableList<PaidRentEquipment> (EquipmentOwner.Equipment);
				foreach (PaidRentEquipment eq in equipments)
					eq.PropertyChanged += EquimentPropertyChanged;
				UpdateTotalLabels ();
				treeRentPackages.ItemsDataSource = equipments;
			}
		}

		void EquimentPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateTotalLabels ();
		}

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IPaidRentEquipmentOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference " +
						"должен реализовывать интерфейс {0}", typeof(IAdditionalAgreementOwner)));
					}
					EquipmentOwner = (IPaidRentEquipmentOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
		}

		void UpdateTotalLabels ()
		{
			Decimal TotalPrice = 0;
			if (equipments != null)
				foreach (PaidRentEquipment eq in equipments)
					TotalPrice += eq.Price;
			labelTotalPrice.Text = String.Format ("{0} руб.", 
				(DailyRent ? TotalPrice * (parentReference.ParentObject as DailyRentAgreement).RentDays : TotalPrice));
		}

		public PaidRentPackagesView ()
		{
			this.Build ();
			treeRentPackages.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeRentPackages.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;
			PaidRentEquipment equipment = new PaidRentEquipment ();
			equipment.IsNew = true;
			equipments.Add (equipment);
			equipment.PropertyChanged += EquimentPropertyChanged;
			ITdiDialog dlg = new PaidRentEquipmentDlg (ParentReference, equipment);
			(dlg as PaidRentEquipmentDlg).DailyRent = DailyRent;
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = OrmMain.CreateObjectDialog (ParentReference, treeRentPackages.GetSelectedObjects () [0]);
			(dlg as PaidRentEquipmentDlg).DailyRent = DailyRent;
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			equipments.Remove (treeRentPackages.GetSelectedObjects () [0] as PaidRentEquipment);
		}

		protected void OnTreeRentPackagesRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}
	}
}

