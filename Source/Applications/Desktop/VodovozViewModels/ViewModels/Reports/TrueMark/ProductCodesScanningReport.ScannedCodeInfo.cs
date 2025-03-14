using System.Collections.Generic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.ViewModels.Reports.TrueMark
{
	public partial class ProductCodesScanningReport
	{
		public class ScannedCodeInfo
		{
			public int DriverId { get; set; }
			public string DriverFIO { get; set; }
			public CarOwnType? CarOwnType { get; set; }
			public string DriverSubdivisionGeoGroup { get; set; }
			public int OrderId { get; set; }
			public int MarkedProdictsInOrderCount { get; set; }
			public ScannedCodeData ScannedCodeData { get; set; }
		}
	}
}
