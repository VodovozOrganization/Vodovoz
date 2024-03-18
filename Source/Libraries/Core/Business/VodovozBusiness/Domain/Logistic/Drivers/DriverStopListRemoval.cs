using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "снятие стопа-листов водителям",
		Nominative = "снятие стоп-листа водителю")]
	[EntityPermission]
	[HistoryTrace]
	public class DriverStopListRemoval : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Employee _driver;
		private Employee _author;
		private DateTime _dateFrom;
		private DateTime _dateTo;
		private string _comment = string.Empty;

		public virtual int Id { get; set; }

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Сотрудник, снявший стоп-лист водителю")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Стоп-лист снят с")]
		public virtual DateTime DateFrom
		{
			get => _dateFrom;
			set => SetField(ref _dateFrom, value);
		}

		[Display(Name = "Стоп-лист снят по")]
		public virtual DateTime DateTo
		{
			get => _dateTo;
			set => SetField(ref _dateTo, value);
		}

		[Display(Name = "Комментарий по снятию стоп-листа")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public virtual string Title =>
			$"Снятие стоп-листа №{Id} водителя {Driver?.FullName} c {DateFrom:dd.MM.yyyy HH:mm} по {DateTo:dd.MM.yyyy HH:mm}";

		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Id != 0)
			{
				yield return new ValidationResult(string.Format("Нельзя редактировать уже созданное снятие стоп-листа"));
			}

			if(Driver == null)
			{
				yield return new ValidationResult("Не указан водитель, которому снимается стоп-лист", new[] { nameof(Driver) });
			}

			if(Driver != null && Driver.Category != EmployeeCategory.driver)
			{
				yield return new ValidationResult("Сотрудник, которому снимается стоп-лист не является водителем", new[] { nameof(Driver) });
			}

			if(Author == null)
			{
				yield return new ValidationResult("Не указан сотрудник, снимающий стоп-лист", new[] { nameof(Author) });
			}

			if(DateFrom == default(DateTime))
			{
				yield return new ValidationResult("Не указаны дата и время начала снятия стоп-листа", new[] { nameof(DateFrom) });
			}

			if(DateTo == default(DateTime))
			{
				yield return new ValidationResult("Не указаны дата и время окончания снятия стоп-листа", new[] { nameof(DateTo) });
			}

			if(string.IsNullOrEmpty(Comment))
			{
				yield return new ValidationResult("Не добавлен комментарий по снятию стоп-листа", new[] { nameof(Comment) });
			}

			if(!string.IsNullOrEmpty(Comment) && Comment.Length > 800)
			{
				yield return new ValidationResult("Длина комментария не должна превышать 800 символов", new[] { nameof(Comment) });
			}
		}
		#endregion
	}
}
