using System;

namespace DatabaseServiceWorker.PowerBiWorker.Dto
{
	internal sealed class CoverageDto
	{
		public DateTime Date { get; set; }
		internal double Fill { get; set; }
		internal double AverageRadius { get; set; }
		internal double NumberOfCars { get; set; }
	}
}
