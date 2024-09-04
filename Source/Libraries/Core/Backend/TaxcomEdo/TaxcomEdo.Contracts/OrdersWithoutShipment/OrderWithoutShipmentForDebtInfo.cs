namespace TaxcomEdo.Contracts.OrdersWithoutShipment
{
	/// <summary>
	/// Информация о счете без погрузки на долг для ЭДО(электронного документооборота)
	/// </summary>
	public class OrderWithoutShipmentForDebtInfo : OrderWithoutShipmentInfo
	{
		public override string ToString()
		{
			return "Счет без отгрузки на долг";
		}
	}
}
