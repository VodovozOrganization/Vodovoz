using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface IEntityDialogSelectionAutocompleteSelector<TEntity>
		where TEntity : class, IDomainObject
	{
		#region Autocomplete
		event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;
		string GetTitle(object node);
		void LoadAutocompletion(string[] searchText, int takeCount);
		#endregion

		#region EntitySelection
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
		event EventHandler SelectEntityFromJournalSelected;
		void OpenSelector();
		#endregion
	}
}
