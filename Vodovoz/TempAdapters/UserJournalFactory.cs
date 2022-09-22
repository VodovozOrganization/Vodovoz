using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals;
using Vodovoz.Journals.FilterViewModels.Employees;
using Vodovoz.JournalViewers;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class UserJournalFactory : IUserJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSelectUserAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<SelectUserJournalViewModel>(typeof(User),
				() => new SelectUserJournalViewModel(
					new UserJournalFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory,
					new PermissionRepository(),
					ServicesConfig.CommonServices)
			);
		}
	}
}
