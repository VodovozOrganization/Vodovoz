using System;
using System.Data.Bindings;
using QSOrmProject;
using QSTDI;
using NHibernate;
using NLog;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class NomenclatureDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptorOrg = new Adaptor();
		private Nomenclature subject;
		private bool NewItem = false;

		public ITdiTabParent TabParent { set; get;}

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public bool HasChanges { 
			get{return NewItem || Session.IsDirty();}
		}

		private string _tabName = "Новая номенклатура";
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

		public ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession();
				return session;
			}
			set {
				session = value;
			}
		}

		public object Subject
		{
			get {return subject;}
			set {
				if (value is Nomenclature)
					subject = value as Nomenclature;
			}
		}

		public NomenclatureDlg()
		{
			this.Build();
			NewItem = true;
			subject = new Nomenclature();
			ConfigureDlg();
		}

		public NomenclatureDlg(int id)
		{
			this.Build();
			subject = Session.Load<Nomenclature>(id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public NomenclatureDlg(Nomenclature sub)
		{
			this.Build();
			subject = Session.Load<Nomenclature>(sub.Id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			spinWeight.Sensitive = false;
			entryName.IsEditable = true;
			adaptorOrg.Target = subject;
			datatable1.DataSource = adaptorOrg;
			enumType.DataSource = adaptorOrg;
			referenceUnit.SubjectType = typeof(MeasurementUnits);
			referenceColor.SubjectType = typeof (Colors);
			referenceManufacturer.SubjectType = typeof(Manufacturer);
			referenceType.SubjectType = typeof(EquipmentType);
			ConfigureInputs((NomenclatureCategory)enumType.Active);
		}

		public bool Save()
		{
			logger.Info("Сохраняем номенклатуру...");
			Session.SaveOrUpdate(subject);
			Session.Flush();
			OrmMain.NotifyObjectUpdated(subject);
			return true;
		}

		public override void Destroy()
		{
			Session.Close();
			base.Destroy();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab(false);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}

		protected void OnEnumTypeChanged (object sender, EventArgs e)
		{
			ConfigureInputs ((NomenclatureCategory)enumType.Active);
		}

		protected void ConfigureInputs (NomenclatureCategory selected) {
			referenceUnit.Sensitive = true;
			labelManufacturer.Sensitive = referenceManufacturer.Sensitive = false;
			labelColor.Sensitive = referenceColor.Sensitive = false;
			labelClass.Sensitive =  referenceType.Sensitive = false;
			labelModel.Sensitive = entryModel.IsEditable = false;
			labelSerial.Sensitive = checkSerial.Sensitive = false;
			labelDeposit.Sensitive = spinDeposit.Sensitive = false;

			if (selected == NomenclatureCategory.equipment) {
				labelManufacturer.Sensitive = referenceManufacturer.Sensitive = true;
				labelColor.Sensitive = referenceColor.Sensitive = true;
				labelClass.Sensitive = referenceType.Sensitive = true;
				labelModel.Sensitive = entryModel.IsEditable = true;
				labelSerial.Sensitive = checkSerial.Sensitive = true;
			} else if (selected == NomenclatureCategory.rent) {
				labelClass.Sensitive = referenceType.Sensitive = true;
				//referenceColor.Subject = null;
				//referenceManufacturer = null;
				labelDeposit.Sensitive = spinDeposit.IsEditable = true;
			}
		}
	}
}

