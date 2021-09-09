using System.Collections.Generic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IUserJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateUserAutocompleteSelectorFactory();
	}
}
