namespace Vodovoz.Models.TrueMark
{
	public interface ITrueMarkWaterCode
	{
		string GTIN { get; set; }
		string SerialNumber { get; set; }
		string CheckCode { get; set; }
	}
}
