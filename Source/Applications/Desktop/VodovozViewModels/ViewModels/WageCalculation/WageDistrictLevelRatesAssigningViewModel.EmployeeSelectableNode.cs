using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.ViewModels.WageCalculation
{
	public partial class WageDistrictLevelRatesAssigningViewModel
	{
		public class EmployeeSelectableNode : PropertyChangedBase
		{
			private bool _isSelected;

			public int Id { get; set; }
			public string LastName { get; set; }
			public string Name { get; set; }
			public string Patronymic { get; set; }

			public bool IsSelected
			{
				get => _isSelected;
				set => SetField(ref _isSelected, value);
			}

			public string FullName =>
				LastName + " " + Name + (string.IsNullOrWhiteSpace(Patronymic) ? "" : " " + Patronymic);
		}
	}
}
