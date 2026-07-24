using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Телефония сотрудника ВАТС
	/// </summary>
	public class VpbxUserTelephony
	{
		/// <summary>
		/// Внутренний номер сотрудника
		/// </summary>
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		/// <summary>
		/// Номер исходящей линии сотрудника.
		/// Заполняется, только если в запросе указано дополнительное поле outgoingline
		/// </summary>
		[JsonPropertyName("outgoingline")]
		public string OutgoingLine { get; set; }

		/// <summary>
		/// Id исходящей линии сотрудника.
		/// Заполняется, только если в запросе указано дополнительное поле line_id
		/// </summary>
		[JsonPropertyName("line_id")]
		public long? LineId { get; set; }

		/// <summary>
		/// Id номера sip-trunk'а исходящего номера.
		/// Заполняется, только если в запросе указано дополнительное поле trunk_number_id
		/// </summary>
		[JsonPropertyName("trunk_number_id")]
		public long? TrunkNumberId { get; set; }

		/// <summary>
		/// Алгоритм дозвона.
		/// Заполняется, только если в запросе указано дополнительное поле dial_alg
		/// </summary>
		[JsonPropertyName("dial_alg")]
		public int? DialAlg { get; set; }

		/// <summary>
		/// Средства дозвона до сотрудника
		/// </summary>
		[JsonPropertyName("numbers")]
		public IReadOnlyList<VpbxUserNumber> Numbers { get; set; }
	}
}
