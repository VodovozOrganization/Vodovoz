using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "время действия рекламной листовки",
		Prepositional = "времени действия рекламной листовки"
	)]
	[HistoryTrace]
	public class FlyerActionTime : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private DateTime _startDate;
		private DateTime? _endDate;
		private Flyer _flyer;
		
		public virtual int Id { get; set; }

		[Display(Name = "Дата старта раздачи листовок")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}
		
		[Display(Name = "Дата окончания раздачи листовок")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		
		[Display(Name = "Листовка")]
		public virtual Flyer Flyer
		{
			get => _flyer;
			set => SetField(ref _flyer, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == default)
			{
				yield return new ValidationResult("Необходимо поставить актуальную дату старта выдачи листовки",
					new[] { nameof(StartDate) });
			}
		}
	}
}