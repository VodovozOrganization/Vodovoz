using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents.DriverTerminalTransfer
{
	public enum DriverTerminalTransferDocumentType
	{
		[Display(Name = "Документ переноса терминала другому водителю")]
		AnotherDriver,
		[Display(Name = "Документ переноса терминала для одного водителя")]
		SelfDriver
	}
}
