using System;
using System.Collections.Generic;

namespace TrueMark.Contracts.Responses
{
	public class CodeCheckResponse
	{
		/// <summary>
		/// Результат обработки операции (0 - «Успешно»; 4хх, 5хх — «Получен неверныйзапрос»)
		/// </summary>
		public int Code { get; set; }
		/// <summary>
		/// Текстовое описание результата выполнения метода
		/// возвращается значение <c>ok</c>, если значение параметра Code равно 0(«Успешно»),
		/// иначе возвращается сообщение об ошибке
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Список проверенных кодов маркировки
		/// </summary>
		//[JsonPropertyName("codes")]
		public IList<CodeCheckInfo> Codes { get; set; }
		/// <summary>
		/// Уникальный идентификатор запроса
		/// </summary>
		public Guid ReqId { get; set; }
		/// <summary>
		/// Дата и время регистрации запроса (в UTC)
		/// </summary>
		public long ReqTimestamp { get; set; }
	}
}
