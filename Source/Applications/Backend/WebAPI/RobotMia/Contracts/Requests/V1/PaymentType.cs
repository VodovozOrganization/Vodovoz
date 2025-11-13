using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Requests.V1
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
		/// Терминал QR
		/// </summary>
		TerminalQR,
		/// <summary>
		/// Терминал по карте
		/// </summary>
		TerminalCard,
		/// <summary>
		/// Qr-код
		/// </summary>
		SmsQR
	}
}
