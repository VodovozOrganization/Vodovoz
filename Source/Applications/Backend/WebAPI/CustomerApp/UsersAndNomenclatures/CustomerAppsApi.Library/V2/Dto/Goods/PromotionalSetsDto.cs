using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Список промонаборов
	/// </summary>
	public class PromotionalSetsDto
	{
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
		/// <summary>
		/// Список промиков
		/// </summary>
		public IList<PromotionalSetDto> PromotionalSets { get; set; }
	}
}
