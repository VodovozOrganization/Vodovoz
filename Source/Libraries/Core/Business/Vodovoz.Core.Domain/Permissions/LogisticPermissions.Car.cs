using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class LogisticPermissions
	{
		public static class Car
		{
			/// <summary>
			/// Изменение модели в автомобиле
			/// </summary>
			[Display(
				Name = "Изменение модели в автомобиле")]
			public static string CanChangeCarModel => "can_change_car_model";

			/// <summary>
			/// Изменение номера ТК в карточке автомобиля
			/// </summary>
			[Display(
				Name = "Изменение номера ТК в карточке автомобиля")]
			public static string CanChangeFuelCardNumber => "can_change_fuel_card_number";

			/// <summary>
			/// Изменение в автомобиле количества забираемых бутылей с адреса
			/// </summary>
			[Display(
				Name = "Изменение в автомобиле количества забираемых бутылей с адреса")]
			public static string CanChangeCarsBottlesFromAddress => "can_change_cars_bottles_from_address";

			/// <summary>
			/// Изменение принадлежности в автомобиле
			/// </summary>
			[Display(
				Name = "Изменение принадлежности в автомобиле")]
			public static string CanChangeCarVersion => "can_change_car_version";

			/// <summary>
			/// Изменение даты старой версии в автомобиле
			/// </summary>
			[Display(
				Name = "Изменение даты старой версии в автомобиле")]
			public static string CanChangeCarVersionDate => "can_change_car_version_date";

			/// <summary>
			/// Доступ к отчету "Отчет о принадлежности ТС"
			/// </summary>
			[Display(
				Name = "Доступ к отчету \"Отчет о принадлежности ТС\"")]
			public static string HasAccessToCarOwnershipReport => "has_access_to_car_ownership_report";

			/// <summary>
			/// Доступ к настройке уведомления о приближающемся ТО
			/// </summary>
			[Display(
				Name = "Доступ к настройке уведомления о приближающемся ТО")]
			public static string CanEditTechInspectSetting => nameof(CanEditTechInspectSetting);

			/// <summary>
			/// Доступ к настройке уведомления о приближающейся страховке
			/// </summary>
			[Display(
				Name = "Доступ к настройке уведомления о приближающейся страховке")]
			public static string CanEditInsuranceNotificationsSettings => "can_edit_insurance_notifications_settings";

			/// <summary>
			/// Доступ к настройке уведомления о приближающемся ГТО авто
			/// </summary>
			[Display(
				Name = "Доступ к настройке уведомления о приближающейся страховке")]
			public static string CanEditCarTechnicalCheckupNotificationsSettings => "can_edit_car_technical_checkup_notifications_settings";

			/// <summary>
			/// Создание калибровки баланса топлива
			/// </summary>
			[Display(
				Name = "Калибровка баланса топлива",
				Description = "Пользователь может создавать событие авто для калибровки баланса топлива")]
			public static string CanCreateFuelBalanceCalibrationCarEvent => "can_create_fuel_balance_calibration_car_event";
		}
	}
}
