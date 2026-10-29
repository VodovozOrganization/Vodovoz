using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Изменяемые данные группы ВАТС
	/// </summary>
	public class VpbxGroupUpdate
	{
		/// <summary>
		/// Состав группы. Передаваемый массив полностью заменяет текущий состав,
		/// поэтому в нём должны быть перечислены все сотрудники, которые должны остаться в группе
		/// </summary>
		[JsonPropertyName("operators")]
		public IEnumerable<VpbxGroupOperatorUpdate> Operators { get; set; }
	}
}
