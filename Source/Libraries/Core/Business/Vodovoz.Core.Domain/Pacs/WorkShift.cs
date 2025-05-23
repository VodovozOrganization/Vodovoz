using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Pacs
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "смены",
		Nominative = "смена")]
	[EntityPermission]
	[HistoryTrace]
	public class WorkShift : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private string _name;
		private TimeSpan _duration;

		/// <summary>
		/// Для NHibernate, не использовать.
		/// </summary>
		protected WorkShift()
		{
		}

		/// <summary>
		/// Конструктор для создания смены.
		/// </summary>
		/// <param name="name">Название смены</param>
		/// <param name="duration">Длительность смены</param>
		protected WorkShift(string name, TimeSpan duration)
		{
			Name = name;
			Duration = duration;
		}

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			protected set => SetField(ref _id, value);
		}

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			protected set => SetField(ref _name, value);
		}


		[Display(Name = "Длительность")]
		public virtual TimeSpan Duration
		{
			get => _duration;
			protected set => SetField(ref _duration, value);
		}

		/// <summary>
		/// Создает смену с заданным названием и длительностью.
		/// </summary>
		/// <param name="name">Название</param>
		/// <param name="duration">Длительность</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static WorkShift Create(string name, TimeSpan duration)
		{
			if(duration <= TimeSpan.Zero)
			{
				throw new ArgumentException("Длительность должна быть больше нуля", nameof(Duration));
			}

			if(string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("Название должно быть заполнено", nameof(name));
			}

			return new WorkShift(name, duration);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Duration <= TimeSpan.Zero)
			{
				yield return new ValidationResult("Длительность должна быть больше нуля", new[] { nameof(Duration) });
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
			}
		}
	}
}
