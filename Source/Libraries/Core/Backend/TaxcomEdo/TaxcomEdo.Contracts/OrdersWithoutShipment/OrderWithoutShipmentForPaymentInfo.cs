namespace TaxcomEdo.Contracts.OrdersWithoutShipment
{
	/// <summary>
	/// Информация о счете без погрузки на постоплату для ЭДО(электронного документооборота)
	/// </summary>
	public class OrderWithoutShipmentForPaymentInfo : OrderWithoutShipmentInfo
	{
		public override string ToString()
		{
			return "Счет без отгрузки на постоплату";
		}
	}
}
