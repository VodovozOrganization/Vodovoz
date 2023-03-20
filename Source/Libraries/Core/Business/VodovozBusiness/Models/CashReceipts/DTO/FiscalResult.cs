using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class FiscalizationResult
	{
		public SendStatus SendStatus { get; set; }
		public long FiscalDocumentNumber { get; set; }
		public DateTime FiscalDocumentDate { get; set; }
		public FiscalDocumentStatus Status { get; set; }
		public DateTime StatusChangedTime { get; set; }
		public string FailDescription { get; set; }
	}
}
