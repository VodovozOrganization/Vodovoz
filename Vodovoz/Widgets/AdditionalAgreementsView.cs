using System;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using System.Collections.Generic;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementsView : Gtk.Bin
	{
		private IAdditionalAgreementOwner agreementOwner;
		private GenericObservableList<AdditionalAgreement> additionalAgreements;
		private ISession session;

		public ISession Session {
			get { return session; }
			set { session = value; }
		}

		public IAdditionalAgreementOwner AgreementOwner {
			get { return agreementOwner; }
			set {
				agreementOwner = value;
				if (agreementOwner.AdditionalAgreements == null)
					AgreementOwner.AdditionalAgreements = new List<AdditionalAgreement> ();
				additionalAgreements = new GenericObservableList<AdditionalAgreement> (AgreementOwner.AdditionalAgreements);
				treeAdditionalAgreements.ItemsDataSource = additionalAgreements;
			}
		}

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IAdditionalAgreementOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IAdditionalAgreementOwner)));
					}
					AgreementOwner = (IAdditionalAgreementOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
		}

		public AdditionalAgreementsView ()
		{
			this.Build ();
			treeAdditionalAgreements.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeAdditionalAgreements.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		void OnButtonAddClicked (AgreementType type)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;
			AdditionalAgreement agreement;

			ITdiDialog dlg;
			switch (type) {
			case AgreementType.FreeRent:
				agreement = new FreeRentAgreement ();
				dlg = new AdditionalAgreementFreeRent (ParentReference, agreement as FreeRentAgreement);
				break;
			case AgreementType.NonfreeRent:
				agreement = new NonfreeRentAgreement ();
				dlg = new AdditionalAgreementNonFreeRent (ParentReference, agreement as NonfreeRentAgreement);
				break;
			case AgreementType.Repair:
				agreement = new RepairAgreement ();
				dlg = new AdditionalAgreementRepair (ParentReference, agreement as RepairAgreement);
				break;
			case AgreementType.WaterSales:
				agreement = new WaterSalesAgreement ();
				dlg = new AdditionalAgreementWater (ParentReference, agreement as WaterSalesAgreement);
				break;
			case AgreementType.DailyRent:
				agreement = new DailyRentAgreement ();
				dlg = new AdditionalAgreementDailyRent (ParentReference, agreement as DailyRentAgreement);
				break;
			default:
				throw new NotSupportedException (String.Format ("Тип {0} пока не поддерживается.", type));
			}
			agreement.Contract = (agreementOwner as CounterpartyContract);
			additionalAgreements.Add (agreement);

			//Вычисляем номер для нового соглашения.
			var numbers = new List<int> ();
			foreach (AdditionalAgreement a in additionalAgreements) {
				int res;
				if (Int32.TryParse (a.AgreementNumber, out res))
					numbers.Add (res);
			}
			numbers.Sort ();
			String number = "00";
			if (numbers.Count > 0) {
				number += (numbers [numbers.Count - 1] + 1).ToString ();
				number = number.Substring (number.Length - 3, 3);
			} else
				number += "1";
			agreement.AgreementNumber = number;
			agreement.IsNew = true;

			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;
				
			ITdiDialog dlg = OrmMain.CreateObjectDialog (ParentReference, treeAdditionalAgreements.GetSelectedObjects () [0]);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnTreeAdditionalAgreementsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			additionalAgreements.Remove (treeAdditionalAgreements.GetSelectedObjects () [0] as AdditionalAgreement);
		}

		protected void OnButtonAddEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			OnButtonAddClicked ((AgreementType)e.ItemEnum);
		}
	}
}

