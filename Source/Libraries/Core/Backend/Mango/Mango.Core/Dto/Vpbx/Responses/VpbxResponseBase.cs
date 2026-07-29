using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Общая часть ответа API ВАТС
	/// </summary>
	public abstract class VpbxResponseBase
	{
		/// <summary>
		/// Код результата, см. <see cref="VpbxResultCodes"/>.
		/// Возвращается не всеми методами API, поэтому может быть не заполнен
		/// </summary>
		[JsonPropertyName("result")]
		public int? Result { get; set; }

		/// <summary>
		/// Описание ошибки. Опциональное, заполняется не всегда
		/// </summary>
		[JsonPropertyName("description")]
		public string Description { get; set; }

		/// <summary>
		/// Параметры запроса, к которым относится ошибка, например {"groupId": 0}.
		/// Состав зависит от метода и от кода результата, поэтому хранится в исходном виде
		/// </summary>
		[JsonPropertyName("fields")]
		public JsonElement? Fields { get; set; }
	}
}
