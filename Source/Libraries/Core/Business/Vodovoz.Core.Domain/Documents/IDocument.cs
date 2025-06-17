using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Documents
{
	public interface IDocument
	{
		Employee Author { get; set; }
		bool CanEdit { get; set; }
		string DateString { get; }
		int Id { get; set; }
		DateTime LastEditedTime { get; set; }
		Employee LastEditor { get; set; }
		string Number { get; }
		DateTime TimeStamp { get; set; }
		DateTime Version { get; set; }
	}
}