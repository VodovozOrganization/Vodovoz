using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
	public class WaterJournalFactory : IEntityAutocompleteSelectorFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public WaterJournalFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public Type EntityType => typeof(Nomenclature);
		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			return CreateAutocompleteSelector(multipleSelect);
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var waterJournal = _lifetimeScope.Resolve<WaterJournalViewModel>();

			waterJournal.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
			return waterJournal;
		}
	}
}
