using System;
using System.Data.Bindings;
using NHibernate;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSTDI;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FreeRentPackageDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private ISession session;
		private Adaptor adaptor = new Adaptor ();
		private FreeRentPackage subject;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public bool HasChanges { 
			get { return Session.IsDirty (); }
		}

		private string _tabName = "Новый пакет бесплатных услуг";

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

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		public object Subject {
			get { return subject; }
			set {
				if (value is FreeRentPackage)
					subject = value as FreeRentPackage;
			}
		}

		public FreeRentPackageDlg ()
		{
			this.Build ();
			subject = new FreeRentPackage ();
			Session.Persist (subject);
			ConfigureDlg ();
		}

		public FreeRentPackageDlg (int id)
		{
			this.Build ();
			subject = Session.Load<FreeRentPackage> (id);
			TabName = subject.Name;
			ConfigureDlg ();
		}

		public FreeRentPackageDlg (FreeRentPackage sub)
		{
			this.Build ();
			subject = Session.Load<FreeRentPackage> (sub.Id);
			TabName = subject.Name;
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			referenceDepositService.SubjectType = typeof(Nomenclature);
			referenceEquipmentType.SubjectType = typeof(EquipmentType);
			referenceDepositService.ItemsCriteria = Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.deposit));
		}

		public bool Save ()
		{
			var valid = new QSValidator<FreeRentPackage> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем пакет бесплатных услуг...");
			Session.Flush ();
			OrmMain.NotifyObjectUpdated (subject);
			return true;
		}

		public override void Destroy ()
		{
			Session.Close ();
			adaptor.Disconnect ();
			base.Destroy ();
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save ())
				OnCloseTab (false);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}
	}
}

