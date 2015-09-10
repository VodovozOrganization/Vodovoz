using System;
using QSTDI;
using QSOrmProject;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ReadyForShipmentDlg : TdiTabBase
	{
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		public ReadyForShipmentDlg ()
		{
			this.TabName = "Товар на погрузку";
			ycomboboxWarehouse.ItemsList = Repository.WarehouseRepository.GetActiveWarehouse (UoW);
		}


	}
}

