﻿using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Factories
{
	public class CarManufacturerJournalFactory : ICarManufacturerJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateCarManufacturerAutocompleteSelectorFactory(bool multipleSelect = false)
		{
			return new EntityAutocompleteSelectorFactory<CarManufacturerJournalViewModel>(
				typeof(CarManufacturer),
				() => new CarManufacturerJournalViewModel(
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices)
				{
					SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
				});
		}
	}
}
