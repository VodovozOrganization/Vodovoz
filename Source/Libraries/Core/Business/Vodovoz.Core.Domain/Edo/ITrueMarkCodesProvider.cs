namespace Vodovoz.Core.Domain.TrueMark
{
	public interface ITrueMarkCodesProvider
	{
		string CashReceiptCode { get; }
		string IdentificationCode { get; }
		string FormatForCheck1260 { get; }
	}
}
