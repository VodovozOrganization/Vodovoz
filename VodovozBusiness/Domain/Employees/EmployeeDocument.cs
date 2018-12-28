using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы",
		Nominative = "документ")]
	public class EmployeeDocument: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		DocumentType document;

		[Display(Name = "Тип документа")]
		public virtual DocumentType Document {
			get { return document; }
			set { SetField(ref document, value, () => Document); }
		}

		Employee employee;

		[Display(Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value, () => Employee); }
		}

		public enum DocumentType
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
			TemporaryId
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
	}

	public class DocumentTypeStringType : NHibernate.Type.EnumStringType
	{
		public DocumentTypeStringType() : base(typeof(EmployeeDocument.DocumentType))
		{
		}
	}
}
