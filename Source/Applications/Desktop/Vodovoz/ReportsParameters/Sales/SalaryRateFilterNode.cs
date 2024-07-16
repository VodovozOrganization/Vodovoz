using QS.DomainModel.Entity;

namespace Vodovoz.ReportsParameters.Sales
{
	public class SalaryRateFilterNode : PropertyChangedBase
	{
		private bool _selected;

		public bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}

		public int WageId { get; set; }
		public string Name { get; set; }
	}
}
