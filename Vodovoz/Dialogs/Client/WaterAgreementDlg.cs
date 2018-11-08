using System;
using System.Linq;
using Gamma.GtkWidgets;
using NLog;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using QS.HistoryLog.Repositories;
using QS.Dialog;
using QSOrmProject;
using QSValidation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Repositories.Client;

namespace Vodovoz
{
	public partial class WaterAgreementDlg : QS.Dialog.Gtk.EntityDialogBase<WaterSalesAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;
		public event EventHandler<ContractSavedEventArgs> ContractSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger();

		bool isEditable = true;

		public bool IsEditable {
			get { return isEditable; }
			set {
				isEditable = value;
				buttonSave.Sensitive =
					referenceDeliveryPoint.Sensitive = dateIssue.Sensitive = dateStart.Sensitive = value;
			}
		}

		public WaterAgreementDlg(CounterpartyContract contract)
		{
			this.Build();
			UoWGeneric = WaterSalesAgreement.Create(contract);
			ConfigureDlg();
		}

		public WaterAgreementDlg(CounterpartyContract contract, DeliveryPoint deliveryPoint) : this(contract)
		{
			UoWGeneric.Root.DeliveryPoint = UoW.GetById<DeliveryPoint>(deliveryPoint.Id);
			ConfigureDlg();
		}

		public WaterAgreementDlg(CounterpartyContract contract, DeliveryPoint deliveryPoint, DateTime? date) : this(contract, deliveryPoint)
		{
			if(date.HasValue)
				UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = date.Value;
			ConfigureDlg();
			Entity.FillFixedPricesFromDeliveryPoint(UoW);
		}

		public WaterAgreementDlg(WaterSalesAgreement sub) : this(sub.Id)
		{
		}

		public WaterAgreementDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<WaterSalesAgreement>(id);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, Entity.Contract.Counterparty);
			referenceDeliveryPoint.Binding.AddBinding(Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource();
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();

			dateIssue.Binding.AddBinding(Entity, e => e.IssueDate, w => w.Date).InitializeFromSource();
			dateStart.Binding.AddBinding(Entity, e => e.StartDate, w => w.Date).InitializeFromSource();

			if(Entity.DocumentTemplate == null && Entity.Contract != null)
				Entity.UpdateContractTemplate(UoW);

			if(Entity.DocumentTemplate != null) {
				(Entity.DocumentTemplate.DocParser as WaterAgreementParser).RootObject = Entity;
				(Entity.DocumentTemplate.DocParser as WaterAgreementParser)
					.AddPricesTable(WaterPricesRepository.GetCompleteWaterPriceTable(UoW));
			}

			templatewidget1.CanRevertCommon = QSProjectsLib.QSMain.User.Permissions["can_set_common_additionalagreement"];
			templatewidget1.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget1.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();

			ytreeviewFixedPrices.ColumnsConfig = ColumnsConfigFactory.Create<WaterSalesAgreementFixedPrice>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Фиксированная цена").AddNumericRenderer(x => x.Price).Editing().Digits(2)
					.Adjustment(new Gtk.Adjustment(0, 0, 1e6, 1, 10, 10))
				.Finish();

			ytreeviewFixedPricesChanges.ColumnsConfig = ColumnsConfigFactory.Create<FieldChange>()
				.AddColumn("Время изменения").AddTextRenderer(x => x.Entity.ChangeTimeText)
				.AddColumn("Пользователь").AddTextRenderer(x => x.Entity.ChangeSet.UserName)
				.AddColumn("Старое значение").AddTextRenderer(x => x.OldPangoText, useMarkup: true)
				.AddColumn("Новое значение").AddTextRenderer(x => x.NewPangoText, useMarkup: true)
				.Finish();

			ytreeviewFixedPrices.Selection.Changed += (sender, e) => {
				var fixedPrice = (ytreeviewFixedPrices.GetSelectedObject() as WaterSalesAgreementFixedPrice);
				if(fixedPrice == null) {
					ytreeviewFixedPricesChanges.ItemsDataSource = null;
					return;
				}

				var fixedPricesChanges = HistoryChangesRepository
					.GetFieldChanges<WaterSalesAgreementFixedPrice>(UoW, new[]{fixedPrice.Id}, x => x.Price)
					.OrderBy(x => x.Entity.ChangeTime).ToList();

				ytreeviewFixedPricesChanges.ItemsDataSource = fixedPricesChanges;
			};

			ytreeviewFixedPrices.ItemsDataSource = Entity.ObservableFixedPrices;
			ytreeviewFixedPrices.Selection.Changed += YtreeviewFixedPrices_Selection_Changed;
		}

		void YtreeviewFixedPrices_Selection_Changed(object sender, EventArgs e)
		{
			buttonDel.Sensitive = ytreeviewFixedPrices.Selection.CountSelectedRows() > 0;
		}

		public override bool Save()
		{
			if(!Entity.HasFixedPrice && Entity.FixedPrices.Count > 0) {
				foreach(var v in Entity.FixedPrices.ToList()) {
					Entity.FixedPrices.Remove(v);
					UoW.Delete(v);
				}
			}

			var valid = new QSValidator<WaterSalesAgreement>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем доп. соглашение...");
			UoWGeneric.Save();
			logger.Info("Ok");
			if(AgreementSaved != null)
				AgreementSaved(this, new AgreementSavedEventArgs(UoWGeneric.Root));
			return true;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var addNewNomenclature = new OrmReference(Repository.NomenclatureRepository.NomenclatureWaterOnlyQuery());
			addNewNomenclature.Mode = OrmReferenceMode.Select;
			addNewNomenclature.TabName = "Выберите номенклатуру";
			addNewNomenclature.ObjectSelected += AddNewNomenclature_ObjectSelected; ;
			TabParent.AddSlaveTab(this, addNewNomenclature);
		}

		void AddNewNomenclature_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclatureToAdd = e.Subject as Nomenclature;
			decimal price = 0;
			if(nomenclatureToAdd.DependsOnNomenclature != null) {
				var fixPrice = Entity.FixedPrices
									 .Where(p => p.Nomenclature.Id == nomenclatureToAdd.DependsOnNomenclature.Id)
									 .FirstOrDefault();
				price = fixPrice == null ? 0 : fixPrice.Price;
			}
			Entity.AddFixedPrice(e.Subject as Nomenclature, price);
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			var item = ytreeviewFixedPrices.GetSelectedObject<WaterSalesAgreementFixedPrice>();
			Entity.ObservableFixedPrices.Remove(item);
			UoW.Delete(item);
		}

		protected void OnReferenceDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			Entity.FillFixedPricesFromDeliveryPoint(UoW);
		}
	}
}

