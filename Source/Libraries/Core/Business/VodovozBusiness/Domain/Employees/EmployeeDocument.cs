using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы сотрудника",
		Nominative = "документ сотрудника")]
	[EntityPermission]
	public class EmployeeDocument: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private EmployeeDocumentType _document;

		[Display(Name = "Тип документа")]
		public virtual EmployeeDocumentType Document {
			get { return _document; }
			set { SetField(ref _document, value); }
		}

		private string _title ="Документы";

		public virtual string Title {
			get { return _title; }
			set { SetField(ref _title, value); }
		}

		private string _name;

		[Display(Name = "Название")]
		public virtual string Name {
			get { return _name; }
			set { SetField(ref _name, value); }
		}

		private bool? _mainDoc;

		[Display(Name = "Главный документ")]
		public virtual bool? MainDocument{
			get { return _mainDoc; }
			set { SetField(ref _mainDoc, value); }
		}

		private string _passportSeria;

		[Display(Name = "Серия паспорта")]
		public virtual string PassportSeria {
			get { return _passportSeria; }
			set { SetField(ref _passportSeria, value); }
		}

		private string _passportNumber;

		[Display(Name = "Номер паспорта")]
		public virtual string PassportNumber {
			get { return _passportNumber; }
			set { SetField(ref _passportNumber, value); }
		}

		private string _passportIssuedOrg;

		[Display(Name = "Кем выдан паспорт")]
		public virtual string PassportIssuedOrg {
			get { return _passportIssuedOrg; }
			set { SetField(ref _passportIssuedOrg, value); }
		}

		private DateTime? _passportIssuedDate;

		[Display(Name = "Дата выдачи паспорта")]
		public virtual DateTime? PassportIssuedDate {
			get { return _passportIssuedDate; }
			set { SetField(ref _passportIssuedDate, value); }
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

	public enum EmployeeDocumentType
	{
		[Display(Name = "Паспорт")]
		Passport,
		[Display(Name = "Загранпаспорт")]
		InternationalPassport,
		[Display(Name = "Свидетельство о рождении")]
		BirthCertificate,
		[Display(Name = "Удостоверение офицера")]
		OfficerCertificate,
		[Display(Name = "Справка об освобождении из места лишения свободы ")]
		PrisonReleaseCertificate,
		[Display(Name = "Паспорт морфлот")]
		NavyPassport,
		[Display(Name = "Военный билет")]
		MilitaryID,
		[Display(Name = "Диппаспорт")]
		Dippasport,
		[Display(Name = "Свидетельство беженца")]
		RefugeeCertificate,
		[Display(Name = "Вид на жительство")]
		Residence,
		[Display(Name = "Удостоверение беженца")]
		RefugeeId,
		[Display(Name = "Временное удостоверение")]
		TemporaryId,
		[Display(Name = "СНИЛС")]
		InsuranceNumber,
		[Display(Name = "ИНН")]
		INN,
		[Display(Name = "Паспорт иностранного гражданина")]
		ForeignCitizenPassport,
		[Display(Name = "Другое")]
		Other
	}
}
