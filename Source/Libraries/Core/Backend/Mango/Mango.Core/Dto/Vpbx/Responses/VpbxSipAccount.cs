using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// SIP-учётная запись сотрудника ВАТС
	/// </summary>
	public class VpbxSipAccount
	{
		/// <summary>
		/// Номер SIP-учётной записи
		/// </summary>
		[JsonPropertyName("number")]
		public string Number { get; set; }
	}
}
