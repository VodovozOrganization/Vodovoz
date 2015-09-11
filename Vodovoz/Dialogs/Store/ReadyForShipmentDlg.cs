using System;
using QSTDI;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ReadyForShipmentDlg : TdiTabBase
	{
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		public ReadyForShipmentDlg (ShipmentDocumentType type, int id)
		{
			Build ();
			this.TabName = "Товар на погрузку";
			ycomboboxWarehouse.ItemsList = Repository.Store.WarehouseRepository.WarehouseForShipment (UoW, type, id);
		}


	}
}

