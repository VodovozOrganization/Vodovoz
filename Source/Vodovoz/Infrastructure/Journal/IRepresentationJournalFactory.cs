using System;
using QS.Project.Dialogs.GtkUI;
using QS.RepresentationModel.GtkUI;

namespace Vodovoz.Infrastructure.Journal
{
	public interface IRepresentationJournalFactory
	{
		RepresentationJournalDialog CreateJournal(IRepresentationModel model);
	}
}
