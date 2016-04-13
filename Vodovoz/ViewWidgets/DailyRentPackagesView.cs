using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DailyRentPackagesView : Gtk.Bin
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		GenericObservableList<PaidRentEquipment> equipment;

		public DailyRentPackagesView ()
		{
			this.Build ();

			treeRentPackages.ColumnsConfig = ColumnsConfigFactory.Create<PaidRentEquipment>()
				.AddColumn("Пакет").AddTextRenderer(x => x.PackageName)
				.AddColumn("Оборудование").AddTextRenderer(x => x.EquipmentName)
				.AddColumn("Цена аренды (в сутки)").AddTextRenderer(x => x.PriceString)
				.AddColumn("Серийный номер").AddTextRenderer(x => x.EquipmentSerial)
				.Finish();

			treeRentPackages.Selection.Changed += OnSelectionChanged;
			UpdateTotalLabels ();
		}

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonAdd.Sensitive = buttonDelete.Sensitive = treeRentPackages.Sensitive = value;
			} 
		}

		private IUnitOfWorkGeneric<DailyRentAgreement> agreementUoW;

		public IUnitOfWorkGeneric<DailyRentAgreement> AgreementUoW {
			get { return agreementUoW; }
			set {
				if (agreementUoW == value)
					return;
				agreementUoW = value;
				if (AgreementUoW.Root.Equipment == null)
					AgreementUoW.Root.Equipment = new List<PaidRentEquipment> ();
				equipment = AgreementUoW.Root.ObservableEquipment;
				equipment.ElementChanged += Equipment_ElementChanged; 
				treeRentPackages.ItemsDataSource = equipment;
				UpdateTotalLabels ();
			}
		}

		void Equipment_ElementChanged (object aList, int[] aIdx)
		{
			UpdateTotalLabels ();
		}

		public void UpdateTotalLabels ()
		{
			Decimal TotalPrice = 0;
			Decimal TotalDeposit = 0;
			if (equipment != null)
				foreach (PaidRentEquipment eq in equipment) {
					TotalPrice += eq.Price;
					TotalDeposit += eq.Deposit;
				}
			if (AgreementUoW != null) {
				labelTotalPrice.Text = CurrencyWorks.GetShortCurrencyString (TotalPrice * AgreementUoW.Root.RentDays);
				labelTotalDeposit.Text = CurrencyWorks.GetShortCurrencyString (TotalDeposit);
			}
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = treeRentPackages.Selection.CountSelectedRows () > 0;
			buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			var availableTypes = EquipmentTypeRepository.GetPaidRentEquipmentTypes (AgreementUoW);

			//TODO FIXME Filter used equipment
			var Query = EquipmentRepository.GetEquipmentWithTypesQuery (availableTypes);
			OrmReference SelectDialog = new OrmReference (typeof(Equipment), 
				                            AgreementUoW, 
				                            Query.GetExecutableQueryOver (AgreementUoW.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += EquipmentSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void EquipmentSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			PaidRentEquipment eq = new PaidRentEquipment ();
			eq.Equipment = (Equipment)e.Subject;
			var rentPackage = AgreementUoW.Session.CreateCriteria (typeof(PaidRentPackage))
				.List<PaidRentPackage> ()
				.First (p => p.EquipmentType == eq.Equipment.Nomenclature.Type);
			eq.Deposit = rentPackage.Deposit;
			eq.PaidRentPackage = rentPackage;
			eq.Price = rentPackage.PriceDaily;
			equipment.Add (eq);
			UpdateTotalLabels ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (treeRentPackages.GetSelectedObjects ().Length == 1)
				equipment.Remove (treeRentPackages.GetSelectedObjects () [0] as PaidRentEquipment);
		}
	}
}

