namespace Vodovoz.Core.Domain.TrueMark
{
	public interface ITrueMarkCodesProvider
	{
		string CashReceiptCode { get; }
		string IdentificationCode { get; }
		string Tag1260Code { get; }
	}
}