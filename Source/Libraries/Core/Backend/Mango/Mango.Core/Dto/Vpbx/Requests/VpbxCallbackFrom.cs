using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Инициатор звонка в команде обратного звонка
	/// </summary>
	public class VpbxCallbackFrom
	{
		/// <summary>
		/// Внутренний номер сотрудника ВАТС, которому ВАТС дозванивается первым.
		/// Звонок выполняется по исходящей линии, указанной в карточке этого сотрудника
		/// </summary>
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		/// <summary>
		/// Номер телефона инициатора звонка.
		/// Заполняется, когда дозвониться нужно не на все средства связи сотрудника,
		/// а на конкретный номер. Если не заполнен, средство дозвона выбирает ВАТС
		/// по настройкам сотрудника
		/// </summary>
		[JsonPropertyName("number")]
		public string Number { get; set; }
	}
}
