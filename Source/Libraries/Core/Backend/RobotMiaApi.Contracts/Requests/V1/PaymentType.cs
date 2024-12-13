using System.Text.Json.Serialization;

namespace RobotMiaApi.Contracts.Requests.V1
{
	/// <summary>
	/// Тип оплаты
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum PaymentType
	{
		/// <summary>
		/// Наличные
		/// </summary>
		Cash,
		/// <summary>
		/// Терминал
		/// </summary>
		Terminal,
		/// <summary>
		/// Qr-код
		/// </summary>
		SmsQR
	}
}
