using QS.DomainModel.UoW;
using QS.Navigation;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Factories;

namespace Vodovoz
{
	public partial class WarehouseDlg : QS.Dialog.Gtk.EntityDialogBase<Warehouse>
	{
		private readonly ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory = new SubdivisionJournalFactory();
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private Nomenclature selectedNomenclature;

		public WarehouseDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Warehouse>();
			ConfigureDialog();
		}

		public WarehouseDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Warehouse>(id);
			ConfigureDialog();
		}

		public WarehouseDlg(Warehouse sub) : this(sub.Id) { }

		private bool CanEdit => permissionResult.CanUpdate || Entity.Id == 0 && permissionResult.CanCreate;

		private void ConfigureDialog()
		{
			buttonSave.Sensitive = CanEdit;
			btnCancel.Clicked += (sender, args) => OnCloseTab(false, CloseSource.Cancel);
			
			yentryName.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.Name, (widget) => widget.Text)
				.InitializeFromSource();
			yentryName.IsEditable = CanEdit;
			ycheckOnlineStore.Binding
				.AddBinding(Entity, e => e.PublishOnlineStore, w => w.Active)
				.InitializeFromSource();
			ycheckOnlineStore.Sensitive = CanEdit;
			ycheckbuttonCanReceiveBottles.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.CanReceiveBottles, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonCanReceiveBottles.Sensitive = CanEdit;
			ycheckbuttonCanReceiveEquipment.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.CanReceiveEquipment, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonCanReceiveEquipment.Sensitive = CanEdit;
			ycheckbuttonArchive.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.IsArchive, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonArchive.Sensitive =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_archive_warehouse") && CanEdit;

			comboTypeOfUse.ItemsEnum = typeof(WarehouseUsing);
			comboTypeOfUse.Binding
				.AddBinding(Entity, e => e.TypeOfUse, w => w.SelectedItem)
				.InitializeFromSource();
			comboTypeOfUse.Sensitive = CanEdit;
			
			ySpecCmbOwner.SetRenderTextFunc<Subdivision>(s => s.Name);
			ySpecCmbOwner.ItemsList = _subdivisionRepository.GetAllDepartmentsOrderedByName(UoW);
			ySpecCmbOwner.Binding
				.AddBinding(Entity, s => s.OwningSubdivision, w => w.SelectedItem)
				.InitializeFromSource();
			ySpecCmbOwner.Sensitive = CanEdit;

			entityEntryMovementNotificationsRecipient
				.SetEntityAutocompleteSelectorFactory(_subdivisionJournalFactory.CreateSubdivisionAutocompleteSelectorFactory());
			entityEntryMovementNotificationsRecipient.Binding
				.AddBinding(Entity, e => e.MovementDocumentsNotificationsSubdivisionRecipient, w => w.Subject)
				.InitializeFromSource();
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QS.Validation.QSValidator<Warehouse>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			_logger.Info("Сохраняем склад...");
			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}
