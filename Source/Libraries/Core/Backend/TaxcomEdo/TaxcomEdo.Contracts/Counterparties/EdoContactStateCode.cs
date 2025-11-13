using System.Text.Json.Serialization;

namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Статус контакта по ЭДО
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum EdoContactStateCode
	{
		/// <summary>
		/// Входящий
		/// </summary>
		Incoming,
		/// <summary>
		/// Отправлено приглашение
		/// </summary>
		Sent,
		/// <summary>
		/// Принят
		/// </summary>
		Accepted,
		/// <summary>
		/// Отклонен
		/// </summary>
		Rejected,
		/// <summary>
		/// Ошибка
		/// </summary>
		Error
	}
}
