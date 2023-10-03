using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Widgets.Cars.CarModelSelection
{
	public partial class CarModelSelectionFilterViewModel
	{
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
