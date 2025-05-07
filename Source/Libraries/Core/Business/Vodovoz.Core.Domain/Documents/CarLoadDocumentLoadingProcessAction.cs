using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Действие при сборке талона погрузки автомобиля
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "действия при сборке талонов погрузки автомобилей",
		Nominative = "действие при сборке талона погрузки автомобиля")]
	public class CarLoadDocumentLoadingProcessAction : IDomainObject
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Идентификатор талона погрузки
		/// </summary>
		[Display(Name = "Id талона погрузки")]
		public virtual int CarLoadDocumentId { get; set; }

		/// <summary>
		/// Идентификатор сотрудника-сборщика талона
		/// </summary>
		[Display(Name = "Id сотрудника-сборщика талона")]
		public virtual int PickerEmployeeId { get; set; }

		/// <summary>
		/// Дата и время действия
		/// </summary>
		[Display(Name = "Дата и время действия")]
		public virtual DateTime ActionTime { get; set; }

		/// <summary>
		/// Тип действия
		/// </summary>
		[Display(Name = "Тип действия")]
		public virtual CarLoadDocumentLoadingProcessActionType ActionType { get; set; }
	}
}
