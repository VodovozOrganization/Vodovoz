using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Средство дозвона до сотрудника ВАТС
	/// </summary>
	public class VpbxMemberNumber
	{
		/// <summary>
		/// Номер. Зависит от <see cref="Protocol"/>: PSTN-номер, sip-номер или FMC-номер
		/// </summary>
		[JsonPropertyName("number")]
		public string Number { get; set; }

		/// <summary>
		/// Протокол номера телефона: tel - PSTN номер, sip - sip-номер, fmc - FMC номер
		/// </summary>
		[JsonPropertyName("protocol")]
		public string Protocol { get; set; } = VpbxNumberProtocols.Tel;

		/// <summary>
		/// Время ожидания ответа в секундах.
		/// Специальное значение 0 - действуют общие ограничения платформы или оператора связи
		/// </summary>
		[JsonPropertyName("wait_sec")]
		public int WaitSec { get; set; } = 30;

		/// <summary>
		/// Статус номера: on - активен, off - выключен
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; } = VpbxNumberStatuses.On;
	}
}
