using System;
using Autofac;
using Gamma.Widgets;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(false)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class StockBalanceFilter : RepresentationFilterBase<StockBalanceFilter>
	{
		protected override void ConfigureWithUow()
		{
			speccomboStock.SetRenderTextFunc<Warehouse>(x => x.Name);
			speccomboStock.ItemsList = ScopeProvider.Scope.Resolve<IWarehouseRepository>().GetActiveWarehouse(UoW);
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
				speccomboStock.SelectedItem = UoW.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
		}

		bool showArchive;
		public bool ShowArchive {
			get => showArchive;
			set {
				showArchive = checkShowArchive.Active = value;
			}
		}

		public StockBalanceFilter(IUnitOfWork uow)
		{
			this.Build();
			UoW = uow;
		}

		public StockBalanceFilter() : this(ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
		}

		protected void OnEnumcomboTypeEnumItemSelected(object sender, QS.Widgets.EnumItemClickedEventArgs e)
		{
			OnRefiltered();
		}

		public Warehouse RestrictWarehouse {
			get {
				if(speccomboStock.SelectedItem is Warehouse)
					return speccomboStock.SelectedItem as Warehouse;
				else
					return null;
			}
			set {
				speccomboStock.SelectedItem = value;
				speccomboStock.Sensitive = false;
			}
		}

		protected void OnSpeccomboStockItemSelected(object sender, ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}

		protected void OnCheckShowArchiveToggled(object sender, EventArgs e)
		{
			ShowArchive = checkShowArchive.Active;
			OnRefiltered();
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

