using QS.Project.Journal.EntitySelector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class WarehouseJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSelectorFactory()
		{
			return new WarehouseSelectorFactory();
		}
	}
}
