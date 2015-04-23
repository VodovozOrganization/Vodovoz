using System;
using QSTDI;
using QSOrmProject;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyContractDlg : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();
		protected ISession session;
		protected Adaptor adaptor = new Adaptor ();
		protected IContractOwner ContractOwner;
		protected bool isSaveButton;
		protected CounterpartyContract subjectCopy;

		public bool HasChanges {
			get { return false; }
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		protected string _tabName = "Новый договор с контрагентом";

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

		#region IOrmDialog implementation

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		#endregion

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IContractOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IContractOwner)));
					}
					ContractOwner = (IContractOwner)parentReference.ParentObject;
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
					(parentReference.ParentObject as IContractOwner).CounterpartyContracts.Remove (subject);
				else {
					ObjectCloner.FieldsCopy<CounterpartyContract> (subjectCopy, ref subject);
					subject.FirePropertyChanged ();
				}
			}
			adaptor.Disconnect ();
			base.Destroy ();
		}


		private CounterpartyContract subject;

		public object Subject {
			get { return subject; }
			set {
				if (value is CounterpartyContract)
					subject = value as CounterpartyContract;
			}
		}

		public CounterpartyContractDlg (OrmParentReference parentReference, CounterpartyContract sub)
		{
			this.Build ();
			ParentReference = parentReference;
			subject = sub;
			TabName = subject.Number;
			ConfigureDlg ();
			subjectCopy = ObjectCloner.Clone<CounterpartyContract> (sub);
		}

		private void ConfigureDlg ()
		{
			adaptor.Target = subject;
			datatable5.DataSource = adaptor;
			referenceOrganization.SubjectType = typeof(Organization);
			additionalagreementsview1.ParentReference = new OrmParentReference (Session, (Subject as CounterpartyContract), "AdditionalAgreements");

		}

		public bool Save ()
		{
			var valid = new QSValidator<CounterpartyContract> (subject);
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
	}
}

