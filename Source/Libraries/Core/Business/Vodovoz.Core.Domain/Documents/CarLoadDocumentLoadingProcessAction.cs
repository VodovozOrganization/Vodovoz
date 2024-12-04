using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "действия при сборке талонов погрузки автомобилей",
		Nominative = "действие при сборке талона погрузки автомобиля")]
	public class CarLoadDocumentLoadingProcessAction : IDomainObject
	{
		public virtual int Id { get; set; }

		[Display(Name = "Id талона погрузки")]
		public virtual int CarLoadDocumentId { get; set; }

		[Display(Name = "Id сотрудника-сборщика талона")]
		public virtual int PickerEmployeeId { get; set; }

		[Display(Name = "Дата и время действия")]
		public virtual DateTime ActionTime { get; set; }

		[Display(Name = "Тип действия")]
		public virtual CarLoadDocumentLoadingProcessActionType ActionType { get; set; }
	}
}
