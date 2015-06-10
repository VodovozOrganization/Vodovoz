using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using NHibernate.Criterion;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class EquipmentGenerator : Gtk.Bin, ITdiDialog
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot ();
		bool isDupSet;

		#region ITdiTab implementation

		public ITdiTabParent TabParent { set; get; }

		public List<Equipment> RegisteredEquipment {private set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

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

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			throw new NotImplementedException ();
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

		void PreparedReport()
		{
			isDupSet = printTwo.Active;
			if (RegisteredEquipment == null)
				return;
			string ReportPath = System.IO.Path.Combine (Directory.GetCurrentDirectory (), "Reports", "Equipment" + ".rdl");
			DBWorks.SQLHelper Parameters = new DBWorks.SQLHelper ("dup={0}&equipment_id=", printTwo.Active ? 1 : 0);
			Parameters.StartNewList ("", ",");
			foreach(var equipment in RegisteredEquipment)
			{
				Parameters.AddAsList (equipment.Id.ToString ());
			}

			reportviewer2.LoadReport (new Uri (ReportPath), Parameters.Text, QSMain.ConnectionString);
		}

		protected void OnReferenceNomenclatureChanged (object sender, EventArgs e)
		{
			labelModel.LabelProp = (referenceNomenclature.Subject as Nomenclature).Model;
			TestCanRegister ();
		}

		void TestCanRegister()
		{
			bool nomenclatureOk = referenceNomenclature.Subject != null;
			bool countOk = spinAmount.ValueAsInt > 0;
			bool warrantyOk = !dateWarrantyEnd.IsEmpty;

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

			if(result)
			{
				RegisteredEquipment = new List<Equipment> ();
				for(int count = 0; count < spinAmount.ValueAsInt; count++)
				{
					var newEquipment = new Equipment {
						Nomenclature = (referenceNomenclature.Subject as Nomenclature),
						WarrantyEndDate = dateWarrantyEnd.Date
					};
					uow.Save (newEquipment);
					RegisteredEquipment.Add (newEquipment);
				}
				uow.Commit ();

				//Деактивируем создание оборудования
				referenceNomenclature.Sensitive = spinAmount.Sensitive = 
					dateWarrantyEnd.Sensitive = buttonCreate.Sensitive = false;
				PreparedReport ();
				buttonAddAndClose.Sensitive = true;
			}
		}

		protected void OnPrintTwoToggled (object sender, EventArgs e)
		{
			if (isDupSet != printTwo.Active)
				PreparedReport ();
		}


	}
}

