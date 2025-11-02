using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Flyers
{
	public class FlyerViewModel : EntityTabViewModelBase<Flyer>
	{
		private readonly IFlyerRepository _flyerRepository;
		private ILifetimeScope _lifetimeScope;
		private DateTime? _flyerStartDate;
		private DateTime? _flyerEndDate;
		
		private DelegateCommand _activateFlyerCommand;
		private DelegateCommand _deactivateFlyerCommand;
		private FlyerActionTime _currentFlyerActionTime;

		public FlyerViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFlyerRepository flyerRepository,
			INavigationManager navigationManager) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));

			if(!uowBuilder.IsNewEntity)
			{
				TabName = $"{Entity.FlyerNomenclature.Name}";
			}
			
			SetCurrentFlyerActionTime();
			AddServiceToValidationContext();

			NomenclatureViewModel = new CommonEEVMBuilderFactory<Flyer>(this, Entity, UoW, navigationManager, _lifetimeScope)
				.ForProperty(x => x.FlyerNomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.additional;
					filter.RestrictArchive = false;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
		}

		public DateTime? FlyerStartDate
		{
			get => _flyerStartDate;
			set
			{
				if(SetField(ref _flyerStartDate, value))
				{
					OnPropertyChanged(nameof(CanActivateFlyer));
				}
			}
		}
		
		public DateTime? FlyerEndDate
		{
			get => _flyerEndDate;
			set
			{
				if(SetField(ref _flyerEndDate, value))
				{
					OnPropertyChanged(nameof(CanDeactivateFlyer));
				}
			}
		}

		public bool CanEditFlyerNomenclature => Entity.Id == 0;
		public bool IsFlyerActivated => _currentFlyerActionTime != null && !_currentFlyerActionTime.EndDate.HasValue;
		public bool CanActivateFlyer => FlyerStartDate.HasValue;
		public bool CanDeactivateFlyer => FlyerEndDate.HasValue;
		
		public IEntityEntryViewModel NomenclatureViewModel { get; }

		public DelegateCommand ActivateFlyerCommand => _activateFlyerCommand ?? (_activateFlyerCommand = new DelegateCommand(
			() =>
			{
				if(FlyerStartDate.HasValue && FlyerStartDate <= DateTime.Today)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Дата старта не может быть раньше, либо равной сегодняшнему дню");
					
					return;
				}
				
				if(FlyerStartDate.HasValue && FlyerStartDate < _currentFlyerActionTime?.EndDate)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Дата старта не может быть раньше предыдущей даты окончания");
					
					return;
				}

				ActivateFlyer();
				SetCurrentFlyerActionTime();
				OnPropertyChanged(nameof(IsFlyerActivated));
			}
		));

		public DelegateCommand DeactivateFlyerCommand => _deactivateFlyerCommand ?? (_deactivateFlyerCommand = new DelegateCommand(
			() =>
			{
				if(FlyerEndDate.HasValue && FlyerEndDate < DateTime.Today)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Дата окончания не может быть раньше сегодняшнего дня");
					
					return;
				}
				
				if(FlyerEndDate.HasValue && FlyerEndDate <= _currentFlyerActionTime?.StartDate)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Дата окончания не может быть меньше или равна дате старта");
					
					return;
				}

				DeactivateFlyer();
				OnPropertyChanged(nameof(IsFlyerActivated));
			}
		));

		private void SetCurrentFlyerActionTime()
		{
			var flyerActionTime = Entity.FlyerActionTimes.LastOrDefault();
			_currentFlyerActionTime = flyerActionTime;
		}

		private void ActivateFlyer()
		{
			var flyerActionTime = new FlyerActionTime
			{
				Flyer = Entity,
				StartDate = FlyerStartDate.Value
			};

			Entity.ObservableFlyerActionTimes.Add(flyerActionTime);
		}
		
		private void DeactivateFlyer()
		{
			if(_currentFlyerActionTime != null)
			{
				_currentFlyerActionTime.EndDate = FlyerEndDate;
			}
		}
		
		private void AddServiceToValidationContext()
		{
			ValidationContext.ServiceContainer.AddService(typeof(IFlyerRepository), _flyerRepository);
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
