using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface IEntitySelectionAutocompleteSelector<TEntity>
		where TEntity : class, IDomainObject
	{
		string GetTitle(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;
		void LoadAutocompletion(string[] searchText, int takeCount);
	}
}
