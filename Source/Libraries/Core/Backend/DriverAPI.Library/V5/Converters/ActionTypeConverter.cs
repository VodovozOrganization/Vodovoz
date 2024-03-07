using DriverApi.Contracts.V5;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.V5.Converters
{
	/// <summary>
	/// Конвертер типа действия в приложении
	/// </summary>
	public class ActionTypeConverter
	{
		/// <summary>
		/// Метод конвертации в DTO
		/// </summary>
		/// <param name="driverMobileAppActionType">Действия в мобильном приложении водителя из ДВ</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public ActionDtoType ConvertToAPIActionType(DriverMobileAppActionType driverMobileAppActionType)
		{
			ActionDtoType result;

			switch(driverMobileAppActionType)
			{
				case DriverMobileAppActionType.OpenOrderInfoPanel:
					result = ActionDtoType.OpenOrderInfoPanel;
					break;
				case DriverMobileAppActionType.OpenOrderDeliveryPanel:
					result = ActionDtoType.OpenOrderDeliveryPanel;
					break;
				case DriverMobileAppActionType.OpenOrderReceiptionPanel:
					result = ActionDtoType.OpenOrderReceiptionPanel;
					break;
				case DriverMobileAppActionType.CompleteOrderClicked:
					result = ActionDtoType.CompleteOrderClicked;
					break;
				default:
					throw new ConverterException(nameof(driverMobileAppActionType), driverMobileAppActionType, $"Значение {driverMobileAppActionType} не поддерживается");
			}

			return result;
		}

		/// <summary>
		/// Метод конвертации типа действия в мобильном приложении водителей в тип действий ДВ
		/// </summary>
		/// <param name="actionDtoType">Тип действия в Api</param>
		/// <returns></returns>
		/// <exception cref="ConverterException"></exception>
		public DriverMobileAppActionType ConvertToDriverMobileAppActionType(ActionDtoType actionDtoType)
		{
			DriverMobileAppActionType result;

			switch(actionDtoType)
			{
				case ActionDtoType.OpenOrderInfoPanel:
					result = DriverMobileAppActionType.OpenOrderInfoPanel;
					break;
				case ActionDtoType.OpenOrderDeliveryPanel:
					result = DriverMobileAppActionType.OpenOrderDeliveryPanel;
					break;
				case ActionDtoType.OpenOrderReceiptionPanel:
					result = DriverMobileAppActionType.OpenOrderReceiptionPanel;
					break;
				case ActionDtoType.CompleteOrderClicked:
					result = DriverMobileAppActionType.CompleteOrderClicked;
					break;
				default:
					throw new ConverterException(nameof(actionDtoType), actionDtoType, $"Значение {actionDtoType} не поддерживается");
			}

			return result;
		}
	}
}
