using System.Collections.Generic;

namespace Vodovoz.EntityRepositories.Logistic
{
	/// <summary>
	/// Условия отбора выполненных адресов по водителям и районам для отчета по выполненным адресам
	/// </summary>
	public class CompletedAddressesCountsFilter
	{
		/// <summary>
		/// Идентификаторы включенных водителей МЛ. Пустой список - ограничения нет
		/// </summary>
		public IList<int> IncludedDriverIds { get; set; } = new List<int>();

		/// <summary>
		/// Идентификаторы исключенных водителей МЛ
		/// </summary>
		public IList<int> ExcludedDriverIds { get; set; } = new List<int>();

		/// <summary>
		/// Идентификаторы включенных районов. Пустой список и не включенный элемент "Без района" - ограничения нет
		/// </summary>
		public IList<int> IncludedDistrictIds { get; set; } = new List<int>();

		/// <summary>
		/// Идентификаторы исключенных районов
		/// </summary>
		public IList<int> ExcludedDistrictIds { get; set; } = new List<int>();

		/// <summary>
		/// Элемент "Без района" включен
		/// </summary>
		public bool IsWithoutDistrictIncluded { get; set; }

		/// <summary>
		/// Элемент "Без района" исключен
		/// </summary>
		public bool IsWithoutDistrictExcluded { get; set; }
	}
}
