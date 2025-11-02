using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Journals;
using Vodovoz.ViewModels.Journals.FilterViewModels.Users;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class UserJournalFactory : IUserJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSelectUserAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<SelectUserJournalViewModel>(typeof(User),
				() => new SelectUserJournalViewModel(
					new UsersJournalFilterViewModel(),
					ServicesConfig.UnitOfWorkFactory,
					ServicesConfig.CommonServices)
			);
		}
	}
}
