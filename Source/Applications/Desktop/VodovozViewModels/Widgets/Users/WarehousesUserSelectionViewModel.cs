using QS.ViewModels;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModels.Widgets.Users
{
	public class WarehousesUserSelectionViewModel : WidgetViewModelBase
	{
		#region Свойства

		private GenericObservableList<Warehouse> _selectedWarehouses;
		public virtual GenericObservableList<Warehouse> SelectedWarehouses
		{
			get
			{
				if(_selectedWarehouses == null)
				{
					_selectedWarehouses = new GenericObservableList<Warehouse>();
				}

				return _selectedWarehouses;
			}
		}

		#endregion
	}
}
