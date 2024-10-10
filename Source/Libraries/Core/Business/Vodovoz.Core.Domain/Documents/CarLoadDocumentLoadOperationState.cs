using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "состояния талонов погрузки",
			Nominative = "состояние талона погрузки")]
	public enum CarLoadDocumentLoadOperationState
	{
		[Display(Name = "Погрузка не начата")]
		NotStarted,
		[Display(Name = "В процессе погрузки")]
		InProgress,
		[Display(Name = "Погрузка завершена")]
		Done
	}
}

