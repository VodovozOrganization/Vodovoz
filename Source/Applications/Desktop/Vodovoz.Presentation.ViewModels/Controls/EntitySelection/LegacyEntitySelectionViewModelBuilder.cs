using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public partial class LegacyEntitySelectionViewModelBuilder<TEntity>
		where TEntity : class, IDomainObject
	{
		#region Обазательные параметры
		protected ILegacyESVMBuilderParameters _parameters;
		protected IPropertyBinder<TEntity> PropertyBinder;
		#endregion

		#region Опциональные компоненты
		protected IEntityDialogSelectionAutocompleteSelector<TEntity> DialogSelectionAndAutocompleteSelector;
		protected IEntityJournalSelector EntityJournalSelector;
		protected IEntitySelectionAdapter<TEntity> EntityAdapter;
		private Func<ITdiTab> _dialogTabFunc;
		private IUnitOfWork _unitOfWork;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly INavigationManager _navigationManager;
		#endregion

		public LegacyEntitySelectionViewModelBuilder(
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public bool IsParametersCreated => !(_parameters is null) && !(PropertyBinder is null);

		#region Инициализация параметров
		public LegacyEntitySelectionViewModelBuilder<TEntity> SetDialogTab(Func<ITdiTab> dialogTabFunc)
		{
			if(_dialogTabFunc != null)
			{
				throw new InvalidOperationException("DialogTabFunc уже установлена");
			}

			_dialogTabFunc = dialogTabFunc ?? throw new ArgumentNullException(nameof(dialogTabFunc));
			TryCreateParameters();

			return this;
		}

		public LegacyEntitySelectionViewModelBuilder<TEntity> SetUnitOfWork(IUnitOfWork unitOfWork)
		{
			if(_unitOfWork != null)
			{
				throw new InvalidOperationException("UnitOfWork уже установлен");
			}

			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			TryCreateParameters();

			return this;
		}

		private void TryCreateParameters()
		{
			if(_dialogTabFunc is null)
			{
				return;
			}

			if(_unitOfWork is null)
			{
				return;
			}

			_parameters = new LegacyESVMBuilderParameters(_dialogTabFunc, _unitOfWork, _navigationManager, _lifetimeScope);
		}
		#endregion

		#region Fluent Config
		public LegacyEntitySelectionViewModelBuilder<TEntity> ForProperty<TBindedEntity>(TBindedEntity bindedEntity, Expression<Func<TBindedEntity, TEntity>> sourceProperty)
			where TBindedEntity : class, INotifyPropertyChanged
		{
			PropertyBinder = new PropertyBinder<TBindedEntity, TEntity>(bindedEntity, sourceProperty);

			return this;
		}

		public virtual LegacyEntitySelectionViewModelBuilder<TEntity> UseViewModelJournalSelector<TJournalViewModel>()
			where TJournalViewModel : JournalViewModelBase
		{
			if(!IsParametersCreated)
			{
				throw new InvalidOperationException("Базовые параметры не установлены");
			}

			EntityJournalSelector = new EntityJournalViewModelSelector<TEntity, TJournalViewModel>(
				_parameters.DialogTabFunc,
				_parameters.NavigationManager);

			return this;
		}

		public virtual LegacyEntitySelectionViewModelBuilder<TEntity> UseViewModelJournalSelector<TJournalViewModel, TJournalFilterViewModel>(Action<TJournalFilterViewModel> filterParams)
			where TJournalViewModel : JournalViewModelBase
			where TJournalFilterViewModel : class, IJournalFilterViewModel
		{
			if(!IsParametersCreated)
			{
				throw new InvalidOperationException("Базовые параметры не установлены");
			}

			EntityJournalSelector = new EntityJournalViewModelSelector<TEntity, TJournalViewModel, TJournalFilterViewModel>(
				_parameters.DialogTabFunc,
				_parameters.NavigationManager,
				filterParams);

			return this;
		}

		public virtual LegacyEntitySelectionViewModelBuilder<TEntity> UseViewModelJournalSelector<TJournalViewModel, TJournalFilterViewModel>(TJournalFilterViewModel filter)
			where TJournalViewModel : JournalViewModelBase
			where TJournalFilterViewModel : class, IJournalFilterViewModel
		{
			if(!IsParametersCreated)
			{
				throw new InvalidOperationException("Базовые параметры не установлены");
			}

			EntityJournalSelector = new EntityJournalViewModelSelector<TEntity, TJournalViewModel, TJournalFilterViewModel>(
				_parameters.DialogTabFunc,
				_parameters.NavigationManager,
				filter);

			return this;
		}

		public virtual LegacyEntitySelectionViewModelBuilder<TEntity> UseSelectionDialogAndAutocompleteSelector(
			Func<IList<int>> entityIdRestrictionFunc = null,
			Func<string, Expression<Func<TEntity, bool>>> entityTitleComparerFunc = null,
			Func<IEnumerable<TEntity>, IEnumerable<TEntity>> resultCollectionProcessingFunc = null,
			Func<SelectionDialogSettings> dialogSettingsFunc = null)
		{
			if(!IsParametersCreated)
			{
				throw new InvalidOperationException("Базовые параметры не установлены");
			}

			DialogSelectionAndAutocompleteSelector = new EntityDialogSelectionAutocompleteSelector<TEntity>(
				_parameters.NavigationManager,
				_parameters.UnitOfWork,
				entityIdRestrictionFunc,
				entityTitleComparerFunc,
				resultCollectionProcessingFunc,
				dialogSettingsFunc);

			return this;
		}

		public virtual LegacyEntitySelectionViewModelBuilder<TEntity> UseAdapter(IEntitySelectionAdapter<TEntity> adapter)
		{
			EntityAdapter = adapter;
			return this;
		}

		public virtual EntitySelectionViewModel<TEntity> Finish()
		{
			if(!IsParametersCreated)
			{
				throw new InvalidOperationException("Базовые параметры не установлены");
			}

			var entityAdapter =
				EntityAdapter ?? new EntitySelectionAdapter<TEntity>(_parameters.UnitOfWork);

			var selectionDialogSelector =
				DialogSelectionAndAutocompleteSelector ?? new EntityDialogSelectionAutocompleteSelector<TEntity>(_parameters.NavigationManager, _parameters.UnitOfWork);

			return new EntitySelectionViewModel<TEntity>(PropertyBinder, selectionDialogSelector, EntityJournalSelector, entityAdapter);
		}
		#endregion
	}
}
