using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы",
		Nominative = "документ")]
	public class EmployeeDocument: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		EmployeeDocumentType document;

		[Display(Name = "Тип документа")]
		public virtual EmployeeDocumentType Document {
			get { return document; }
			set { SetField(ref document, value, () => Document); }
		}

		String title ="Документы";

		public virtual String Title {
			get { return title; }
			set { SetField(ref title, value, () => Title); }
		}

		String name;

		[Display(Name = "Название")]
		public virtual String Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		bool? mainDoc;

		[Display(Name = "Главный документ")]
		public virtual bool? MainDocument{
			get { return mainDoc; }
			set { SetField(ref mainDoc, value, () => MainDocument); }
		}

		string passportSeria;

		[Display(Name = "Серия паспорта")]
		public virtual string PassportSeria {
			get { return passportSeria; }
			set { SetField(ref passportSeria, value, () => PassportSeria); }
		}

		string passportNumber;

		[Display(Name = "Номер паспорта")]
		public virtual string PassportNumber {
			get { return passportNumber; }
			set { SetField(ref passportNumber, value, () => PassportNumber); }
		}

		string passportIssuedOrg;

		[Display(Name = "Кем выдан паспорт")]
		public virtual string PassportIssuedOrg {
			get { return passportIssuedOrg; }
			set { SetField(ref passportIssuedOrg, value, () => PassportIssuedOrg); }
		}

		private DateTime? passportIssuedDate;

		[Display(Name = "Дата выдачи паспорта")]
		public virtual DateTime? PassportIssuedDate {
			get { return passportIssuedDate; }
			set { SetField(ref passportIssuedDate, value, () => PassportIssuedDate); }
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(PassportSeria))
				yield return new ValidationResult("Серия должна быть заполнена", new[] { "PassportSeria" });
			if(String.IsNullOrEmpty(PassportNumber))
				yield return new ValidationResult("Номер должен быть заполнен", new[] { "PassportNumber" });
			//if((MainDocument==true) && (employee.GetMaindocument()!=null))
				//yield return new ValidationResult("Главные документ уже существует", new[] { "MainDocument" });
		}

	}

	public class DocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public DocumentTypeStringType() : base(typeof(EmployeeDocumentType))
		{
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
		[Display(Name = "Другое")]
		Other
	}
}
