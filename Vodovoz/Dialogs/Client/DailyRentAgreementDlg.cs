using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Client;
using Vodovoz.DocTemplates;
using Vodovoz.Domain;
using System.Linq;

namespace Vodovoz
{
	public partial class DailyRentAgreementDlg : OrmGtkDialogBase<DailyRentAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = 
					dateEnd.Sensitive = dateStart.Sensitive = 
						dailyrentpackagesview1.IsEditable = value;
			} 
		}

		private PaidRentPackage PaidRentPackage { get; set; }

		public DailyRentAgreementDlg (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = DailyRentAgreement.Create (contract);
			ConfigureDlg ();
		}

		public DailyRentAgreementDlg (CounterpartyContract contract, DeliveryPoint point, DateTime? IssueDate)// : this (contract)
		{
			this.Build();
			UoWGeneric = DailyRentAgreement.Create(contract);
			UoWGeneric.Root.DeliveryPoint = point;
			if(IssueDate.HasValue)
				UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = IssueDate.Value;
			ConfigureDlg();
		}

		public DailyRentAgreementDlg(CounterpartyContract contract, DeliveryPoint point, DateTime? IssueDate, PaidRentPackage paidRentPackage)// : this (contract)
		{
			this.Build();
			UoWGeneric = DailyRentAgreement.Create(contract);
			UoWGeneric.Root.DeliveryPoint = point;
			if(IssueDate.HasValue)
				UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = IssueDate.Value;
			ConfigureDlg();
			PaidRentPackage = paidRentPackage;
			dailyrentpackagesview1.PaidRentPackage = PaidRentPackage;
			dailyrentpackagesview1.AddEquipment(PaidRentPackage);
		}

		public DailyRentAgreementDlg (DailyRentAgreement sub) : this (sub.Id)
		{
		}

		public DailyRentAgreementDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<DailyRentAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			spinRentDays.Sensitive = false;
			dateIssue.Sensitive = dateStart.Sensitive = false;
			dateIssue.Binding.AddBinding (Entity, e => e.IssueDate, w => w.Date).InitializeFromSource ();
			dateStart.Binding.AddBinding (Entity, e => e.StartDate, w => w.Date).InitializeFromSource ();
			dateEnd.Binding.AddBinding (Entity, e => e.EndDate, w => w.Date).InitializeFromSource ();

			referenceDeliveryPoint.Sensitive = false;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM (UoW, Entity.Contract.Counterparty);
			referenceDeliveryPoint.Binding.AddBinding (Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource ();
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();

			spinRentDays.Binding.AddBinding (Entity, e => e.RentDays, w => w.ValueAsInt).InitializeFromSource ();

			dailyrentpackagesview1.IsEditable = true;
			dailyrentpackagesview1.AgreementUoW = UoWGeneric;

			dateEnd.Date = UoWGeneric.Root.StartDate.AddDays (UoWGeneric.Root.RentDays);

			if (Entity.DocumentTemplate == null && Entity.Contract != null)
				Entity.UpdateContractTemplate(UoW);

			if(Entity.DocumentTemplate != null) {
				(Entity.DocumentTemplate.DocParser as DailyRentAgreementParser).RootObject = Entity;
			}

			templatewidget3.BeforeOpen += (sender, e) => {
				if(Entity.DocumentTemplate != null) {
					(Entity.DocumentTemplate.DocParser as DailyRentAgreementParser).AddTableEquipmentTypes(Entity.Equipment.ToList());
					(Entity.DocumentTemplate.DocParser as DailyRentAgreementParser).AddTableNomenclatures(Entity.Equipment.ToList());
				}
			};
			templatewidget3.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget3.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
			templatewidget3.CanRevertCommon = QSProjectsLib.QSMain.User.Permissions["can_set_common_additionalagreement"];
		}

		public override bool Save ()
		{
			var valid = new QSValidator<DailyRentAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			if (AgreementSaved != null)
				AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			return true;
		}

		protected void OnSpinRentDaysValueChanged (object sender, EventArgs e)
		{
			dailyrentpackagesview1.UpdateTotalLabels ();
		}

		protected void OnDateStartDateChanged (object sender, EventArgs e)
		{
			RecalcRentPeriod ();
		}

		protected void OnDateEndDateChanged (object sender, EventArgs e)
		{
			RecalcRentPeriod ();
		}

		protected void RecalcRentPeriod ()
		{
			spinRentDays.Value = (dateEnd.Date.Date - dateStart.Date.Date).Days;
			dailyrentpackagesview1.UpdateTotalLabels ();
		}
	}
}

