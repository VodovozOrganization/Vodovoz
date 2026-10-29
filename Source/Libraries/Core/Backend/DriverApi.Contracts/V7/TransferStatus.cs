using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V7
{
	/// <summary>
	/// Статус передачи
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum TransferStatus
	{
		/// <summary>
		/// Не передан
		/// </summary>
		NotTransfered,
		/// <summary>
		/// Передан
		/// </summary>
		Transfered
	}
}
