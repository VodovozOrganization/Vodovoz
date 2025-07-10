using System.Text.Json.Serialization;

namespace Contracts.Common
{
	/// <summary>
	/// Куда отправлять данные
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum SendTo
	{
		/// <summary>
		/// На телефон
		/// </summary>
		Phone,
		/// <summary>
		/// На электронную почту
		/// </summary>
		Email
	}
}
