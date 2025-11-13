using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах сотружников",
		Nominative = "информация о прикрепленном файле сотрудника")]
	[HistoryTrace]
	public class EmployeeFileInformation : FileInformation
	{
		private int _employeeId;

		[Display(Name = "Идентификатор сотрудника")]
		[HistoryIdentifier(TargetType = typeof(EmployeeEntity))]
		public virtual int EmployeeId
		{
			get => _employeeId;
			set => _employeeId = value;
		}
	}
}
