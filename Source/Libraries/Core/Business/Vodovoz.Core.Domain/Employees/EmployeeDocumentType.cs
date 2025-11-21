using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Employees
{
	/// <summary>
	/// Тип документа сотрудника
	/// </summary>
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
