using System;

namespace DatabaseServiceWorker.PowerBiWorker.Dto
{
	internal sealed class UndeliveredDto
	{
		internal DateTime Date { get; set; }
		internal string Responsible { get; set; }
		internal int Quantity { get; set; }
		internal int Quantity19 { get; set; }
	}
}
