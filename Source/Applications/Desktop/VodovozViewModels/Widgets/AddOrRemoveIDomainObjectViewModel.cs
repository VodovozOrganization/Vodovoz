using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Bindings;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.ViewModels.Services;

namespace Vodovoz.ViewModels.Widgets
{
	/// <summary>
	/// Вью модель для работы со списком Сущностей, поддерживающих <c>INamedDomainObject</c>
	/// </summary>
	public class AddOrRemoveIDomainObjectViewModel : UoWWidgetViewModelBase, IDisposable
	{
		private readonly EntityJournalOpener _journalOpener;

		private Type _entityType;
		private INamedDomainObject _selectedEntity;
		private string _title = "Неизвестно";
		private ITdiTab _parentTab;
		private DialogTabViewModelBase _parentViewModel;

		public AddOrRemoveIDomainObjectViewModel(EntityJournalOpener journalOpener)
		{
			_journalOpener = journalOpener ?? throw new ArgumentNullException(nameof(journalOpener));
		}

		private void InitializeCommands()
		{
			AddCommand = new DelegateCommand(AddEntity);
			RemoveCommand = new DelegateCommand(RemoveEntity);
		}

		public IEnumerable<INamedDomainObject> Entities { get; private set; }

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
			IUnitOfWork uow,
			ITdiTab parentTab,
			IEnumerable<INamedDomainObject> entities)
		{
			_entityType = entityType;
			Title = title;
			SetCanEdit(canEdit);
			_parentTab = parentTab;
			Entities = entities;
			UoW = uow;
			
			InitializeCommands();
		}
		
		public void Configure(
			Type entityType,
			bool canEdit,
			string title,
			IUnitOfWork uow,
			DialogTabViewModelBase parentViewModel,
			IEnumerable<INamedDomainObject> entities)
		{
			_entityType = entityType;
			Title = title;
			SetCanEdit(canEdit);
			_parentViewModel = parentViewModel;
			Entities = entities;
			UoW = uow;
			
			InitializeCommands();
		}

		public void SetCanEdit(bool canEdit)
		{
			CanEdit = canEdit;
			OnPropertyChanged(nameof(CanEdit));
		}

		private void AddEntity()
		{
			var viewModel =
				_parentTab != null
					? _journalOpener.OpenJournalViewModelFromTdiTab(_entityType, _parentTab).ViewModel
					: _journalOpener.OpenJournalViewModelFromDialogViewModel(_entityType, _parentViewModel).ViewModel;

			if(!(viewModel is JournalViewModelBase journal))
			{
				return;
			}

			journal.SelectionMode = JournalSelectionMode.Multiple;
			journal.OnSelectResult += OnEntitySelectResult;
		}

		private void OnEntitySelectResult(object sender, JournalSelectedEventArgs e)
		{
			(sender as JournalViewModelBase).OnSelectResult -= OnEntitySelectResult;
			
			var addingEntities = e.SelectedObjects;

			foreach(var addingEntity in addingEntities)
			{
				var entity = UoW.GetById(_entityType, DomainHelper.GetId(addingEntity));
				
				if(Entities.Contains(entity))
				{
					continue;
				}
				
				(Entities as IList).Add(entity);
			}
		}

		private void RemoveEntity()
		{
			if(!Entities.Contains(SelectedEntity))
			{
				return;
			}

			(Entities as IList).Remove(SelectedEntity);
		}

		public void Dispose()
		{
			_parentTab = null;
		}
	}
}
