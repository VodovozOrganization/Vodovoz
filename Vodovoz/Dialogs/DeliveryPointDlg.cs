using System;
using QSOrmProject;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSTDI;
using QSValidation;
using NHibernate.Criterion;
using QSProjectsLib;
using System.Collections.Generic;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DeliveryPointDlg : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();
		protected Adaptor adaptor = new Adaptor ();
		protected IDeliveryPointOwner DeliveryPointOwner;
		protected bool isSaveButton;
		protected DeliveryPoint subjectCopy;

		public bool HasChanges {
			get { return false; } //FIXME }
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Новая точка доставки";

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
					if (!(parentReference.ParentObject is IDeliveryPointOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IDeliveryPointOwner)));
					}
					DeliveryPointOwner = (IDeliveryPointOwner)parentReference.ParentObject;
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
					(parentReference.ParentObject as IDeliveryPointOwner).DeliveryPoints.Remove (subject);
				else {
					ObjectCloner.FieldsCopy<DeliveryPoint> (subjectCopy, ref subject);
					subject.FirePropertyChanged ();
				}
			}
			adaptor.Disconnect ();
			base.Destroy ();
		}


		private DeliveryPoint subject;

		public object Subject {
			get { return subject; }
			set {
				if (value is DeliveryPoint)
					subject = value as DeliveryPoint;
			}
		}

		public DeliveryPointDlg (OrmParentReference parentReference)
		{
			this.Build ();
			ParentReference = parentReference;
			subject = new DeliveryPoint ();
			TabName = "Новая точка доставки";
			(parentReference.ParentObject as IDeliveryPointOwner).DeliveryPoints.Add (subject);
			ConfigureDlg ();
			subjectCopy = ObjectCloner.Clone<DeliveryPoint> (subject);
		}

		public DeliveryPointDlg (OrmParentReference parentReference, DeliveryPoint sub)
		{
			this.Build ();
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.Name;
			ConfigureDlg ();
			subjectCopy = ObjectCloner.Clone<DeliveryPoint> (sub);
		}

		private void ConfigureDlg ()
		{
			entryPhone.SetDefaultCityCode ("812");
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			referenceLogisticsArea.SubjectType = typeof(LogisticsArea);
			referenceLogisticsArea.Sensitive = QSMain.User.Permissions ["logistican"];
			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);
			entryPhone.ValidationMode = QSWidgetLib.ValidationType.phone;
			referenceContact.SubjectType = typeof(Contact);
			referenceContact.ParentReference = new OrmParentReference (ParentReference.Session, ParentReference.ParentObject, "Contacts");
			entryCity.FocusOutEvent += FocusOut;
			entryStreet.FocusOutEvent += FocusOut;
			entryRegion.FocusOutEvent += FocusOut;
			entryBuilding.FocusOutEvent += FocusOut;
		}

		void FocusOut (object o, Gtk.FocusOutEventArgs args)
		{
			SetLogisticsArea ();
		}

		public bool Save ()
		{
			var valid = new QSValidator<DeliveryPoint> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			subject.IsNew = false;
			OrmMain.DelayedNotifyObjectUpdated (ParentReference.ParentObject, subject);
			return true;
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		protected void SetLogisticsArea ()
		{
			IList <DeliveryPoint> sameAddress = ParentReference.Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.Eq ("Region", subject.Region))
				.Add (Restrictions.Eq ("City", subject.City))
				.Add (Restrictions.Eq ("Street", subject.Street))
				.Add (Restrictions.Eq ("Building", subject.Building))
				.Add (Restrictions.IsNotNull ("LogisticsArea"))
			    .Add (Restrictions.Not (Restrictions.Eq ("Id", subject.Id)))
				.List<DeliveryPoint> ();
			if (sameAddress.Count > 0) {
				subject.LogisticsArea = sameAddress [0].LogisticsArea;
			}
		}
	}
}

