using System;
using System.Data.Bindings;
using NHibernate;
using NLog;
using QSOrmProject;
using QSTDI;
using QSValidation;
using System.Collections.Generic;
using NHibernate.Criterion;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementNonFreeRent : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		protected NonfreeRentAgreement subjectCopy;
		protected bool isSaveButton;
		protected static Logger logger = LogManager.GetCurrentClassLogger ();
		protected Adaptor adaptor = new Adaptor ();
		protected IAdditionalAgreementOwner AgreementOwner;

		public bool HasChanges {
			get { return false; }
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Новое доп. соглашение";

		public string TabName {
			get { return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
			}
		}

		#endregion

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					if (!(parentReference.ParentObject is IAdditionalAgreementOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IAdditionalAgreementOwner)));
					}
					AgreementOwner = (IAdditionalAgreementOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (Save ()) {
				isSaveButton = true;
				OnCloseTab (false);
			}
		}

		protected void OnCloseTab (bool askSave)
		{
			if (TabParent.CheckClosingSlaveTabs ((ITdiTab)this))
				return;

			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		public override void Destroy ()
		{
			if (!isSaveButton) {
				if (subject.IsNew)
					(parentReference.ParentObject as IAdditionalAgreementOwner).AdditionalAgreements.Remove (subject);
				else
					ObjectCloner.FieldsCopy<NonfreeRentAgreement> (subjectCopy, ref subject);
			}
			adaptor.Disconnect ();
			base.Destroy ();
		}

		private NonfreeRentAgreement subject;

		public object Subject {
			get { return subject; }
			set {
				if (value is NonfreeRentAgreement)
					subject = value as NonfreeRentAgreement;
			}
		}

		public AdditionalAgreementNonFreeRent (OrmParentReference parentReference, NonfreeRentAgreement sub)
		{
			this.Build ();
			subjectCopy = ObjectCloner.Clone<NonfreeRentAgreement> (sub);
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.AgreementTypeTitle + " " + subject.AgreementNumber;
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			entryAgreementNumber.IsEditable = true;

			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in (parentReference.ParentObject as CounterpartyContract).Counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = ParentReference.Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.In ("Id", identifiers));
			dataAgreementType.Text = (parentReference.ParentObject as CounterpartyContract).Number + " - А";

			paidrentpackagesview1.ParentReference = new OrmParentReference (ParentReference.Session, subject, "Equipment");
			paidrentpackagesview1.DailyRent = false;
		}

		public bool Save ()
		{
			var valid = new QSValidator<NonfreeRentAgreement> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			subject.IsNew = false;
			OrmMain.DelayedNotifyObjectUpdated (ParentReference.ParentObject, subject);
			return true;
		}
	}
}

