using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace TaxcomEdo.Contracts.Documents
{
	public class InfoForCreatingBillWithoutShipmentForDebtEdo : InfoForCreatingBillWithoutShipmentEdo
	{
		public static readonly string ExchangeAndQueueName = "info-for-create-bills-without-shipment-for-debt";

		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public InfoForCreatingBillWithoutShipmentForDebtEdo() { }
		
		/// <summary>
		/// Информация о счете без погрузки на долг для ЭДО <see cref="OrderWithoutShipmentForDebtInfo"/>
		/// </summary>
		public OrderWithoutShipmentForDebtInfo OrderWithoutShipmentForDebtInfo
		{
			get => OrderWithoutShipmentInfo as OrderWithoutShipmentForDebtInfo;
			set => OrderWithoutShipmentInfo = value;
		}

		public BillFileData BillFileData
		{
			get => FileData as BillFileData;
			set => FileData = value;
		}
	}
}
