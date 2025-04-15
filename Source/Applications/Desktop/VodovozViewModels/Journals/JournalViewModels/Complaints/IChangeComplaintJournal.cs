using System;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public interface IChangeComplaintJournal
	{
		Action<Type> ChangeView { get; set; }
	}
}
