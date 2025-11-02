using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Settings.Car;

namespace Vodovoz.ViewModels.Widgets.Cars.CarModelSelection
{
	public partial class CarModelSelectionFilterViewModel : WidgetViewModelBase
	{
		private readonly List<CarModel> _carModels;
		private readonly IUnitOfWork _unitOfWork;
		private readonly int[] _firstInSelectionListCarModelIds;
		private string _searchString;
		private bool _isShowArchiveCarModels;
		private IEnumerable<CarTypeOfUse> _selectedCarTypesOfUse;
		private IEnumerable<CarTypeOfUse> _excludedCarTypesOfUse;

		public CarModelSelectionFilterViewModel(IUnitOfWork unitOfWork, ICarSettings carSettings)
		{
			if(carSettings is null)
			{
				throw new ArgumentNullException(nameof(carSettings));
			}

			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

			_firstInSelectionListCarModelIds = carSettings.FirstInSelectionListCarModelId;

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
				OnPropertyChanged(nameof(CarModelNodes));
			}
		}

		public IEnumerable<CarTypeOfUse> SelectedCarTypesOfUse
		{
			get => _selectedCarTypesOfUse ?? new List<CarTypeOfUse>();
			set
			{
				SetField(ref _selectedCarTypesOfUse, value);
				UpdateCarModelNodes();
				OnPropertyChanged(nameof(CarModelNodes));
			}
		}

		public IEnumerable<CarTypeOfUse> ExcludedCarTypesOfUse
		{
			get => _excludedCarTypesOfUse ?? new List<CarTypeOfUse>();
			set
			{
				SetField(ref _excludedCarTypesOfUse, value);
				UpdateCarModelNodes();
				OnPropertyChanged(nameof(CarModelNodes));
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
				OnPropertyChanged(nameof(CarModelNodes));
			}
		}

		public string IncludedExcludesNodesCountInfo =>
			$"Выбрано: {IncludedCarModelNodesCount}\tИсключено: {ExcludedCarModelNodesCount}";

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
				if(SelectedCarTypesOfUse.Contains(carModel.CarTypeOfUse)
					&& !ExcludedCarTypesOfUse.Contains(carModel.CarTypeOfUse))
				{
					var carModelRow = CarModelNodes
						.Where(r => r.ModelId == carModel.Id)
						.FirstOrDefault();

					if(carModelRow == null)
					{
						carModelRow = CarModelSelectableNode.CreateCarModelNode(carModel);
					}

					if(IsShowArchiveCarModels || !carModelRow.IsArchive)
					{
						carModelNodes.Add(carModelRow);
					}
				}
			}

			carModelNodes = carModelNodes
				.OrderBy(n => n.ModelInfo)
				.ToList();

			CarModelNodes.Clear();

			foreach(var item in carModelNodes)
			{
				CarModelNodes.Add(item);
			}

			MoveSpecifiedCarModelsToTop();

			UpdateCarModelsNodesVisibility();

			UpdateIncludedExcludedNodesInfo();
		}

		private void MoveSpecifiedCarModelsToTop()
		{
			var index = _firstInSelectionListCarModelIds.Length;

			while(index > 0)
			{
				index--;

				var carModelNodeToMoveToTop = CarModelNodes
					.Where(c => c.ModelId == _firstInSelectionListCarModelIds[index])
					.FirstOrDefault();

				if(carModelNodeToMoveToTop != null)
				{
					CarModelNodes.Remove(carModelNodeToMoveToTop);
					CarModelNodes.Insert(0, carModelNodeToMoveToTop);
				}
			}
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
	}
}
