using System;
using System.Data.Bindings;
using NHibernate;
using NLog;
using QSOrmProject;
using QSTDI;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementNonFreeRent : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger();
		protected ISession session;
		protected Adaptor adaptor = new Adaptor();
		protected IAdditionalAgreementOwner AgreementOwner;

		public bool HasChanges {
			get {return Session.IsDirty();}
		}

		#region ITdiTab implementation
		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Новое доп. соглашение";
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
					if (!(parentReference.ParentObject is IAdditionalAgreementOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IAdditionalAgreementOwner)));
					}
					AgreementOwner = (IAdditionalAgreementOwner)parentReference.ParentObject;
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


		private NonfreeRentAgreement subject;
		public object Subject {
			get {
				return subject;
			}
			set {
				if (value is NonfreeRentAgreement)
					subject = value as NonfreeRentAgreement;
			}
		}
			
		public AdditionalAgreementNonFreeRent(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new NonfreeRentAgreement();
			AgreementOwner.AdditionalAgreements.Add (subject);
			ConfigureDlg();
		}

		public AdditionalAgreementNonFreeRent(OrmParentReference parentReference, NonfreeRentAgreement sub)
		{
			this.Build();
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.AgreementType + " " + subject.AgreementNumber;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			entryAgreementNumber.IsEditable = true;
		}

		public bool Save ()
		{
			var valid = new QSValidator<NonfreeRentAgreement> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			OrmMain.DelayedNotifyObjectUpdated(ParentReference.ParentObject, subject);
			return true;
		}
	}
}

