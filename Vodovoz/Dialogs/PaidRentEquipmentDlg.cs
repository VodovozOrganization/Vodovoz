using System;
using System.Data.Bindings;
using QSOrmProject;
using QSTDI;
using NHibernate;
using NLog;
using NHibernate.Criterion;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaidRentEquipmentDlg : Gtk.Bin, ITdiDialog, IOrmDialog
	{
		public bool DailyRent;
		protected PaidRentEquipment subjectCopy;
		protected bool isSaveButton;
		static Logger logger = LogManager.GetCurrentClassLogger ();
		bool loadFromPackage;
		ISession session;
		Adaptor adaptor = new Adaptor ();
		PaidRentEquipment subject;
		IPaidRentEquipmentOwner PaidRentOwner;

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public bool HasChanges {
			get { return false; }
		}

		string _tabName = "Новое оборудование к доп. соглашению";

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
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		public object Subject {
			get { return subject; }
			set {
				if (value is PaidRentEquipment)
					subject = value as PaidRentEquipment;
			}
		}

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IPaidRentEquipmentOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IPaidRentEquipmentOwner)));
					}
					PaidRentOwner = (IPaidRentEquipmentOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
		}

		public PaidRentEquipmentDlg (OrmParentReference parentReference, PaidRentEquipment sub)
		{
			this.Build ();
			ParentReference = parentReference;
			subject = sub;
			loadFromPackage = subject.IsNew;
			if (subject.Equipment != null && subject.PaidRentPackage != null)
				TabName = subject.EquipmentName + " " + subject.PackageName;
			subjectCopy = ObjectCloner.Clone<PaidRentEquipment> (sub);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			labelPrice.Text = labelDeposit.Text = "";
			referenceEquipment.SubjectType = typeof(Equipment);
			referencePaidRentPackage.SubjectType = typeof(PaidRentPackage);
			referencePaidRentPackage.Changed += OnReferencePaidRentPackageChanged;
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			if (referenceEquipment.ItemsCriteria == null)
				referenceEquipment.ItemsCriteria = Session.CreateCriteria<Equipment> ();
			referenceEquipment.ItemsCriteria.Add (EquipmentWorks.FilterUsedEquipment (Session));
			if (subject.PaidRentPackage != null)
				referenceEquipment.ItemsCriteria
					.CreateAlias ("Nomenclature", "n")
					.Add (Restrictions.Eq ("n.Type", subject.PaidRentPackage.EquipmentType));
		}

		public bool Save ()
		{
			var valid = new QSValidator<PaidRentEquipment> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			subject.IsNew = false;
			OrmMain.DelayedNotifyObjectUpdated (ParentReference.ParentObject, subject);
			return true;
		}

		public override void Destroy ()
		{
			if (!isSaveButton) {
				if (subject.IsNew)
					(parentReference.ParentObject as IPaidRentEquipmentOwner).Equipment.Remove (subject);
				else {
					ObjectCloner.FieldsCopy<PaidRentEquipment> (subjectCopy, ref subject);
					subject.FirePropertyChanged ();
				}
			}
			adaptor.Disconnect ();
			base.Destroy ();
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (Save ()) {
				isSaveButton = true;
				OnCloseTab (false);
			}
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

		protected void OnReferencePaidRentPackageChanged (object sender, EventArgs e)
		{
			if (loadFromPackage) {	//Загружаем все значения из выбранного пакета.
				subject.Price = DailyRent ? 
					(referencePaidRentPackage.Subject as PaidRentPackage).PriceDaily :
					(referencePaidRentPackage.Subject as PaidRentPackage).PriceMonthly;
				//subject.Deposit = (referencePaidRentPackage.Subject as PaidRentPackage).DepositService.RentPrice;
			} else					//Загружаем уже сохраненное значение
				loadFromPackage = true;
			if (subject.PaidRentPackage != null)
				referenceEquipment.ItemsCriteria
					.CreateAlias ("Nomenclature", "n")
					.Add (Restrictions.Eq ("n.Type", subject.PaidRentPackage.EquipmentType));
			UpdatePrice ();
		}

		protected void UpdatePrice ()
		{
			labelPrice.Text = String.Format ("{0} руб. в " + (DailyRent ? "сутки" : "месяц"), subject.Price);
			labelDeposit.Text = String.Format ("{0} руб.", subject.Deposit);
		}
	}
}

