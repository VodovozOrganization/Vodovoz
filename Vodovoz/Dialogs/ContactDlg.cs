using System;
using System.Collections.Generic;
using System.Data.Bindings;
using NHibernate;
using NLog;
using QSContacts;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ContactDlg : Gtk.Bin, ITdiDialog, IOrmSlaveDialog
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();
		protected ISession session;
		protected IContactOwner contactOwner;
		protected Adaptor adaptor = new Adaptor ();
		protected Contact subject;
		protected bool isNew = false;

		public bool HasChanges {
			get { return false; }
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public ITdiTabParent TabParent { get ; set ; }

		private string _tabName = "Новое контактное лицо";

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
					Session = OrmMain.OpenSession ();
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
					Session = parentReference.UoW.Session;
					if (!(parentReference.ParentObject is IContactOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IContactOwner)));
					}
					contactOwner = (IContactOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (Save ())
				OnCloseTab (false);
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
			adaptor.Disconnect ();
			base.Destroy ();
		}

		public object Subject {
			get { return subject; }
			set {
				if (value is Contact)
					subject = value as Contact;
			}
		}

		public ContactDlg (OrmParentReference parenReferance)
		{
			this.Build ();
			ParentReference = parenReferance;
			subject = new Contact ();
			isNew = true;
			ConfigureDlg ();
		}

		public ContactDlg (OrmParentReference parenReferance, Contact sub)
		{
			this.Build ();
			ParentReference = parenReferance;
			subject = sub;
			TabName = sub.Surname + " " + sub.Name + " " + sub.Lastname;
			ConfigureDlg ();
		}

		void ConfigureDlg ()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			entrySurname.IsEditable = entryName.IsEditable = entryLastname.IsEditable = true;
			dataComment.Editable = true;
			referencePost.SubjectType = typeof(Post);
			emailsView.Session = Session;
			if (subject.Emails == null)
				subject.Emails = new List<Email> ();
			emailsView.Emails = subject.Emails;
			phonesView.Session = Session;
			if (subject.Phones == null)
				subject.Phones = new List<Phone> ();
			phonesView.Phones = subject.Phones;
		}

		public bool Save ()
		{
			logger.Info ("Сохраняем контактное лицо...");
			if (isNew)
				(parentReference.ParentObject as Counterparty).Contacts.Add (subject);
			phonesView.SaveChanges ();
			emailsView.SaveChanges ();
			if (contactOwner != null)
				OrmMain.DelayedNotifyObjectUpdated (ParentReference.ParentObject, subject);
			return true;
		}
	}
}

