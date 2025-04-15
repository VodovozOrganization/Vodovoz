namespace Vodovoz.ViewModels.ViewModels.Documents.SelfDeliveryCodesScan
{
	public partial class CodesScanViewModel
	{
		public class CodeToCheck
		{
			public string RawCode { get; set; }
			public bool NeedRecheck { get; set; }
			public string Error { get; set; }
		}
	}
}
