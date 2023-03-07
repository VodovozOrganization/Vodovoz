﻿using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class WarehouseJournalFactory : IWarehouseJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSelectorFactory(
			WarehouseJournalFilterViewModel filterViewModel = null)
		{
			return new EntityAutocompleteSelectorFactory<WarehouseJournalViewModel>(typeof(Warehouse), () =>
				new WarehouseJournalViewModel(
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					new SubdivisionRepository(new ParametersProvider()),
					filterViewModel ?? new WarehouseJournalFilterViewModel())
				{
					SelectionMode = QS.Project.Journal.JournalSelectionMode.Single
				});
		}
	}
}
