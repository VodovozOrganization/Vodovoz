using System;
using QS.Project.Dialogs.GtkUI;
using QS.RepresentationModel.GtkUI;

namespace Vodovoz.Infrastructure.Journal
{
	public class PermissionControlledRepresentationJournalFactory : IRepresentationJournalFactory
	{
		public RepresentationJournalDialog CreateJournal(IRepresentationModel model)
		{
			return new PermissionControlledRepresentationJournal(model);
		}
	}
}
