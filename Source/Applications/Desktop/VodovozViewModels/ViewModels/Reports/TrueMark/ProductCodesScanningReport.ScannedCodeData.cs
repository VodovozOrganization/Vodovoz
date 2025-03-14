using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.TrueMark
{
	public partial class ProductCodesScanningReport
	{
		public class ScannedCodeData
		{
			public ProductCodeProblem Problem { get; set; }
			public int DuplicatesCount { get; set; }
			public bool IsInvalid { get; set; }
		}
	}
}
