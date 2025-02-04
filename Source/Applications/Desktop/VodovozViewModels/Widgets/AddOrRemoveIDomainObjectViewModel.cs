using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.ViewModels.Services;

namespace Vodovoz.ViewModels.Widgets
{
	/// <summary>
	/// Вью модель для работы со списком Сущностей, поддерживающих <c>INamedDomainObject</c>
	/// </summary>
	public class AddOrRemoveIDomainObjectViewModel : WidgetViewModelBase, IDisposable
	{
		private Type _entityType;
		private INamedDomainObject _selectedEntity;
		private string _title = "Неизвестно";
		private ITdiTab _parentTab;
		private EntityJournalOpener _journalOpener;

		public AddOrRemoveIDomainObjectViewModel(EntityJournalOpener journalOpener)
		{
			_journalOpener = journalOpener ?? throw new ArgumentNullException(nameof(journalOpener));
		}

		private void InitializeCommands()
		{
			AddCommand = new DelegateCommand(AddEntity);
			RemoveCommand = new DelegateCommand(RemoveEntity);
		}

		public IList<INamedDomainObject> Entities { get; private set; }

		public string Title
		{
			get => _title;
			private set => SetField(ref _title, value);
		}

		public INamedDomainObject SelectedEntity
		{
			get => _selectedEntity;
			set => SetField(ref _selectedEntity, value);
		}

		public bool CanRemoveEntity => CanEdit && SelectedEntity != null;
		public bool CanEdit { get; private set; }

		public ICommand AddCommand { get; private set; }
		public ICommand RemoveCommand { get; private set; }

		public void Configure(
			Type entityType,
			bool canEdit,
			string title,
			ITdiTab parentTab,
			IList<INamedDomainObject> entities)
		{
			_entityType = entityType;
			Title = title;
			SetCanEdit(canEdit);
			_parentTab = parentTab;
			Entities = entities;
			
			InitializeCommands();
		}

		public void SetCanEdit(bool canEdit)
		{
			CanEdit = canEdit;
			OnPropertyChanged(nameof(CanEdit));
		}

		private void AddEntity()
		{
			var viewModel = _journalOpener.OpenJournalViewModel(typeof(Subdivision), _parentTab).ViewModel;

			if(!(viewModel is JournalViewModelBase journal))
			{
				return;
			}
			
			journal.OnSelectResult += OnEntitySelectResult;
		}

		private void OnEntitySelectResult(object sender, JournalSelectedEventArgs e)
		{
			(sender as JournalViewModelBase).OnSelectResult -= OnEntitySelectResult;
			
			var addingEntities = e.SelectedObjects;

			foreach(var addingEntity in addingEntities)
			{
				if(Entities.Contains(addingEntity))
				{
					continue;
				}
				
				Entities.Add(SelectedEntity);
			}
		}

		private void RemoveEntity()
		{
			if(!Entities.Contains(SelectedEntity))
			{
				return;
			}

			Entities.Remove(SelectedEntity);
		}

		public void Dispose()
		{
			_parentTab = null;
		}
	}
}
