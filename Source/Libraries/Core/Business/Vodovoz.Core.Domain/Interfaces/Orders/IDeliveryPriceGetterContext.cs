namespace Vodovoz.Core.Domain.Interfaces.Orders
{
	/// <summary>
	/// Данные необходимые для расчета стоимости доставки
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDeliveryPriceGetterContext<out T>
	{
		/// <summary>
		/// Данные для расчета стоимости доставки
		/// </summary>
		T Data { get; }
	}
}
