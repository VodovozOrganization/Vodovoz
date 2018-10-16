using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReadyForReceptionFilter : RepresentationFilterBase<ReadyForReceptionFilter>
	{
		protected override void ConfigureWithUow()
		{
			yspeccomboWarehouse.ItemsList = Repository.Store.WarehouseRepository.GetActiveWarehouse(UoW);
			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
				yspeccomboWarehouse.SelectedItem = CurrentUserSettings.Settings.DefaultWarehouse;
		}

		public ReadyForReceptionFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}
		public ReadyForReceptionFilter()
		{
			this.Build();
		}

		void UpdateCreteria()
		{
			OnRefiltered();
		}

		public Warehouse RestrictWarehouse {
			get { return yspeccomboWarehouse.SelectedItem as Warehouse; }
			set {
				yspeccomboWarehouse.SelectedItem = value;
				yspeccomboWarehouse.Sensitive = false;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public bool RestrictWithoutUnload {
			get { return checkWithoutUnload.Active; }
			set {
				checkWithoutUnload.Active = value;
				checkWithoutUnload.Sensitive = false;
			}
		}

		protected void OnYspeccomboWarehouseItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			UpdateCreteria();
		}

		protected void OnCheckWithoutUnloadToggled(object sender, EventArgs e)
		{
			UpdateCreteria();
		}
	}
}

