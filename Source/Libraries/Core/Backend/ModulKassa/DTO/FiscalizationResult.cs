using System;

namespace ModulKassa.DTO
{
	public class FiscalizationResult
	{
		public SendStatus SendStatus { get; set; }
		public FiscalDocumentInfo FiscalDocumentInfo { get; set; }
		public string ErrorMessage { get; set; }
	}
}
