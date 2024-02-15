﻿using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
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
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices)
			);
		}
	}
}
