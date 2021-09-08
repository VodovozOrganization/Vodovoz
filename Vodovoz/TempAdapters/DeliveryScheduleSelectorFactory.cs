using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class DeliveryScheduleSelectorFactory : IDeliveryScheduleSelectorFactory
	{
		public IEntityAutocompleteSelectorFactory CreateDeliveryScheduleAutocompleteSelectorFactory()
		{
			return new SimpleEntitySelectorFactory<DeliverySchedule, DeliveryScheduleDlg>(() =>
			{
				var journal = new SimpleEntityJournalViewModel<DeliverySchedule, DeliveryScheduleDlg>(
					ds => ds.Name,
					() => new DeliveryScheduleDlg(),
					(node) => new DeliveryScheduleDlg(node.Id),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices);
				journal.SelectionMode = JournalSelectionMode.Single;
				return journal;
			});
		}
	}
}
