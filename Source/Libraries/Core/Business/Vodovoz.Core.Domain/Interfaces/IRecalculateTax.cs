namespace Vodovoz.Core.Domain.Interfaces
{
	public interface IRecalculateTax
	{
		IRecalculateTaxSource RecalculateTaxSource { get; }
		IDepositNomenclature Nomenclature { get; }
		decimal? IncludeNDS { get; set; }
		decimal ActualSum { get; }
		decimal? ValueAddedTax { get; set; }
	}
}
