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

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FreeRentPackagesView : WidgetOnDialogBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		private GenericObservableList<FreeRentEquipment> equipments;

		Decimal TotalDeposit = 0;

		int TotalWaterAmount = 0;

		private IUnitOfWorkGeneric<FreeRentAgreement> agreementUoW;

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonAdd.Sensitive = buttonDelete.Sensitive = treeRentPackages.Sensitive = value;
			} 
		}

		public IUnitOfWorkGeneric<FreeRentAgreement> AgreementUoW {
			get { return agreementUoW; }
			set {
				if (agreementUoW == value)
					return;
				agreementUoW = value;
				if (AgreementUoW.Root.Equipment == null)
					AgreementUoW.Root.Equipment = new List<FreeRentEquipment> ();
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

		void UpdateTotalLabels ()
		{
			TotalDeposit = TotalWaterAmount = 0;
			if (equipments != null)
				foreach (FreeRentEquipment eq in equipments) {
					TotalDeposit += eq.Deposit;
					TotalWaterAmount += eq.WaterAmount;
				}
			if (AgreementUoW != null) {
				labelTotalWaterAmount.Text = String.Format ("{0} " + RusNumber.Case (TotalWaterAmount, "бутыль", "бутыли", "бутылей"), TotalWaterAmount);
				labelTotalDeposit.Text = CurrencyWorks.GetShortCurrencyString (TotalDeposit);
			}
		}

		public FreeRentPackagesView ()
		{
			this.Build ();

			treeRentPackages.ColumnsConfig = ColumnsConfigFactory.Create<FreeRentEquipment>()
				.AddColumn("Пакет").AddTextRenderer(x => x.PackageName)
				.AddColumn("Оборудование").AddTextRenderer(x => x.EquipmentName)
				.AddColumn("Серийный номер").AddTextRenderer(x => x.EquipmentSerial)
				.AddColumn("Сумма залога").AddTextRenderer(x => x.DepositString)
				.AddColumn("Минимальное количество воды").AddTextRenderer(x => x.WaterAmountString)
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
			OrmReference SelectDialog = new OrmReference (AgreementUoW, EquipmentRepository.AvailableEquipmentQuery());
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanEdit;
			SelectDialog.ObjectSelected += EquipmentSelected;

			MyTab.TabParent.AddSlaveTab (MyTab, SelectDialog);
		}

		void EquipmentSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var selectedEquipment = (Equipment)e.Subject;

			var rentPackage = RentPackageRepository.GetFreeRentPackage(AgreementUoW, selectedEquipment.Nomenclature.Type);
			if (rentPackage == null)
			{
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет пакета бесплатной аренды.");
				return;
			}

			FreeRentEquipment eq = new FreeRentEquipment ();
			eq.Equipment = selectedEquipment;
			eq.Deposit = rentPackage.Deposit;
			eq.FreeRentPackage = rentPackage;
			eq.WaterAmount = rentPackage.MinWaterAmount;
			equipments.Add (eq);
			UpdateTotalLabels ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			equipments.Remove (treeRentPackages.GetSelectedObjects () [0] as FreeRentEquipment);
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

			var rentPackage = Repository.RentPackageRepository.GetFreeRentPackage(AgreementUoW, equipmentType);
			if (rentPackage == null)
			{
				MessageDialogWorks.RunErrorDialog("Для выбранного типа оборудования нет пакета бесплатной аренды.");
				return;
			}

			var exclude = equipments.Select(e => e.Equipment.Id).ToArray();

			var selectedEquipment = EquipmentRepository.GetAvailableEquipmentForRent(AgreementUoW, equipmentType, exclude);
			if(selectedEquipment == null)
			{
				MessageDialogWorks.RunErrorDialog("Не найдено свободного оборудования выбранного типа.");
				return;
			}

			FreeRentEquipment eq = new FreeRentEquipment ();
			eq.Equipment = selectedEquipment;
			eq.Deposit = rentPackage.Deposit;
			eq.FreeRentPackage = rentPackage;
			eq.WaterAmount = rentPackage.MinWaterAmount;
			equipments.Add (eq);
			UpdateTotalLabels ();
		}

	}
}

