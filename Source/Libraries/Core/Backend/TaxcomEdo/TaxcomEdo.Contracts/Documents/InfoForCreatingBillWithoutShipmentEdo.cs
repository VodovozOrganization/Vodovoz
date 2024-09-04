using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Необходимая информация для отправки счета без погрузки по ЭДО
	/// </summary>
	public class InfoForCreatingBillWithoutShipmentEdo : InfoForCreatingDocumentEdoWithAttachment
	{
		protected InfoForCreatingBillWithoutShipmentEdo(OrderWithoutShipmentInfo orderWithoutShipmentInfo, FileData fileData)
			: base(fileData)
		{
			OrderWithoutShipmentInfo = orderWithoutShipmentInfo;
		}
		
		/// <summary>
		/// Информация о счете без погрузки для ЭДО <see cref="OrderWithoutShipmentInfo"/>
		/// </summary>
		public OrderWithoutShipmentInfo OrderWithoutShipmentInfo { get; }

		public string GetBillWithoutShipmentInfoTitle()
		{
			if(OrderWithoutShipmentInfo is null)
			{
				return "Не заполнена информация о счете без отгрузки";
			}

			return OrderWithoutShipmentInfo.ToString();
		}

		public static InfoForCreatingBillWithoutShipmentEdo Create(OrderWithoutShipmentInfo orderWithoutShipmentInfo, FileData fileData)
			=> new InfoForCreatingBillWithoutShipmentEdo(orderWithoutShipmentInfo, fileData);
	}
}
