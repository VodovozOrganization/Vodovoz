using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V5.Dto.Carts
{
	/// <summary>
	/// Шаги по проверкам в корзине
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum NextStepCheckUsersBasket
	{
		/// <summary>
		/// Пропускаем с предупреждением
		/// </summary>
		ProceedWithWarnings,
		/// <summary>
		/// Дальнейшее оформление невозможно
		/// </summary>
		Blocked,
		/// <summary>
		/// Пропускаем, все проверки прошли успешно
		/// </summary>
		Proceed
	}
}
