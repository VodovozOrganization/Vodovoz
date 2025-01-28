using System;
using System.Collections.Generic;
using System.Windows.Input;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class AddOrRemoveIDomainObjectViewModel : WidgetViewModelBase, IDisposable
	{
		private INamedDomainObject _selectedEntity;
		private string _title = "Неизвестно";
		private Action _addEntityAction;

		private void InitializeCommands()
		{
			AddCommand = new DelegateCommand(_addEntityAction);
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
			bool canEdit,
			string title,
			IList<INamedDomainObject> entities,
			Action AddEntityAction)
		{
			Title = title;
			SetCanEdit(canEdit);
			Entities = entities;
			_addEntityAction = AddEntityAction;
			InitializeCommands();
		}

		public void SetCanEdit(bool canEdit)
		{
			CanEdit = canEdit;
			OnPropertyChanged(nameof(CanEdit));
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
			_addEntityAction = null;
		}
	}
}
