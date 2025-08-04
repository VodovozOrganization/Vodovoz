using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Dialog;

namespace Vodovoz.ViewModels.Widgets
{
	public abstract class AddOrRemoveIDomainObjectViewModelBase : UoWWidgetViewModelBase, IDisposable
	{
		protected ITdiTab ParentTab;
		protected DialogViewModelBase ParentViewModel;
		private INamedDomainObject _selectedEntity;
		private string _title = "Неизвестно";
		
		protected void InitializeCommands()
		{
			AddCommand = new DelegateCommand(AddEntity, () => CanEdit);

			var removeCommand = new DelegateCommand(RemoveEntity, () => CanRemoveEntity);
			removeCommand.CanExecuteChangedWith(this, x => x.CanRemoveEntity);
			RemoveCommand = removeCommand;
		}

		public IEnumerable<INamedDomainObject> Entities { get; protected set; }

		public string Title
		{
			get => _title;
			protected set => SetField(ref _title, value);
		}

		[PropertyChangedAlso(nameof(CanRemoveEntity))]
		public INamedDomainObject SelectedEntity
		{
			get => _selectedEntity;
			set => SetField(ref _selectedEntity, value);
		}

		public bool CanRemoveEntity => CanEdit && SelectedEntity != null;
		public bool CanEdit { get; protected set; }

		public ICommand AddCommand { get; protected set; }
		public ICommand RemoveCommand { get; protected set; }

		protected void SetCanEdit(bool canEdit)
		{
			CanEdit = canEdit;
			OnPropertyChanged(nameof(CanEdit));
		}

		protected abstract void AddEntity();

		protected virtual void RemoveEntity()
		{
			if(!Entities.Contains(SelectedEntity))
			{
				return;
			}

			(Entities as IList).Remove(SelectedEntity);
		}
		
		public virtual void Dispose()
		{
			ParentTab = null;
			ParentViewModel = null;
		}
	}
}
