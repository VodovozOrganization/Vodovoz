using System;
using Autofac;
using Gamma.Widgets;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.EntityRepositories.Store;
using VodovozBusiness.Services.Users;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(false)]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class StockBalanceFilter : RepresentationFilterBase<StockBalanceFilter>
	{
		private ILifetimeScope _lifetimeScope = ScopeProvider.Scope.BeginLifetimeScope();
		
		protected override void ConfigureWithUow()
		{
			speccomboStock.SetRenderTextFunc<Warehouse>(x => x.Name);
			speccomboStock.ItemsList = _lifetimeScope.Resolve<IWarehouseRepository>().GetActiveWarehouse(UoW);
			var userSettingsManager = _lifetimeScope.Resolve<IUserSettingsManager>();
			
			if(userSettingsManager.Settings.DefaultWarehouse != null)
			{
				speccomboStock.SelectedItem = UoW.GetById<Warehouse>(userSettingsManager.Settings.DefaultWarehouse.Id);
			}
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

		protected override void OnDestroyed()
		{
			_lifetimeScope.Dispose();
			UoW?.Dispose();
			base.OnDestroyed();
		}
	}
}

