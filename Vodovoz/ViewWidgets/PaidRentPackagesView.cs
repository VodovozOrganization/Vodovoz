using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Repositories;
using Vodovoz.Representations;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PaidRentPackagesView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		GenericObservableList<PaidRentEquipment> paidRentEquipments;

		private IUnitOfWorkGeneric<NonfreeRentAgreement> agreementUoW;

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonAdd.Sensitive = buttonDelete.Sensitive = treeRentPackages.Sensitive = value;
			} 
		}

		public PaidRentPackage PaidRentPackage { get; set; }

		public IUnitOfWorkGeneric<NonfreeRentAgreement> AgreementUoW {
			get { return agreementUoW; }
			set {
				if (agreementUoW == value)
					return;
				agreementUoW = value;
				if (AgreementUoW.Root.PaidRentEquipments == null)
					AgreementUoW.Root.PaidRentEquipments = new List<PaidRentEquipment> ();
				paidRentEquipments = AgreementUoW.Root.ObservableEquipment;
				paidRentEquipments.ElementChanged += Equipment_ElementChanged; 
				treeRentPackages.ItemsDataSource = paidRentEquipments;
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
			if (paidRentEquipments != null)
				foreach (PaidRentEquipment eq in paidRentEquipments) {
					TotalPrice += eq.Price;
					TotalDeposit += eq.Deposit;
				}
			if (AgreementUoW != null) {
				labelTotalPrice.Text = CurrencyWorks.GetShortCurrencyString (TotalPrice);
				labelTotalDeposit.Text = CurrencyWorks.GetShortCurrencyString (TotalDeposit);
			}
		}

		public PaidRentPackagesView ()
		{
			this.Build ();

			treeRentPackages.ColumnsConfig = ColumnsConfigFactory.Create<PaidRentEquipment>()
				.AddColumn("Пакет").AddTextRenderer(x => x.PackageName)
				.AddColumn("Оборудование").AddTextRenderer(x => x.EquipmentName)
				.AddColumn("Количество").AddNumericRenderer(x => x.Count)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
				.AddColumn("Цена аренды (в месяц)").AddTextRenderer(x => x.PriceString)
				.Finish();
			treeRentPackages.Selection.Changed += OnSelectionChanged;
			UpdateTotalLabels ();
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
			}else {
				AddEquipmentManually(PaidRentPackage);
			}
		}

		void AddEquipmentManually(PaidRentPackage paidRentPackage)
		{
			PermissionControlledRepresentationJournal SelectDialog = new PermissionControlledRepresentationJournal(new EquipmentsNonSerialForRentVM(AgreementUoW, paidRentPackage.EquipmentType));
			SelectDialog.Mode = JournalSelectMode.Single;
			SelectDialog.CustomTabName("Оборудование для аренды");
			SelectDialog.ObjectSelected += EquipmentSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, SelectDialog);
		}

		void EquipmentSelected (object sender, JournalObjectSelectedEventArgs e)
		{
			var selectedNode = e.GetNodes<NomenclatureForRentVMNode>().FirstOrDefault();
			if(selectedNode == null) {
				return;
			}
			var rentPackage = RentPackageRepository.GetPaidRentPackage(AgreementUoW, selectedNode.Type);
			if (rentPackage == null)
			{
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет условий платной аренды.");
				return;
			}

			if(selectedNode.Available  == 0) {
				if(!MessageDialogWorks.RunQuestionDialog("Не найдено свободного оборудования выбранного типа!\nДобавить принудительно?")) {
					return;
				}
			}

			PaidRentEquipment eq = new PaidRentEquipment ();
			eq.Nomenclature = selectedNode.Nomenclature;
			eq.Deposit = rentPackage.Deposit;
			eq.PaidRentPackage = rentPackage;
			eq.Count = 1;
			eq.Price = rentPackage.PriceMonthly;
			paidRentEquipments.Add (eq);
			UpdateTotalLabels ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var selectedObjects = treeRentPackages.GetSelectedObjects();
			if(selectedObjects.Length == 1) {
				paidRentEquipments.Remove(selectedObjects[0] as PaidRentEquipment);
				UpdateTotalLabels();
			}
		}

		protected void OnButtonAddByTypeClicked(object sender, EventArgs e)
		{
			PermissionControlledRepresentationJournal SelectDialog = new PermissionControlledRepresentationJournal (new ViewModel.EquipmentTypesForRentVM (MyOrmDialog.UoW));
			SelectDialog.Mode = JournalSelectMode.Single;
			SelectDialog.CustomTabName("Выберите тип оборудования");
			SelectDialog.ObjectSelected += EquipmentByTypeSelected;

			MyTab.TabParent.AddSlaveTab (MyTab, SelectDialog);
		}

		void EquipmentByTypeSelected (object sender, JournalObjectSelectedEventArgs args)
		{
			var selectedId = args.GetSelectedIds().FirstOrDefault();
			if(selectedId == 0) {
				return;
			}
			var equipmentType = AgreementUoW.GetById<EquipmentType>(selectedId);

			PaidRentPackage rentPackage = RentPackageRepository.GetPaidRentPackage(AgreementUoW, equipmentType);
			if (rentPackage == null)
			{
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет условий платной аренды.");
				return;
			}

			AddEquipmentByRentPackage(rentPackage);
		}

		void AddEquipmentByRentPackage(PaidRentPackage paidRentPackage)
		{
			var anyNomenclature = EquipmentRepository.GetFirstAnyNomenclatureForRent(AgreementUoW, paidRentPackage.EquipmentType);
			if(anyNomenclature == null) {
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет оборудования в справочнике номенклатур.");
				return;
			}

			var excludeNomenclatures = paidRentEquipments.Select(e => e.Nomenclature.Id).ToArray();

			var selectedNomenclature = EquipmentRepositoryForViews.GetAvailableNonSerialEquipmentForRent(AgreementUoW, paidRentPackage.EquipmentType, excludeNomenclatures);
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
			eq.Price = paidRentPackage.PriceMonthly;
			paidRentEquipments.Add(eq);
			UpdateTotalLabels();
		}

		public void AddEquipment(PaidRentPackage paidRentPackage)
		{
			if(MessageDialogWorks.RunQuestionDialog("Подобрать оборудование автоматически по типу?")){
				AddEquipmentByRentPackage(paidRentPackage);
			}
		}
	}
}