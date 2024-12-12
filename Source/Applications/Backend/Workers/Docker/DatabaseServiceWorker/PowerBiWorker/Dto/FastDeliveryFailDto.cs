using System;

namespace DatabaseServiceWorker.PowerBiWorker.Dto
{
	internal sealed class FastDeliveryFailDto
	{
		internal DateTime Date { get; set; }
		internal int IsValidIsGoodsEnoughTotal { get; set; }
		internal int IsValidLastCoordinateTimeTotal { get; set; }
		internal int IsValidUnclosedFastDeliveriesTotal { get; set; }
		internal int IsValidDistanceByLineToClientTotal { get; set; }
	}
}
