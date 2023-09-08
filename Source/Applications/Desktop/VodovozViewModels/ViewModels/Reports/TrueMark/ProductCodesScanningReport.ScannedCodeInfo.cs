using Vodovoz.Domain.TrueMark;

namespace Vodovoz.ViewModels.ViewModels.Reports.TrueMark
{
	public partial class ProductCodesScanningReport
	{
		public class ScannedCodeInfo
		{
			public int DriverId { get; set; }
			public string DriverFIO { get; set; }
			public int? SourceCodeId { get; set; }
			public int? DuplicatedCodeId { get; set; }
			public bool IsProductCodeSingleDuplicated { get; set; }
			public bool IsProductCodeMultiplyDuplicated { get; set; }
			public bool IsDuplicateSourceCode { get; set; }
			public bool IsUnscannedSourceCode { get; set; }
			public bool IsDefectiveSourceCode { get; set; }
			public bool IsInvalidSourceCode { get; set; }
		}
	}
}
