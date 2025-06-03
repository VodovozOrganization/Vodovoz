using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.Orders
{
	public partial class RecomendationsForOrderDualListViewModel
	{
		public class RightNode : PropertyChangedBase
		{
			private decimal _count;

			public int RecomendationId { get; set; }
			public int NomenclatureId { get; set; }
			public string NomenclatureName { get; set; }

			[PropertyChangedAlso(nameof(Sum))]
			public decimal Count
			{
				get => _count;
				set => SetField(ref _count, value);
			}

			public decimal Price { get; set; }
			public decimal Sum => Count * Price;
		}
	}
}
