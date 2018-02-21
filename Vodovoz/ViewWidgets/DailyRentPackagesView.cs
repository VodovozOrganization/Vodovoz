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
using Gtk;
using Vodovoz.Representations;
using Vodovoz.JournalFilters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DailyRentPackagesView : WidgetOnDialogBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		GenericObservableList<PaidRentEquipment> dailyRentEquipments;

		public DailyRentPackagesView ()
		{
			this.Build ();

			treeRentPackages.ColumnsConfig = ColumnsConfigFactory.Create<PaidRentEquipment>()
				.AddColumn("Пакет").AddTextRenderer(x => x.PackageName)
				.AddColumn("Оборудование").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Количество").AddNumericRenderer(x => x.Count)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
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

		public PaidRentPackage PaidRentPackage { get; set; }

		private IUnitOfWorkGeneric<DailyRentAgreement> agreementUoW;

		public IUnitOfWorkGeneric<DailyRentAgreement> AgreementUoW {
			get { return agreementUoW; }
			set {
				if (agreementUoW == value)
					return;
				agreementUoW = value;
				if (AgreementUoW.Root.Equipment == null)
					AgreementUoW.Root.Equipment = new List<PaidRentEquipment> ();
				dailyRentEquipments = AgreementUoW.Root.ObservableEquipment;
				dailyRentEquipments.ElementChanged += Equipment_ElementChanged; 
				treeRentPackages.ItemsDataSource = dailyRentEquipments;
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
			if (dailyRentEquipments != null)
				foreach (PaidRentEquipment eq in dailyRentEquipments) {
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
			if(PaidRentPackage == null) {
				OrmReference refWin = new OrmReference(typeof(PaidRentPackage));
				refWin.Mode = OrmReferenceMode.Select;
				refWin.ObjectSelected += (innerSender, ee) => {
					AddEquipmentManually((ee.Subject as PaidRentPackage));
				};
				MyTab.TabParent.AddSlaveTab(MyTab, refWin);
			} else {
				AddEquipmentManually(PaidRentPackage);
			}
		}

		void AddEquipmentManually(PaidRentPackage paidRentPackage)
		{
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new EquipmentsNonSerialForRentVM(AgreementUoW, paidRentPackage.EquipmentType));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Оборудование для аренды";
			SelectDialog.ObjectSelected += EquipmentSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void EquipmentSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var selectedNode = (NomenclatureForRentVMNode)e.VMNode;

			var rentPackage = RentPackageRepository.GetPaidRentPackage(AgreementUoW, selectedNode.Type);
			if(rentPackage == null) {
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет условий платной аренды.");
				return;
			}

			if(selectedNode.Available == 0) {
				if(!MessageDialogWorks.RunQuestionDialog("Не найдено свободного оборудования выбранного типа!\nДобавить принудительно?")) {
					return;
				}
			}

			PaidRentEquipment eq = new PaidRentEquipment ();
			eq.Nomenclature = selectedNode.Nomenclature;
			eq.Deposit = rentPackage.Deposit;
			eq.PaidRentPackage = rentPackage;
			eq.Count = 1;
			eq.Price = rentPackage.PriceDaily;
			dailyRentEquipments.Add (eq);
			UpdateTotalLabels ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var selectedObjects = treeRentPackages.GetSelectedObjects();
			if(selectedObjects.Length == 1) {
				dailyRentEquipments.Remove(selectedObjects[0] as PaidRentEquipment);
				UpdateTotalLabels();
			}		}

		protected void OnButtonAddByTypeClicked(object sender, EventArgs e)
		{
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new ViewModel.EquipmentTypesForRentVM(MyOrmDialog.UoW));
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.TabName = "Выберите тип оборудования";
			SelectDialog.ObjectSelected += EquipmentByTypeSelected;

			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void EquipmentByTypeSelected (object sender, ReferenceRepresentationSelectedEventArgs args)
		{
			var equipmentType = AgreementUoW.GetById<EquipmentType>(args.ObjectId);

			var rentPackage = Repository.RentPackageRepository.GetPaidRentPackage(AgreementUoW, equipmentType);
			if(rentPackage == null) {
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет условий платной аренды.");
				return;
			}

			AddEquipmentByRentPackage(rentPackage);
		}

		private void AddEquipmentByRentPackage(PaidRentPackage paidRentPackage)
		{
			var anyNomenclature = EquipmentRepository.GetFirstAnyNomenclatureForRent(AgreementUoW, paidRentPackage.EquipmentType);
			if(anyNomenclature == null) {
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет оборудования в справочнике номенклатур.");
				return;
			}

			var excludeNomenclatures = dailyRentEquipments.Select(e => e.Nomenclature.Id).ToArray();

			var selectedNomenclature = EquipmentRepository.GetAvailableNonSerialEquipmentForRent(AgreementUoW, paidRentPackage.EquipmentType, excludeNomenclatures);
			if(selectedNomenclature == null) {
				if(!MessageDialogWorks.RunQuestionDialog("Не найдено свободного оборудования выбранного типа!\nДобавить принудительно?")) {
					return;
				} else {
					selectedNomenclature = anyNomenclature;
				}
			}

			PaidRentEquipment eq = new PaidRentEquipment();
			eq.Nomenclature = selectedNomenclature;
			eq.Deposit = paidRentPackage.Deposit;
			eq.PaidRentPackage = paidRentPackage;
			eq.Count = 1;
			eq.Price = paidRentPackage.PriceDaily;
			dailyRentEquipments.Add(eq);
			UpdateTotalLabels();
		}

		public void AddEquipment(PaidRentPackage paidRentPackage)
		{
			if(MessageDialogWorks.RunQuestionDialog("Подобрать оборудование автоматически по типу?")) {
				AddEquipmentByRentPackage(paidRentPackage);
			}
		}

	}
}

