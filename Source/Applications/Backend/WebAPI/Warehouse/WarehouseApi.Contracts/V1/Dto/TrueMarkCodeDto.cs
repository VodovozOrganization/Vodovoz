using System.Text.Json.Serialization;
using TrueMark.Contracts;

namespace WarehouseApi.Contracts.V1.Dto
{
	/// <summary>
	/// Код маркировки ЧЗ
	/// </summary>
	public class TrueMarkCodeDto
	{
		/// <summary>
		/// Порядковый номер
		/// </summary>
		public int SequenceNumber { get; set; }

		/// <summary>
		/// Код честного знака
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Уровень кода
		/// </summary>
		public WarehouseApiTruemarkCodeLevel Level { get; set; }

		/// <summary>
		/// Родительский код
		/// </summary>
		public string Parent { get; set; }

		/// <summary>
		/// Товарная группа маркировки (вода / корма для животных).
		/// Для транспортных кодов равна null: такой код — контейнер и может содержать коды любых групп.
		/// </summary>
		
		[JsonConverter(typeof(TrueMarkProductGroupJsonConverter))]
		[JsonPropertyName("productGroup")]
		public TrueMarkProductGroup? ProductGroup { get; set; }
	}
}
