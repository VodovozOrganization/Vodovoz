using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace TaxcomEdo.Contracts.Documents
{
	public class InfoForCreatingBillWithoutShipmentForPaymentEdo : InfoForCreatingBillWithoutShipmentEdo
	{
		public static readonly string ExchangeAndQueueName = "info-for-create-bills-without-shipment-for-payment";

		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public InfoForCreatingBillWithoutShipmentForPaymentEdo() { }
		
		/// <summary>
		/// Информация о счете без погрузки на постоплату для ЭДО <see cref="OrderWithoutShipmentForPaymentInfo"/>
		/// </summary>
		public OrderWithoutShipmentForPaymentInfo OrderWithoutShipmentForPaymentInfo
		{
			get => OrderWithoutShipmentInfo as OrderWithoutShipmentForPaymentInfo;
			set => OrderWithoutShipmentInfo = value;
		}

		public BillFileData BillFileData
		{
			get => FileData as BillFileData;
			set => FileData = value;
		}
	}
}
