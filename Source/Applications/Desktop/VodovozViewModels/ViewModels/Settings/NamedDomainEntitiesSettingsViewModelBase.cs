using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public abstract class NamedDomainEntitiesSettingsViewModelBase : WidgetViewModelBase
	{
		private INamedDomainObject _selectedEntity;

		protected NamedDomainEntitiesSettingsViewModelBase(
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
			string parameterName)
		{
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			UnitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			GeneralSettingsParametersProvider =
				generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));

			GetEntitiesCollection();
			InitializeCommands();
			
			ParameterName = parameterName;
		}

		protected abstract void GetEntitiesCollection();

		protected ICommonServices CommonServices { get; }
		protected IUnitOfWorkFactory UnitOfWorkFactory { get; }
		protected IGeneralSettingsParametersProvider GeneralSettingsParametersProvider { get; }
		public DelegateCommand AddEntityCommand { get; private set; }
		public DelegateCommand RemoveEntityCommand { get; private set; }
		public DelegateCommand SaveEntitiesCommand { get; private set; }
		public DelegateCommand ShowInfoCommand { get; private set; }
		
		public string ParameterName { get; }
		public bool CanEdit { get; set; }
		public string DetailTitle { get; set; }
		public string MainTitle { get; set; }
		public bool CanRemove => CanEdit && SelectedEntity != null;
		public bool CanSave => CanEdit && ObservableEntities != null && ObservableEntities.Any();
		
		public GenericObservableList<INamedDomainObject> ObservableEntities { get; protected set; }

		[PropertyChangedAlso(nameof(CanRemove))]
		public INamedDomainObject SelectedEntity
		{
			get => _selectedEntity;
			set => SetField(ref _selectedEntity, value);
		}
		
		public string Info { get; set; }

		protected abstract void AddEntity();
		protected abstract void SaveEntities();
		
		private void InitializeCommands()
		{
			AddEntityCommand = new DelegateCommand(AddEntity);
			RemoveEntityCommand = new DelegateCommand(RemoveEntity, () => CanRemove);
			SaveEntitiesCommand = new DelegateCommand(SaveEntities);
			ShowInfoCommand = new DelegateCommand(ShowInfo);
		}

		private void RemoveEntity()
		{
			if(SelectedEntity is null)
			{
				return;
			}
			
			ObservableEntities.Remove(SelectedEntity);
			OnPropertyChanged(nameof(CanSave));
		}

		private void ShowInfo() => CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, Info);
	}
}
