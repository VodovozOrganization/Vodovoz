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
		private Adaptor adaptor = new Adaptor();
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
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
			enumType.DataSource = adaptor;
			enumVAT.DataSource = adaptor;
			referenceUnit.SubjectType = typeof(MeasurementUnits);
			referenceColor.SubjectType = typeof (EquipmentColors);
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
			spinWeight.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.rent);
			labelManufacturer.Sensitive = referenceManufacturer.Sensitive = (selected == NomenclatureCategory.equipment);
			labelColor.Sensitive = referenceColor.Sensitive = (selected == NomenclatureCategory.equipment);
			labelClass.Sensitive =  referenceType.Sensitive = (selected == NomenclatureCategory.equipment || selected == NomenclatureCategory.rent);
			labelModel.Sensitive = entryModel.Sensitive = (selected == NomenclatureCategory.equipment) ;
			labelSerial.Sensitive = checkSerial.Sensitive = (selected == NomenclatureCategory.equipment) ;
			labelDeposit.Sensitive = spinDeposit.Sensitive = (selected == NomenclatureCategory.rent);
		}
	}
}

