using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.DocTemplates;
using Gamma.GtkWidgets;
using Vodovoz.Domain.Goods;
using System.Linq;

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
						checkIsFixedPrice.Sensitive = value; //spinFixedPrice.Sensitive = 
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

			if (Entity.AgreementTemplate == null && Entity.Contract != null)
				Entity.UpdateContractTemplate(UoW);

			if (Entity.AgreementTemplate != null)
				(Entity.AgreementTemplate.DocParser as WaterAgreementParser).RootObject = Entity;
			templatewidget1.Binding.AddBinding(Entity, e => e.AgreementTemplate, w => w.Template).InitializeFromSource();
			templatewidget1.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();

			ytreeviewFixedPrices.ColumnsConfig = ColumnsConfigFactory.Create<WaterSalesAgreementFixedPrice>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Фиксированная цена").AddNumericRenderer(x => x.Price).Editing().Digits(2)
					.Adjustment(new Gtk.Adjustment(0, 0, 1e6, 1, 10, 10))
				.Finish();

			ytreeviewFixedPrices.ItemsDataSource = Entity.ObservablFixedPrices;
			ytreeviewFixedPrices.Selection.Changed += YtreeviewFixedPrices_Selection_Changed;
		}

		void YtreeviewFixedPrices_Selection_Changed (object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeviewFixedPrices.Selection.CountSelectedRows() > 0;
		}

		public override bool Save ()
		{
			if (!Entity.IsFixedPrice && Entity.FixedPrices.Count > 0)
			{
				foreach (var v in Entity.FixedPrices.ToList())
				{
					Entity.FixedPrices.Remove(v);
					UoW.Delete(v);
				}
			}

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
			GtkScrolledWindowFixedPrice.Visible = buttonAdd.Visible = buttonDel.Visible = checkIsFixedPrice.Active;

		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			var addNewNomenclature = new OrmReference(Repository.NomenclatureRepository.NomenclatureWaterOnlyQuery());
			addNewNomenclature.Mode = OrmReferenceMode.Select;
			addNewNomenclature.TabName = "Выберите номенклатуру";
			addNewNomenclature.ObjectSelected += AddNewNomenclature_ObjectSelected;;
			TabParent.AddSlaveTab(this, addNewNomenclature);

		}

		void AddNewNomenclature_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Entity.AddFixedPrice(e.Subject as Nomenclature, 0);
		}

		protected void OnButtonDelClicked (object sender, EventArgs e)
		{
			var item = ytreeviewFixedPrices.GetSelectedObject<WaterSalesAgreementFixedPrice>();
			Entity.ObservablFixedPrices.Remove(item);
			UoW.Delete(item);
		}
	}
}

