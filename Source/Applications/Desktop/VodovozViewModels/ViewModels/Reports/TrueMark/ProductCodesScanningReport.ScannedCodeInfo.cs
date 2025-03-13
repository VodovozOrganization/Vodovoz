using System.Collections.Generic;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
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
			public int MarkedProdictsCount { get; set; }
			public IList<RouteListItemTrueMarkProductCode> ScannedCodes { get; set; }


			//public int ScannedCodesCount { get; set; }
			//public int UnscannedCodesCount { get; set; }
			//public int SingleDuplicatedCodesCount { get; set; }
			//public int MultiplyDuplicatedCodesCount { get; set; }
			//public int DefectiveCodesCount { get; set; }
			//public int InvalidCodesCount { get; set; }


			//public int? SourceCodeId { get; set; }
			//public int? DuplicatedCodeId { get; set; }
			//public bool IsProductCodeSingleDuplicated { get; set; }
			//public bool IsProductCodeMultiplyDuplicated { get; set; }
			//public bool IsDuplicateSourceCode { get; set; }
			//public bool IsUnscannedSourceCode { get; set; }
			//public bool IsDefectiveSourceCode { get; set; }
			//public bool IsInvalidSourceCode { get; set; }
		}
	}
}
