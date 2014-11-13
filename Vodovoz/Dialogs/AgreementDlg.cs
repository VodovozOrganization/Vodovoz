using System;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AgreementDlg : Gtk.Bin, QSTDI.ITdiTab
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private IAdditionalAgreementOwner additionalAgreementOwner;
		private Adaptor adaptor = new Adaptor();
		private AdditionalAgreement subject;

		OrmParentReference parentReference;
		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if(!(parentReference.ParentObject is IAdditionalAgreementOwner))
					{
						throw new ArgumentException (String.Format("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IAdditionalAgreementOwner)));
					}
					this.additionalAgreementOwner = (IAdditionalAgreementOwner)parentReference.ParentObject;
				}
			}
			get {
				return parentReference;
			}
		}

		public AgreementDlg(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new AdditionalAgreement();
			additionalAgreementOwner.AdditionalAgreements.Add (subject);
			ConfigureDlg();
		}

		public AgreementDlg(OrmParentReference parenReferance, AdditionalAgreement subject)
		{
			this.Build();
			ParentReference = parenReferance;
			this.subject = subject;
			TabName = subject.Id.ToString();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			enumType.DataSource = adaptor;
			entryNumber.IsEditable = true;
		}

		#region ITdiTab implementation
		public event EventHandler<QSTDI.TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<QSTDI.TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Новое дополнительное соглашение";
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

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			logger.Info("Сохраняем дополнительное соглашение...");
			if(additionalAgreementOwner != null)
				OrmMain.DelayedNotifyObjectUpdated (additionalAgreementOwner, subject);
			return true;
		}

		public bool HasChanges {
			get {return Session.IsDirty();}
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

		public object Subject {
			get {return subject;}
			set {
				if (value is AdditionalAgreement)
					subject = value as AdditionalAgreement;
			}
		}
		#endregion

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
	}
}

