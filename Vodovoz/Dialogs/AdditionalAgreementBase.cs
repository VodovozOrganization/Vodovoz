using System;
using QSTDI;
using QSOrmProject;
using NLog;
using NHibernate;
using System.Data.Bindings;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementBase : Gtk.Bin, ITdiDialog, IOrmDialog
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

		public virtual object Subject { get; set; }
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

		public virtual bool Save ()
		{
			return false;
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

		public override void Destroy()
		{
			adaptor.Disconnect();
			base.Destroy();
		}

		public AdditionalAgreementBase ()
		{
		}
	}
}

