using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы сотрудника",
		Nominative = "документ сотрудника")]
	[EntityPermission]
	public class EmployeeDocument: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private string _title = "Документы";
		private string _name;
		private bool? _mainDoc;
		private string _passportSeria;
		private string _passportNumber;
		private string _passportIssuedOrg;
		private DateTime? _passportIssuedDate;
		private EmployeeDocumentType _document;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Заголовок
		/// </summary>
		[Display(Name = "Заголовок")]
		public virtual string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Главный документ
		/// </summary>
		[Display(Name = "Главный документ")]
		public virtual bool? MainDocument
		{
			get => _mainDoc;
			set => SetField(ref _mainDoc, value);
		}

		/// <summary>
		/// Серия паспорта
		/// </summary>
		[Display(Name = "Серия паспорта")]
		public virtual string PassportSeria
		{
			get => _passportSeria;
			set => SetField(ref _passportSeria, value);
		}

		/// <summary>
		/// Номер паспорта
		/// </summary>
		[Display(Name = "Номер паспорта")]
		public virtual string PassportNumber
		{
			get => _passportNumber;
			set => SetField(ref _passportNumber, value);
		}

		/// <summary>
		/// Кем выдан паспорт
		/// </summary>
		[Display(Name = "Кем выдан паспорт")]
		public virtual string PassportIssuedOrg
		{
			get => _passportIssuedOrg;
			set => SetField(ref _passportIssuedOrg, value);
		}

		/// <summary>
		/// Дата выдачи паспорта
		/// </summary>
		[Display(Name = "Дата выдачи паспорта")]
		public virtual DateTime? PassportIssuedDate
		{
			get => _passportIssuedDate;
			set => SetField(ref _passportIssuedDate, value);
		}

		/// <summary>
		/// Тип документа
		/// </summary>
		[Display(Name = "Тип документа")]
		public virtual EmployeeDocumentType Document
		{
			get => _document;
			set => SetField(ref _document, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(PassportSeria))
			{
				yield return new ValidationResult("Серия должна быть заполнена", new[] { "PassportSeria" });
			}

			if(string.IsNullOrEmpty(PassportNumber))
			{
				yield return new ValidationResult("Номер должен быть заполнен", new[] { "PassportNumber" });
			}
		}

	}
}
