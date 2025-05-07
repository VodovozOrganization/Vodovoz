using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Dialog;
using Vodovoz.ViewModels.Services;

namespace Vodovoz.ViewModels.Widgets
{
	/// <summary>
	/// Вью модель для работы со списком Сущностей, поддерживающих <c>INamedDomainObject</c>
	/// </summary>
	public class AddOrRemoveIDomainObjectViewModel : AddOrRemoveIDomainObjectViewModelBase
	{
		private readonly EntityJournalOpener _journalOpener;

		private Type _entityType;

		public AddOrRemoveIDomainObjectViewModel(EntityJournalOpener journalOpener)
		{
			_journalOpener = journalOpener ?? throw new ArgumentNullException(nameof(journalOpener));
		}

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
			ParentTab = parentTab;
			Entities = entities;
			UoW = uow;
			
			InitializeCommands();
		}
		
		public void Configure(
			Type entityType,
			bool canEdit,
			string title,
			IUnitOfWork uow,
			DialogViewModelBase parentViewModel,
			IEnumerable<INamedDomainObject> entities)
		{
			_entityType = entityType;
			Title = title;
			SetCanEdit(canEdit);
			ParentViewModel = parentViewModel;
			Entities = entities;
			UoW = uow;
			
			InitializeCommands();
		}

		public void SetCanEdit(bool canEdit)
		{
			CanEdit = canEdit;
			OnPropertyChanged(nameof(CanEdit));
		}

		protected override void AddEntity()
		{
			var viewModel =
				ParentTab != null
					? _journalOpener.OpenJournalViewModelFromTdiTab(_entityType, ParentTab).ViewModel
					: _journalOpener.OpenJournalViewModelFromDialogViewModel(_entityType, ParentViewModel).ViewModel;

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
	}
}
