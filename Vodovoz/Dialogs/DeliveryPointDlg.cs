using System;
using QSOrmProject;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSTDI;
using QSValidation;
using QSContacts;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DeliveryPointDlg : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger();
		protected ISession session;
		protected Adaptor adaptor = new Adaptor();
		protected IDeliveryPointOwner DeliveryPointOwner;

		public bool HasChanges {
			get {return Session.IsDirty();}
		}

		#region ITdiTab implementation
		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Новая точка доставки";
		public string TabName
		{
			get{return _tabName;}
			set{
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}
		}
		#endregion

		#region IOrmDialog implementation
		public NHibernate.ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set {
				session = value;
			}
		}
		#endregion

		OrmParentReference parentReference;
		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IDeliveryPointOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IDeliveryPointOwner)));
					}
					DeliveryPointOwner = (IDeliveryPointOwner)parentReference.ParentObject;
				}
			}
			get {
				return parentReference;
			}
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (TabParent.CheckClosingSlaveTabs ((ITdiTab)this))
				return;

			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

		public override void Destroy()
		{
			adaptor.Disconnect();
			base.Destroy();
		}


		private DeliveryPoint subject;

		public object Subject {
			get {
				return subject;
			}
			set {
				if (value is DeliveryPoint)
					subject = value as DeliveryPoint;
			}
		}

		public DeliveryPointDlg(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new DeliveryPoint();
			DeliveryPointOwner.DeliveryPoints.Add (subject);
			ConfigureDlg();
		}

		public DeliveryPointDlg(OrmParentReference parentReference, DeliveryPoint sub)
		{
			this.Build();
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			//referenceContact.SubjectType = typeof(Contact);
		}

		public bool Save ()
		{
			var valid = new QSValidator<DeliveryPoint> (subject);
			if (valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			OrmMain.DelayedNotifyObjectUpdated(ParentReference.ParentObject, subject);
			return true;
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}
	}
}

