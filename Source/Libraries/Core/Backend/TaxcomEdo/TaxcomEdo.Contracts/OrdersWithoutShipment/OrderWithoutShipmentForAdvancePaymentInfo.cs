namespace TaxcomEdo.Contracts.OrdersWithoutShipment
{
	/// <summary>
	/// Информация о счете без погрузки на предоплату для ЭДО(электронного документооборота)
	/// </summary>
	public class OrderWithoutShipmentForAdvancePaymentInfo : OrderWithoutShipmentInfo
	{
		public override string ToString()
		{
			return "Счет без отгрузки на предоплату";
		}
	}
}
