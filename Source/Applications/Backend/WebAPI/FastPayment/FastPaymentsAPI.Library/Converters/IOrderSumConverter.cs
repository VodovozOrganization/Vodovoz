namespace FastPaymentsAPI.Library.Converters
{
	public interface IOrderSumConverter
	{
		int ConvertOrderSumToKopecks(decimal orderSum);
	}
}
