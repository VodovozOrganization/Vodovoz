using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;

namespace Vodovoz
{
	public partial class WarehouseDlg : QS.Dialog.Gtk.EntityDialogBase<Warehouse>
	{
		private readonly ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private Nomenclature selectedNomenclature;

		public WarehouseDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Warehouse>();
			ConfigureDialog();
		}

		public WarehouseDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Warehouse>(id);
			ConfigureDialog();
		}

		public WarehouseDlg(Warehouse sub) : this(sub.Id) { }

		protected void ConfigureDialog()
		{
			yentryName.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.Name, (widget) => widget.Text)
				.InitializeFromSource();
			ycheckOnlineStore.Binding.AddBinding(Entity, e => e.PublishOnlineStore, w => w.Active).InitializeFromSource();
			ycheckbuttonCanReceiveBottles.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.CanReceiveBottles, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonCanReceiveEquipment.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.CanReceiveEquipment, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonArchive.Binding
				.AddBinding(UoWGeneric.Root, (warehouse) => warehouse.IsArchive, (widget) => widget.Active)
				.InitializeFromSource();
			ycheckbuttonArchive.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_archive_warehouse");

			comboTypeOfUse.ItemsEnum = typeof(WarehouseUsing);
			comboTypeOfUse.Binding.AddBinding(Entity, e => e.TypeOfUse, w => w.SelectedItem).InitializeFromSource();

			ySpecCmbOwner.SetRenderTextFunc<Subdivision>(s => s.Name);
			ySpecCmbOwner.ItemsList = _subdivisionRepository.GetAllDepartmentsOrderedByName(UoW);
			ySpecCmbOwner.Binding.AddBinding(Entity, s => s.OwningSubdivision, w => w.SelectedItem).InitializeFromSource();
	
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