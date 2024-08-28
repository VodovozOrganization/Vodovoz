using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах сотружников",
		Nominative = "информация о прикрепленном файле сотрудника")]
	public class EmployeeFileInformation : FileInformation
	{
		private int _employeeId;

		[Display(Name = "Идентификатор сотрудника")]
		public virtual int EmployeeId
		{
			get => _employeeId;
			set => _employeeId = value;
		}
	}
}
