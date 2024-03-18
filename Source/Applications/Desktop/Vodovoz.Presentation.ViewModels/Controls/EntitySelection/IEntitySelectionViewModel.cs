using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface IEntitySelectionViewModel : INotifyPropertyChanged, IDisposable
	{
		bool DisposeViewModel { get; set; }

		#region Выбранная сущьность
		string EntityTitle { get; }
		object Entity { get; set; }
		#endregion

		#region События для внешних подписчиков
		event EventHandler Changed;
		event EventHandler ChangedByUser;
		#endregion

		#region Настройки виджета
		bool IsEditable { get; set; }
		#endregion

		#region Доступность функций View
		bool CanSelectEntity { get; }
		bool CanClearEntity { get; }
		bool SensitiveAutoCompleteEntry { get; }
		#endregion

		#region Команды от View
		DelegateCommand OpenSelectDialogCommand { get; }
		DelegateCommand ClearEntityCommand { get; }
		#endregion

		#region Автодополнение
		int AutocompleteListSize { get; set; }
		void AutocompleteTextEdited(string text);
		string GetAutocompleteTitle(object node);
		void AutocompleteSelectNode(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;
		#endregion
	}

	public class AutocompleteUpdatedEventArgs : EventArgs
	{
		public IList List;

		public AutocompleteUpdatedEventArgs(IList list)
		{
			this.List = list ?? throw new ArgumentNullException(nameof(list));
		}
	}

	public class EntitySelectedEventArgs : EventArgs
	{
		public object Entity;

		public EntitySelectedEventArgs(object entity)
		{
			Entity = entity ?? throw new ArgumentNullException(nameof(entity));
		}
	}

	public interface IEntitySelectionAutocompleteSelector<TEntity>
		where TEntity : class, IDomainObject
	{
		string GetTitle(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;
		void LoadAutocompletion(string[] searchText, int takeCount);
		IList<TEntity> GetEntities();
	}

	public interface IEntitySelectionAdapter<TEntity>
		where TEntity : class, IDomainObject
	{
		TEntity GetEntityByNode(object node);
		EntitySelectionViewModel<TEntity> EntitySelectionViewModel { set; }
	}

	public interface IEntityJournalSelector
	{
		void OpenSelector(string dialogTitle = null);
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
	}
}
