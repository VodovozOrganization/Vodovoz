using Autofac;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Settings.Delivery;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Goods
{
	public class AdditionalLoadingSettingsViewModel : UowDialogViewModelBase, IAskSaveOnCloseViewModel
	{
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IInteractiveService _interactiveService;

		private DelegateCommand<IList<AdditionalLoadingNomenclatureDistribution>> _removeNomenclatureDistributionCommand;
		private DelegateCommand _addNomenclatureDistributionCommand;
		private DelegateCommand _showFlyerInfoCommand;

		private int _bottlesCount;
		private double _fastDeliveryMaxDistance;
		private bool _flyerAdditionEnabled;
		private bool _flyerAdditionForNewClientsEnabled;
		private bool _flyerForNewCounterpartyEnabled;
		private int _flyerForNewCounterpartyBottlesCount;
		private int _maxFastOrdersPerSpecificTime;

		public AdditionalLoadingSettingsViewModel(
			ILifetimeScope scope,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ICommonServices commonServices,
			IDeliveryRulesSettings deliveryRulesSettings,
			IDeliveryRepository deliveryRepository
			)
			: base(unitOfWorkFactory, navigation)
		{
			if(scope == null)
			{
				throw new ArgumentNullException(nameof(scope));
			}
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}
			_deliveryRulesSettings = deliveryRulesSettings ??
				throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_interactiveService = commonServices.InteractiveService;

			CanEdit = commonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(AdditionalLoadingNomenclatureDistribution)).CanUpdate;
			
			FastDeliveryMaxDistance = _deliveryRepository.GetGetMaxDistanceToLatestTrackPointKm();
			MaxFastOrdersPerSpecificTime = _deliveryRulesSettings.MaxFastOrdersPerSpecificTime;

			FlyerAdditionEnabled = _deliveryRulesSettings.AdditionalLoadingFlyerAdditionEnabled;
			BottlesCount = _deliveryRulesSettings.BottlesCountForFlyer;

			FlyerForNewCounterpartyEnabled = _deliveryRulesSettings.FlyerForNewCounterpartyEnabled;
			FlyerForNewCounterpartyBottlesCount = _deliveryRulesSettings.FlyerForNewCounterpartyBottlesCount;

			Initialize();
		}

		public override string Title => "Настройка запаса и радиуса";

		public GenericObservableList<AdditionalLoadingNomenclatureDistribution> ObservableNomenclatureDistributions { get; private set; }

		public decimal PercentSum => ObservableNomenclatureDistributions.Sum(x => x.Percent);

		public bool CanEdit { get; }

		public int BottlesCount
		{
			get => _bottlesCount;
			set => SetField(ref _bottlesCount, value);
		}

		public double FastDeliveryMaxDistance
		{
			get => _fastDeliveryMaxDistance;
			set => SetField(ref _fastDeliveryMaxDistance, value);
		}

		public int MaxFastOrdersPerSpecificTime
		{
			get => _maxFastOrdersPerSpecificTime;
			set => SetField(ref _maxFastOrdersPerSpecificTime, value);
		}

		public bool FlyerAdditionEnabled
		{
			get => _flyerAdditionEnabled;
			set => SetField(ref _flyerAdditionEnabled, value);
		}

		public bool FlyerForNewCounterpartyEnabled
		{ 
			get => _flyerForNewCounterpartyEnabled; 
			set => SetField(ref _flyerForNewCounterpartyEnabled, value);
		}

		public int FlyerForNewCounterpartyBottlesCount
		{
			get => _flyerForNewCounterpartyBottlesCount;
			set => SetField(ref _flyerForNewCounterpartyBottlesCount, value);
		}

		public DelegateCommand<IList<AdditionalLoadingNomenclatureDistribution>> RemoveNomenclatureDistributionCommand =>
			_removeNomenclatureDistributionCommand
			?? (_removeNomenclatureDistributionCommand = new DelegateCommand<IList<AdditionalLoadingNomenclatureDistribution>>(
				distributions =>
				{
					foreach(var distribution in distributions)
					{
						UoW.Delete(distribution);
						ObservableNomenclatureDistributions.Remove(distribution);
					}
				},
				distributions => distributions != null && distributions.Any()));

		public DelegateCommand AddNomenclatureDistributionCommand => _addNomenclatureDistributionCommand
			?? (_addNomenclatureDistributionCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
						this,
						OpenPageOptions.AsSlave
					);
					page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
					page.ViewModel.OnSelectResult += OnNomenclaturesSelected;
				}));

		public DelegateCommand ShowFlyerInfoCommand => _showFlyerInfoCommand
			?? (_showFlyerInfoCommand = new DelegateCommand(
				() =>
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Info,
						"Если установлена галочка 'Добавлять в запас листовки...'\n" +
						"при добавлении запаса в МЛ также будут добавляться\n" +
						"активные листовки каждого типа, который есть на остатках,\n" +
						"в количестве, которое рассчитывается по формуле:\n\n" +
						"Кол-во всех добавленных 19л бут. в запасе / указанное кол-во бутылей для 1 листовки.\n" +
						"Остаток отбрасывается.\n\n" +
						"Т.е если в МЛ добавилось 100 19л бутылей в запас,\n" +
						"а в настройках запаса был указан расчёт 1 листовка на 2 бутыли,\n" +
						"то в МЛ добавится 50 листовок каждого типа (если они есть на остатках)"
					);
				}));

		private void OnNomenclaturesSelected(object sender, JournalSelectedEventArgs e)
		{
			var nodesToAdd = e.SelectedObjects
				.Cast<NomenclatureJournalNode>()
				.Where(selectedNode => ObservableNomenclatureDistributions.All(x => x.Nomenclature.Id != selectedNode.Id))
				.ToList();

			var notValidWeightOrVolume = new List<Nomenclature>();
			var notValidCategory = new List<Nomenclature>();
			foreach(var nomenclature in UoW.GetById<Nomenclature>(nodesToAdd.Select(x => x.Id)))
			{
				if(nomenclature.Weight == 0 || nomenclature.Volume == 0)
				{
					notValidWeightOrVolume.Add(nomenclature);
				}
				else if(!Nomenclature.CategoriesWithWeightAndVolume.Contains(nomenclature.Category))
				{
					notValidCategory.Add(nomenclature);
				}
				else
				{
					ObservableNomenclatureDistributions.Add(new AdditionalLoadingNomenclatureDistribution
					{
						Nomenclature = nomenclature,
						Percent = 1
					});
				}
			}

			if(notValidWeightOrVolume.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Можно добавлять только номенклатуры с заполненным объёмом и весом.\n" +
					"Номенклатуры, которые не были добавлены:\n\n" +
					$"{string.Join("\n", notValidWeightOrVolume.Select(x => $"{x.Id} {x.Name}"))}");
			}
			else if(notValidCategory.Any())
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Можно добавлять только номенклатуры с категорией из списка: " +
					$"{string.Join(", ", Nomenclature.CategoriesWithWeightAndVolume.Select(x => $"<b>{x.GetEnumTitle()}</b>"))}.\n" +
					"Номенклатуры, которые не были добавлены:\n\n" +
					$"{string.Join("\n", notValidCategory.Select(x => $"{x.Id} {x.Category.GetEnumTitle()} {x.Name}"))}");
			}
		}

		private void Initialize()
		{
			var distributions = UoW.GetAll<AdditionalLoadingNomenclatureDistribution>()
				.OrderByDescending(x => x.Percent)
				.ToList();
			ObservableNomenclatureDistributions = new GenericObservableList<AdditionalLoadingNomenclatureDistribution>(distributions);
			ObservableNomenclatureDistributions.ElementAdded += OnElementAdded;
			ObservableNomenclatureDistributions.ElementRemoved += OnElementRemoved;
			ObservableNomenclatureDistributions.ElementChanged += OnElementChanged;
		}

		private void OnElementAdded(object list, int[] idx)
		{
			OnPropertyChanged(nameof(PercentSum));
		}

		private void OnElementRemoved(object list, int[] idx, object o)
		{
			OnPropertyChanged(nameof(PercentSum));
		}

		private void OnElementChanged(object list, int[] idx)
		{
			OnPropertyChanged(nameof(PercentSum));
		}

		protected override bool Validate()
		{
			if(ObservableNomenclatureDistributions.Sum(x => x.Percent) != 100m)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Сумма процентов распределения номенклатур запаса не равняется 100");
				return false;
			}

			if(BottlesCount < 1)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Кол-во 19л бут. для расчёта добавления листовок не может быть меньше 1");
				return false;
			}

			if(MaxFastOrdersPerSpecificTime < 1)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Кол-во заказов с доставкой за час не может быть меньше 1");
				return false;
			}

			return base.Validate();
		}

		public override bool Save()
		{
			if(!Validate())
			{
				return false;
			}
			foreach(var priority in ObservableNomenclatureDistributions)
			{
				UoW.Save(priority);
			}

			_deliveryRepository.UpdateFastDeliveryMaxDistanceParameter(FastDeliveryMaxDistance);
			UpdateFastDeliveryMaxDistanceValueInAllNotClosedRouteLists(FastDeliveryMaxDistance);

			_deliveryRulesSettings.UpdateAdditionalLoadingFlyerAdditionEnabledParameter(FlyerAdditionEnabled.ToString());
			_deliveryRulesSettings.UpdateBottlesCountForFlyerParameter(BottlesCount.ToString());

			_deliveryRulesSettings.UpdateFlyerForNewCounterpartyEnabledParameter(FlyerForNewCounterpartyEnabled.ToString());
			_deliveryRulesSettings.UpdateFlyerForNewCounterpartyBottlesCountParameter(FlyerForNewCounterpartyBottlesCount.ToString());

			_deliveryRulesSettings.UpdateMaxFastOrdersPerSpecificTimeParameter(MaxFastOrdersPerSpecificTime.ToString());
			UpdateMaxFastDeliveryOrdersValueInAllNotClosedRouteLists(MaxFastOrdersPerSpecificTime);

			UoW.Commit();
			return true;
		}

		private void UpdateFastDeliveryMaxDistanceValueInAllNotClosedRouteLists(double fastDeliveryMaxDistance)
		{
			var distanceToSet = (decimal)fastDeliveryMaxDistance;
			var notClosedRouteLists = UoW.GetAll<RouteList>().Where(r => r.Status != RouteListStatus.Closed).ToList();
			foreach(var routeList in notClosedRouteLists)
			{
				routeList.UpdateFastDeliveryMaxDistanceValue(distanceToSet);
			}
		}
		private void UpdateMaxFastDeliveryOrdersValueInAllNotClosedRouteLists(int maxFastDeliveryOrders)
		{
			var notClosedFastDeliveryRouteLists = UoW.GetAll<RouteList>()
				.Where(r => r.Status != RouteListStatus.Closed 
				            && r.AdditionalLoadingDocument != null)
				.ToList();

			foreach(var routeList in notClosedFastDeliveryRouteLists)
			{
				routeList.UpdateMaxFastDeliveryOrdersValue(maxFastDeliveryOrders);
			}
		}

		public override void Dispose()
		{
			ObservableNomenclatureDistributions.ElementAdded -= OnElementAdded;
			ObservableNomenclatureDistributions.ElementRemoved -= OnElementRemoved;
			ObservableNomenclatureDistributions.ElementChanged -= OnElementChanged;
			base.Dispose();
		}

		public bool AskSaveOnClose => CanEdit;
	}
}
