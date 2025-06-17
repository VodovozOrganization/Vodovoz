using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Состояние сборки талона погрузки автомобиля
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "состояния талонов погрузки",
		Nominative = "состояние талона погрузки")]
	public enum CarLoadDocumentLoadOperationState
	{
		/// <summary>
		/// Погрузка не начата
		/// </summary>
		[Display(Name = "Погрузка не начата")]
		NotStarted,
		/// <summary>
		/// В процессе погрузки
		/// </summary>
		[Display(Name = "В процессе погрузки")]
		InProgress,
		/// <summary>
		/// Погрузка завершена
		/// </summary>
		[Display(Name = "Погрузка завершена")]
		Done
	}
}
