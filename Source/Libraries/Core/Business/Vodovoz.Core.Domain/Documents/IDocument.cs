using System;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Domain.Documents
{
	public interface IDocument
	{
		EmployeeEntity Author { get; set; }
		bool CanEdit { get; set; }
		string DateString { get; }
		int Id { get; set; }
		DateTime LastEditedTime { get; set; }
		EmployeeEntity LastEditor { get; set; }
		string Number { get; }
		DateTime TimeStamp { get; set; }
		DateTime Version { get; set; }
	}
}
