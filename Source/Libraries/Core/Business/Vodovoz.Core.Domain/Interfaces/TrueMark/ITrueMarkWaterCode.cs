namespace Vodovoz.Core.Domain.Interfaces.TrueMark
{
	public interface ITrueMarkWaterCode
	{
		string Gtin { get; set; }
		string SerialNumber { get; set; }
		string CheckCode { get; set; }
	}
}
