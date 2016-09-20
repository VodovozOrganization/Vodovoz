using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.DocTemplates;

namespace Vodovoz
{
	public partial class WaterAgreementDlg : OrmGtkDialogBase<WaterSalesAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;
		public event EventHandler<ContractSavedEventArgs> ContractSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = 
					referenceDeliveryPoint.Sensitive = dateIssue.Sensitive = dateStart.Sensitive = 
						checkIsFixedPrice.Sensitive = spinFixedPrice.Sensitive = value;
			} 
		}

		public WaterAgreementDlg (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = WaterSalesAgreement.Create (contract);
			ConfigureDlg ();
		}

		public WaterAgreementDlg (CounterpartyContract contract, DateTime? date) : this (contract)
		{
			if(date.HasValue)
				UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = date.Value;
		}

		public WaterAgreementDlg (WaterSalesAgreement sub) : this (sub.Id)
		{
		}

		public WaterAgreementDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<WaterSalesAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM (UoW, Entity.Contract.Counterparty);
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();
			spinFixedPrice.Sensitive = currencylabel1.Sensitive = UoWGeneric.Root.IsFixedPrice;

			if (Entity.AgreementTemplate == null && Entity.Contract != null)
				Entity.UpdateContractTemplate(UoW);

			if (Entity.AgreementTemplate != null)
				(Entity.AgreementTemplate.DocParser as WaterAgreementParser).RootObject = Entity;
			templatewidget1.Binding.AddBinding(Entity, e => e.AgreementTemplate, w => w.Template).InitializeFromSource();
			templatewidget1.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidator<WaterSalesAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			if (AgreementSaved != null)
				AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			return true;
		}

		protected void OnCheckIsFixedPriceToggled (object sender, EventArgs e)
		{
			spinFixedPrice.Sensitive = currencylabel1.Sensitive = UoWGeneric.Root.IsFixedPrice;
		}
	}
}

