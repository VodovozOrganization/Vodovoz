namespace Vodovoz.Core.Domain.Interfaces.Sale
{
	/// <summary>
	/// Класс для получения ставки налога
	/// </summary>
	public interface IVatRateProvider
	{
		/// <summary>
		/// Получение актуальной ставки налога(НДС)
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <returns>Ставка налога</returns>
		decimal? GetActualRate(IRecalculateTax saleItem);
	}
}
