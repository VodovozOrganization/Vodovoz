namespace Vodovoz.Core.Domain.Interfaces.Sale
{
	public interface ISaleItemTaxHandler
	{
		/// <summary>
		/// Расчет налогов для позиции
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		void CalculateTax(IRecalculateTax saleItem);

		/// <summary>
		/// Пересчитываем налоги
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		void RecalculateTaxSum(IRecalculateTax saleItem);
	}
}
