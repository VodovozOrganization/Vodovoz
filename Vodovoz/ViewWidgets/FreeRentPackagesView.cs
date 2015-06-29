using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using NLog;
using Vodovoz.Repository;
using System.Linq;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FreeRentPackagesView : Gtk.Bin
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		private GenericObservableList<FreeRentEquipment> equipment;

		Decimal TotalDeposit = 0;

		int TotalWaterAmount = 0;

		private IUnitOfWorkGeneric<FreeRentAgreement> agreementUoW;

		public IUnitOfWorkGeneric<FreeRentAgreement> AgreementUoW {
			get { return agreementUoW; }
			set {
				if (agreementUoW == value)
					return;
				agreementUoW = value;
				if (AgreementUoW.Root.Equipment == null)
					AgreementUoW.Root.Equipment = new List<FreeRentEquipment> ();
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

		void UpdateTotalLabels ()
		{
			TotalDeposit = TotalWaterAmount = 0;
			if (equipment != null)
				foreach (FreeRentEquipment eq in equipment) {
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
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null) {
				logger.Warn ("Родительская вкладка не найдена.");
				return;
			}

			var availableTypes = FreeRentPackageRepository.GetPresentEquipmentTypes (AgreementUoW);

			//TODO FIXME Filter used equipment
			var Query = EquipmentRepository.GetEquipmentWithTypesQuery (availableTypes);
			OrmReference SelectDialog = new OrmReference (typeof(Equipment), 
				                            AgreementUoW.Session, 
				                            Query.GetExecutableQueryOver (AgreementUoW.Session).RootCriteria);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += EquipmentSelected;

			mytab.TabParent.AddSlaveTab (mytab, SelectDialog);
		}

		void EquipmentSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			FreeRentEquipment eq = new FreeRentEquipment ();
			eq.Equipment = (Equipment)e.Subject;
			var rentPackage = AgreementUoW.Session.CreateCriteria (typeof(FreeRentPackage))
				.List<FreeRentPackage> ()
				.First (p => p.EquipmentType == eq.Equipment.Nomenclature.Type);
			eq.Deposit = rentPackage.Deposit;
			eq.FreeRentPackage = rentPackage;
			eq.WaterAmount = rentPackage.MinWaterAmount;
			equipment.Add (eq);
			UpdateTotalLabels ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			equipment.Remove (treeRentPackages.GetSelectedObjects () [0] as FreeRentEquipment);
		}
	}
}

