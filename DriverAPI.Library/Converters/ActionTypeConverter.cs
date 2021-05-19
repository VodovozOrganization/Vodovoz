using DriverAPI.Library.Models;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.Converters
{
	public class ActionTypeConverter
	{
		public APIActionType ConvertToAPIActionType(DriverMobileAppActionType driverMobileAppActionType)
		{
			APIActionType result;

			switch (driverMobileAppActionType)
			{
				case DriverMobileAppActionType.OpenOrderInfoPanel:
					result = APIActionType.OpenOrderInfoPanel;
					break;
				case DriverMobileAppActionType.OpenOrderDeliveryPanel:
					result = APIActionType.OpenOrderDeliveryPanel;
					break;
				case DriverMobileAppActionType.OpenOrderReceiptionPanel:
					result = APIActionType.OpenOrderReceiptionPanel;
					break;
				case DriverMobileAppActionType.CompleteOrderClicked:
					result = APIActionType.CompleteOrderClicked;
					break;
				default:
					throw new ConverterException(nameof(driverMobileAppActionType), driverMobileAppActionType, $"Значение {driverMobileAppActionType} не поддерживается");
			}

			return result;
		}

		public DriverMobileAppActionType ConvertToDriverMobileAppActionType(APIActionType aPIActionType)
		{
			DriverMobileAppActionType result;

			switch (aPIActionType)
			{
				case APIActionType.OpenOrderInfoPanel:
					result = DriverMobileAppActionType.OpenOrderInfoPanel;
					break;
				case APIActionType.OpenOrderDeliveryPanel:
					result = DriverMobileAppActionType.OpenOrderDeliveryPanel;
					break;
				case APIActionType.OpenOrderReceiptionPanel:
					result = DriverMobileAppActionType.OpenOrderReceiptionPanel;
					break;
				case APIActionType.CompleteOrderClicked:
					result = DriverMobileAppActionType.CompleteOrderClicked;
					break;
				default:
					throw new ConverterException(nameof(aPIActionType), aPIActionType, $"Значение {aPIActionType} не поддерживается");
			}

			return result;
		}
	}
}
