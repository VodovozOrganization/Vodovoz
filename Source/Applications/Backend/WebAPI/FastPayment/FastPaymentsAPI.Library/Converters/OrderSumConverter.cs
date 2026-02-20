namespace FastPaymentsAPI.Library.Converters
{
	public class OrderSumConverter : IOrderSumConverter
	{
		public int ConvertOrderSumToKopecks(decimal orderSum)
		{
			var sumInKopecks = (int)(orderSum * 100);
			return sumInKopecks;
		}
	}
}
