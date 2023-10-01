using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.CarModelSelection
{
	public class CarModelSelectionFilterViewModel : WidgetViewModelBase
	{
		private readonly List<CarModel> _carModels;
		private readonly IUnitOfWork _unitOfWork;

		private string _searchString;
		private bool _isShowArchiveCarModels;
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;

		public CarModelSelectionFilterViewModel(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

			_carModels = GetAllCarModels();

			CarModelNodes = new GenericObservableList<CarModelSelectableNode>();
			CarModelNodes.PropertyOfElementChanged += OnCarModelNodesPropertyOfElementChanged; ;

			UpdateCarModelNodes();

			ClearSearchStringCommand = new DelegateCommand(ClearSearchString);
			ClearAllExcludesCommand = new DelegateCommand(ClearAllExcludedNodes);
			ClearAllIncludesCommand = new DelegateCommand(ClearAllIncludedNodes);
		}

		#region Properties

		public string SearchString
		{
			get => _searchString;
			set
			{
				SetField(ref _searchString, value);
				UpdateCarModelsNodesVisibility();
			}
		}

		public IEnumerable<CarTypeOfUse> SelectedCarTypesOfUse
		{
			get => _selectedCarTypesOfUse ?? new List<CarTypeOfUse>();
			set
			{
				SetField(ref _selectedCarTypesOfUse, value);
				UpdateCarModelNodes();
			}
		}

		public GenericObservableList<CarModelSelectableNode> CarModelNodes
		{
			get;
			private set;
		}

		public bool IsShowArchiveCarModels
		{
			get => _isShowArchiveCarModels;
			set
			{
				SetField(ref _isShowArchiveCarModels, value);
				UpdateCarModelNodes();
			}
		}

		public string IncludedExcludesNodesCountInfo =>
			$"Выбрано: {IncludedCarModelNodesCount}\nИсключено: {ExcludedCarModelNodesCount}";

		public int IncludedCarModelNodesCount =>
			CarModelNodes
				.Where(n => n.IsIncluded)
				.Count();

		public int ExcludedCarModelNodesCount =>
			CarModelNodes
				.Where(n => n.IsExcluded)
				.Count();

		public int[] IncludedCarModelIds =>
			CarModelNodes
				.Where(n => n.IsIncluded)
				.Select(n => n.ModelId)
			.ToArray() ?? new int[0];

		public int[] ExcludedCarModelIds =>
			CarModelNodes
				.Where(n => n.IsExcluded)
				.Select(n => n.ModelId)
			.ToArray() ?? new int[0];

		public DelegateCommand ClearSearchStringCommand { get; }

		public DelegateCommand ClearAllExcludesCommand { get; }

		public DelegateCommand ClearAllIncludesCommand { get; }

		#endregion Properties

		private List<CarModel> GetAllCarModels()
		{
			var carModels = _unitOfWork.GetAll<CarModel>().ToList();

			return carModels;
		}

		public void UpdateCarModelNodes()
		{
			List<CarModelSelectableNode> carModelNodes = new List<CarModelSelectableNode>();

			foreach(var carModel in _carModels)
			{
				if(SelectedCarTypesOfUse.Contains(carModel.CarTypeOfUse))
				{
					var carModelRow = CarModelNodes
						.Where(r => r.ModelId == carModel.Id)
						.FirstOrDefault();

					if(carModelRow == null)
					{
						carModelRow = CarModelSelectableNode.CreateCarModelNode(carModel);
					}

					carModelNodes.Add(carModelRow);
				}
			}

			carModelNodes = carModelNodes
				.OrderBy(n => n.ModelInfo)
				.ToList();

			CarModelNodes.Clear();

			foreach(var item in carModelNodes)
			{
				if(item.ModelId == 3)
				{
					CarModelNodes.Insert(0, item);
					continue;
				}

				CarModelNodes.Add(item);
			}

			UpdateCarModelsNodesVisibility();
		}

		private void UpdateCarModelsNodesVisibility()
		{
			foreach(var carModelNode in CarModelNodes)
			{
				var isModelInfoMatchesSearchString =
					string.IsNullOrWhiteSpace(SearchString)
					|| IsModelInfoContainsSearchStringCheck(carModelNode);

				var isModelInfoMatchesShowArchiveRequirement =
					IsShowArchiveCarModels
					|| !carModelNode.IsArchive;

				carModelNode.IsVisible =
					isModelInfoMatchesSearchString
					&& isModelInfoMatchesShowArchiveRequirement;
			}

			UpdateIncludedExcludedNodesInfo();
		}

		public bool IsModelInfoContainsSearchStringCheck(CarModelSelectableNode node) =>
			node.ModelInfo.ToLower().Contains(SearchString.Trim().ToLower());

		private void UpdateIncludedExcludedNodesInfo()
		{
			OnPropertyChanged(nameof(IncludedExcludesNodesCountInfo));

			OnPropertyChanged(nameof(IncludedCarModelNodesCount));
			OnPropertyChanged(nameof(ExcludedCarModelNodesCount));

			OnPropertyChanged(nameof(IncludedCarModelIds));
			OnPropertyChanged(nameof(ExcludedCarModelIds));
		}

		private void ClearSearchString()
		{
			SearchString = string.Empty;

			OnPropertyChanged(nameof(CarModelNodes));
		}

		private void ClearAllExcludedNodes()
		{
			foreach(var node in CarModelNodes)
			{
				node.IsExcluded = false;
			}

			OnPropertyChanged(nameof(CarModelNodes));
		}

		private void ClearAllIncludedNodes()
		{
			foreach(var node in CarModelNodes)
			{
				node.IsIncluded = false;
			}

			OnPropertyChanged(nameof(CarModelNodes));
		}

		private void OnCarModelNodesPropertyOfElementChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateIncludedExcludedNodesInfo();
		}

		public class CarModelSelectableNode : PropertyChangedBase
		{
			private int _modelId;
			private string _modelInfo;
			private bool _isArchive;
			private bool _isIncluded;
			private bool _isExcluded;
			private bool _isVisible;

			public int ModelId
			{
				get => _modelId;
				set => SetField(ref _modelId, value);
			}

			public string ModelInfo
			{
				get => _modelInfo;
				set => SetField(ref _modelInfo, value);
			}

			public bool IsArchive
			{
				get => _isArchive;
				set => SetField(ref _isArchive, value);
			}

			public bool IsIncluded
			{
				get => _isIncluded;
				set
				{
					if(_isIncluded != value)
					{
						SetField(ref _isIncluded, value);

						if(_isIncluded)
						{
							IsExcluded = false;
						}
					}
				}
			}

			public bool IsExcluded
			{
				get => _isExcluded;
				set
				{
					if(_isExcluded != value)
					{
						SetField(ref _isExcluded, value);

						if(_isExcluded)
						{
							IsIncluded = false;
						}
					}
				}
			}

			public bool IsVisible
			{
				get => _isVisible;
				set => SetField(ref _isVisible, value);
			}

			public static CarModelSelectableNode CreateCarModelNode(CarModel carModel)
			{
				var carModelNode = new CarModelSelectableNode
				{
					ModelId = carModel.Id,
					ModelInfo = $"{carModel.CarManufacturer.Name} {carModel.Name}",
					IsArchive = carModel.IsArchive,
					IsIncluded = false,
					IsExcluded = false,
					IsVisible = true
				};

				return carModelNode;
			}
		}
	}
}
