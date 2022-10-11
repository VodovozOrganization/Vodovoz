using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders
{
	public class SelectableNomenclatureCategoryNode : PropertyChangedBase
	{
		private bool _isSelected;

		public bool IsSelected
		{
			get => _isSelected;
			set => SetField(ref _isSelected, value);
		}
		
		public DiscountReasonNomenclatureCategory DiscountReasonNomenclatureCategory { get; set; }
	}
}
