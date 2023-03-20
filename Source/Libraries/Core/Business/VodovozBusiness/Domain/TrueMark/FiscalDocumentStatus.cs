namespace Vodovoz.Domain.TrueMark
{
	public enum FiscalDocumentStatus
	{
		None,
		Queued,
		Pending,
		Printed,
		WaitForCallback,
		Completed,
		Failed
	}
}
