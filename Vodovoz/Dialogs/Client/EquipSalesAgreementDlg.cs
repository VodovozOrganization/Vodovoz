using System;
using System.Linq;
using Gamma.GtkWidgets;
using NLog;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QSOrmProject;
using QSValidation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModelBased;

namespace Vodovoz.Dialogs.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EquipSalesAgreementDlg : QS.Dialog.Gtk.EntityDialogBase<SalesEquipmentAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger();

		public bool IsEditable { get; set; } = true;

		public EquipSalesAgreementDlg(CounterpartyContract contract)
		{
			this.Build();
			UoWGeneric = SalesEquipmentAgreement.Create(contract);
			ConfigureDlg();
		}

		public EquipSalesAgreementDlg(CounterpartyContract contract, DeliveryPoint point, DateTime? IssueDate, Nomenclature nomenclature = null)
		{
			this.Build();
			UoWGeneric = SalesEquipmentAgreement.Create(contract);
			UoWGeneric.Root.DeliveryPoint = point;
			if(IssueDate.HasValue)
				UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = IssueDate.Value;
			if(nomenclature != null) {
				Entity.AddEquipment(nomenclature);
			}
			ConfigureDlg();
		}

		public EquipSalesAgreementDlg(SalesEquipmentAgreement sub) : this(sub.Id)
		{
		}

		public EquipSalesAgreementDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<SalesEquipmentAgreement>(id);
			ConfigureDlg();
		}

		public EquipSalesAgreementDlg(IUnitOfWork baseUoW, IEntityOpenOption option)
		{
			this.Build();
			if(!option.NeedCreateNew) {
				UoWGeneric = option.UseChildUoW
					? UnitOfWorkFactory.CreateForChildRoot(baseUoW.GetById<SalesEquipmentAgreement>(option.EntityId), baseUoW)
					: UnitOfWorkFactory.CreateForRoot<SalesEquipmentAgreement>(option.EntityId);
			} else {
				UoWGeneric = option.UseChildUoW
					? UnitOfWorkFactory.CreateWithNewChildRoot<SalesEquipmentAgreement>(baseUoW)
					: UnitOfWorkFactory.CreateWithNewRoot<SalesEquipmentAgreement>();
			}

			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytreeviewEquipments.ColumnsConfig = ColumnsConfigFactory.Create<SalesEquipment>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Цена").AddNumericRenderer(x => x.Price).Editing().Digits(2)
					.Adjustment(new Gtk.Adjustment(0, 0, 1000000, 1, 10, 10))
				.AddColumn("Количество").AddNumericRenderer(x => x.Count).Editing()
					.Adjustment(new Gtk.Adjustment(0, 0, 1000000, 1, 10, 10))
				.Finish();
			ytreeviewEquipments.ItemsDataSource = Entity.ObservableSalesEqipments;
			dateIssue.Sensitive = dateStart.Sensitive = false;
			dateIssue.Binding.AddBinding(Entity, e => e.IssueDate, w => w.Date).InitializeFromSource();
			dateStart.Binding.AddBinding(Entity, e => e.StartDate, w => w.Date).InitializeFromSource();

			referenceDeliveryPoint.Sensitive = false;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM(UoW, Entity.Contract.Counterparty);
			referenceDeliveryPoint.Binding.AddBinding(Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource();
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();

			if(Entity.DocumentTemplate == null && Entity.Contract != null)
				Entity.UpdateContractTemplate(UoW);

			if(Entity.DocumentTemplate != null) {
				(Entity.DocumentTemplate.DocParser as EquipmentAgreementParser).RootObject = Entity;
			}
			templatewidget1.BeforeOpen += (sender, e) => {
				if(Entity.DocumentTemplate != null) {
					(Entity.DocumentTemplate.DocParser as EquipmentAgreementParser).AddPricesTable(Entity.SalesEqipments.ToList());
				}
			};

			templatewidget1.CanRevertCommon = UserPermissionRepository.CurrentUserPresetPermissions["can_set_common_additionalagreement"];
			templatewidget1.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget1.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
		}

		public override bool Save()
		{
			var valid = new QSValidator<SalesEquipmentAgreement>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем доп. соглашение...");
			UoWGeneric.Save();
			logger.Info("Ok");
			AgreementSaved?.Invoke(this, new AgreementSavedEventArgs(UoWGeneric.Root));
			return true;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var addNewNomenclature = new OrmReference(Repository.NomenclatureRepository.NomenclatureEquipOnlyQuery()) {
				Mode = OrmReferenceMode.Select,
				TabName = "Выберите номенклатуру"
			};
			addNewNomenclature.ObjectSelected += AddNewNomenclature_ObjectSelected; ;
			TabParent.AddSlaveTab(this, addNewNomenclature);
		}

		void AddNewNomenclature_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Entity.AddEquipment(e.Subject as Nomenclature);
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			var item = ytreeviewEquipments.GetSelectedObject<SalesEquipment>();
			Entity.ObservableSalesEqipments.Remove(item);
			UoW.Delete(item);
		}
	}
}
