using System;

namespace DatabaseServiceWorker.PowerBiWorker.Dto
{
	internal sealed class LateDto
	{
		internal DateTime Date { get; set; }
		internal decimal LessThan5Minutes { get; set; }
		internal decimal LessThan30Minutes { get; set; }
		internal decimal MoreThan30Minutes { get; set; }
	}
}
