using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QS.Tdi;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	public partial class EquipmentGenerator : Gtk.Bin, ITdiDialog
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot ();
		bool isDupSet;

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public event EventHandler<EquipmentCreatedEventArgs> EquipmentCreated;

		#region ITdiTab implementation

		public ITdiTabParent TabParent { set; get; }

		public bool FailInitialize { get; protected set; }

		public List<Equipment> RegisteredEquipment { private set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;
		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }

		private string _tabName = "Регистрация оборудования";

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

		public bool CompareHashName(string hashName)
		{
			return GenerateHashName() == hashName;
		}

		public static string GenerateHashName()
		{
			return typeof(EquipmentGenerator).Name;
		}

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			buttonAddAndClose.Click ();
			return true;
		}

		public void SaveAndClose()
		{
			buttonAddAndClose.Click ();
		}

		public bool HasChanges {
			get { return RegisteredEquipment != null; }
		}

		#endregion

		public EquipmentGenerator ()
		{
			this.Build ();

			referenceNomenclature.SubjectType = typeof(Nomenclature);
			referenceNomenclature.ItemsCriteria = uow.Session.CreateCriteria<Nomenclature> ()
				.Add (Restrictions.Eq ("Category", NomenclatureCategory.equipment))
				.Add (Restrictions.Eq ("Serial", true));
		}

		public static EquipmentGenerator CreateOne(int nomenclatureID){
			return new EquipmentGenerator (nomenclatureID);
		}

		protected EquipmentGenerator(int nomenclatureId):this()
		{
			referenceNomenclature.Subject = uow.Session.Get<Nomenclature> (nomenclatureId);
			referenceNomenclature.Sensitive = false;
			spinAmount.Value = 1;
			spinAmount.Sensitive = false;
			buttonAddAndClose.Label = "Закрыть";
		}

		void PreparedReport ()
		{
			isDupSet = printTwo.Active;
			if (RegisteredEquipment == null)
				return;
			string ReportPath = System.IO.Path.Combine (Directory.GetCurrentDirectory (), "Reports", "Equipment" + ".rdl");
			DBWorks.SQLHelper Parameters = new DBWorks.SQLHelper ("dup={0}&equipment_id=", printTwo.Active ? 1 : 0);
			Parameters.StartNewList ("", ",");
			foreach (var equipment in RegisteredEquipment) {
				Parameters.AddAsList (equipment.Id.ToString ());
			}
				
			reportviewer2.LoadReport (new Uri (ReportPath), Parameters.Text, QSMain.ConnectionString);
		}

		protected void OnReferenceNomenclatureChanged (object sender, EventArgs e)
		{
			labelModel.LabelProp = (referenceNomenclature.Subject as Nomenclature).Model;
			TestCanRegister ();
		}

		void TestCanRegister ()
		{
			bool nomenclatureOk = referenceNomenclature.Subject != null;
			bool countOk = spinAmount.ValueAsInt > 0;
			bool warrantyOk = !ydatepickerWarrantyEnd.DateOrNull.HasValue;

			buttonCreate.Sensitive = nomenclatureOk && countOk && warrantyOk;
		}

		protected void OnSpinAmountValueChanged (object sender, EventArgs e)
		{
			TestCanRegister ();
		}

		protected void OnDateWarrantyEndDateChanged (object sender, EventArgs e)
		{
			TestCanRegister ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string amount = RusNumber.FormatCase (spinAmount.ValueAsInt, 
				                "Будет зарегистрирован {0} серийный номер для оборудования ",
				                "Будет зарегистрировано {0} серийных номера для оборудования ",
				                "Будет зарегистрировано {0} серийных номеров для оборудования "
			                );
			string Message = amount + String.Format ("({0}). Вы подтверждаете регистрацию?", 
				                 (referenceNomenclature.Subject as Nomenclature).Name);
			MessageDialog md = new MessageDialog ((Window)this.Toplevel, DialogFlags.Modal,
				                   MessageType.Question, 
				                   ButtonsType.YesNo,
				                   Message);
			bool result = md.Run () == (int)ResponseType.Yes;
			md.Destroy ();

			if (result) {
				RegisteredEquipment = new List<Equipment> ();
				for (int count = 0; count < spinAmount.ValueAsInt; count++) {
					var newEquipment = new Equipment {
						Nomenclature = (referenceNomenclature.Subject as Nomenclature),
						WarrantyEndDate = ydatepickerWarrantyEnd.DateOrNull
					};
					uow.Save (newEquipment);
					RegisteredEquipment.Add (newEquipment);
				}
				uow.Commit ();

				//Деактивируем создание оборудования
				referenceNomenclature.Sensitive = spinAmount.Sensitive = 
					ydatepickerWarrantyEnd.Sensitive = buttonCreate.Sensitive = false;
				PreparedReport ();
				buttonAddAndClose.Sensitive = true;
			}
		}

		protected void OnPrintTwoToggled (object sender, EventArgs e)
		{
			if (isDupSet != printTwo.Active)
				PreparedReport ();
		}


		protected void OnButtonAddAndCloseClicked (object sender, EventArgs e)
		{
			if (EquipmentCreated != null) {
				EquipmentCreated (this, new EquipmentCreatedEventArgs (
					RegisteredEquipment.ToArray ()
				));
			}
			OnCloseTab ();
		}

		protected void OnCloseTab ()
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (false));
		}
	}

	public class EquipmentCreatedEventArgs : EventArgs
	{
		public Equipment[] Equipment { get; private set; }

		public EquipmentCreatedEventArgs (Equipment[] equipment)
		{
			Equipment = equipment;
		}
	}

}

