using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace TaxcomEdo.Contracts.Documents
{
	public class InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo : InfoForCreatingBillWithoutShipmentEdo
	{
		public static readonly string ExchangeAndQueueName = "info-for-create-bills-without-shipment-for-advance-payment";
		
		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo() { }

		/// <summary>
		/// Информация о счете без погрузки на долг для ЭДО <see cref="OrderWithoutShipmentForAdvancePaymentInfo"/>
		/// </summary>
		public OrderWithoutShipmentForAdvancePaymentInfo OrderWithoutShipmentForAdvancePaymentInfo
		{
			get => OrderWithoutShipmentInfo as OrderWithoutShipmentForAdvancePaymentInfo;
			set => OrderWithoutShipmentInfo = value;
		}

		public BillFileData BillFileData
		{
			get => FileData as BillFileData;
			set => FileData = value;
		}
	}
}
