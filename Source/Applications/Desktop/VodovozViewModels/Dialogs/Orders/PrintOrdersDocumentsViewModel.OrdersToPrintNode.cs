using QS.DomainModel.Entity;
using System;

namespace Vodovoz.ViewModels.Dialogs.Orders
{
	public partial class PrintOrdersDocumentsViewModel
	{
		public class OrdersToPrintNode : PropertyChangedBase
		{
			private bool _selected;

			public int Id { get; set; }
			public DateTime? DeliveryDate { get; set; }
			
			public bool Selected
			{
				get => _selected;
				set => SetField(ref _selected, value);
			}
		}
	}
}
