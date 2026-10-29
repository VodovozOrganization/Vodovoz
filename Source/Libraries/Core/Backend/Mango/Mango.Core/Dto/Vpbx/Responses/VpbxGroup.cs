using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Группа сотрудников ВАТС
	/// </summary>
	public class VpbxGroup
	{
		/// <summary>
		/// Id группы
		/// </summary>
		[JsonPropertyName("id")]
		public long Id { get; set; }

		/// <summary>
		/// Имя группы
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Примечание к группе
		/// </summary>
		[JsonPropertyName("description")]
		public string Description { get; set; }

		/// <summary>
		/// Короткий номер группы
		/// </summary>
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		/// <summary>
		/// Алгоритм распределения звонков в группе
		/// </summary>
		[JsonPropertyName("dial_alg_group")]
		public int? DialAlgGroup { get; set; }

		/// <summary>
		/// Алгоритм дозвона до сотрудников в группе
		/// </summary>
		[JsonPropertyName("dial_alg_users")]
		public int? DialAlgUsers { get; set; }

		/// <summary>
		/// Статус опции "Переадресовывать звонки на "знакомого" сотрудника": 0 - нет, 1 - да
		/// </summary>
		[JsonPropertyName("auto_redirect")]
		public int? AutoRedirect { get; set; }

		/// <summary>
		/// Id кампании исходящего обзвона для опции
		/// "Автоматически перезванивать по пропущенным звонкам"
		/// </summary>
		[JsonPropertyName("auto_dial")]
		public long? AutoDial { get; set; }

		/// <summary>
		/// Id исходящей линии для автоперезвона
		/// </summary>
		[JsonPropertyName("line_id")]
		public long? LineId { get; set; }

		/// <summary>
		/// Статус опции "До ответа оператора осталось ... минут": 0 - нет, 1 - да
		/// </summary>
		[JsonPropertyName("use_dynamic_ivr")]
		public int? UseDynamicIvr { get; set; }

		/// <summary>
		/// Статус опции "Ваш номер в очереди ...": 0 - нет, 1 - да
		/// </summary>
		[JsonPropertyName("use_dynamic_seq_num")]
		public int? UseDynamicSeqNum { get; set; }

		/// <summary>
		/// Идентификатор мелодии во время ожидания ответа.
		/// Если не заполнен, используется мелодия по умолчанию
		/// </summary>
		[JsonPropertyName("melody_id")]
		public long? MelodyId { get; set; }

		/// <summary>
		/// Состав группы.
		/// Заполняется, только если в запросе указан параметр show_users = 1
		/// </summary>
		[JsonPropertyName("operators")]
		public IReadOnlyList<VpbxGroupOperator> Operators { get; set; }
	}
}
