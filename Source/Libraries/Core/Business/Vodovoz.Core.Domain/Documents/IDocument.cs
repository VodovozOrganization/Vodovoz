using System;

namespace Vodovoz.Core.Domain.Documents
{
	public interface IDocument
	{
		int? AuthorId { get; set; }
		bool CanEdit { get; set; }
		string DateString { get; }
		int Id { get; set; }
		DateTime LastEditedTime { get; set; }
		int? LastEditorId { get; set; }
		string Number { get; }
		DateTime TimeStamp { get; }
		DateTime Version { get; set; }
	}
}
