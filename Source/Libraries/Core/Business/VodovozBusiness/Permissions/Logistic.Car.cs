using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Logistic
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
			/// Доступ к отчету "Отчет о принадлежности ТС"
			/// </summary>
			[Display(
				Name = "Доступ к отчету \"Отчет о принадлежности ТС\"")]
			public static string HasAccessToCarOwnershipReport => "has_access_to_car_ownership_report";
		}
	}
}
