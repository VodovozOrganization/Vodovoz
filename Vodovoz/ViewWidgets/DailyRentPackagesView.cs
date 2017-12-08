using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Repository;
using Vodovoz.Domain.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DailyRentPackagesView : WidgetOnDialogBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		GenericObservableList<PaidRentEquipment> equipments;

		public DailyRentPackagesView ()
		{
			this.Build ();

			treeRentPackages.ColumnsConfig = ColumnsConfigFactory.Create<PaidRentEquipment>()
				.AddColumn("Пакет").AddTextRenderer(x => x.PackageName)
				.AddColumn("Оборудование").AddTextRenderer(x => x.Equipment.NomenclatureName)
				.AddColumn("Серийный номер").AddTextRenderer(x => x.Equipment != null && x.Equipment.Nomenclature.IsSerial ? x.Equipment.Serial : "-")
				.AddColumn("Цена аренды (в сутки)").AddTextRenderer(x => x.PriceString)
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
				equipments = AgreementUoW.Root.ObservableEquipment;
				equipments.ElementChanged += Equipment_ElementChanged; 
				treeRentPackages.ItemsDataSource = equipments;
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
			if (equipments != null)
				foreach (PaidRentEquipment eq in equipments) {
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
			OrmReference SelectDialog = new OrmReference (AgreementUoW, EquipmentRepository.AvailableEquipmentQuery());
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanEdit;
			SelectDialog.ObjectSelected += EquipmentSelected;

			MyTab.TabParent.AddSlaveTab (MyTab, SelectDialog);
		}

		void EquipmentSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var selectedEquipment = (Equipment)e.Subject;

			var rentPackage = RentPackageRepository.GetPaidRentPackage(AgreementUoW, selectedEquipment.Nomenclature.Type);
			if (rentPackage == null)
			{
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет условий платной аренды.");
				return;
			}

			PaidRentEquipment eq = new PaidRentEquipment ();
			eq.Equipment = selectedEquipment;
			eq.Deposit = rentPackage.Deposit;
			eq.PaidRentPackage = rentPackage;
			eq.Price = rentPackage.PriceDaily;
			equipments.Add (eq);
			UpdateTotalLabels ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (treeRentPackages.GetSelectedObjects ().Length == 1)
				equipments.Remove (treeRentPackages.GetSelectedObjects () [0] as PaidRentEquipment);
		}

		protected void OnButtonAddByTypeClicked(object sender, EventArgs e)
		{
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation (new ViewModel.EquipmentTypesForRentVM (MyOrmDialog.UoW));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Выберите тип оборудования";
			SelectDialog.ObjectSelected += EquipmentByTypeSelected;

			MyTab.TabParent.AddSlaveTab (MyTab, SelectDialog);
		}

		void EquipmentByTypeSelected (object sender, ReferenceRepresentationSelectedEventArgs args)
		{
			var equipmentType = AgreementUoW.GetById<EquipmentType>(args.ObjectId);

			var rentPackage = Repository.RentPackageRepository.GetPaidRentPackage(AgreementUoW, equipmentType);
			if (rentPackage == null)
			{
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет условий платной аренды.");
				return;
			}

			var exclude = equipments.Select(e => e.Equipment.Id).ToArray();

			var selectedEquipment = EquipmentRepository.GetAvailableEquipmentForRent(AgreementUoW, equipmentType, exclude);
			if(selectedEquipment == null)
			{
				MessageDialogWorks.RunErrorDialog("Не найдено свободного оборудования выбранного типа.");
				return;
			}

			PaidRentEquipment eq = new PaidRentEquipment ();
			eq.Equipment = selectedEquipment;
			eq.Deposit = rentPackage.Deposit;
			eq.PaidRentPackage = rentPackage;
			eq.Price = rentPackage.PriceDaily;
			equipments.Add (eq);
			UpdateTotalLabels ();
		}

	}
}

