namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <summary>
	/// Данные по количеству бутылей бОльшего объема
	/// </summary>
	public interface IMaxVolumeTotalBottles
	{
		/// <summary>
		/// Максимальное количество бОльшего объема бутылей
		/// </summary>
		decimal Max { get; }
		/// <summary>
		/// Текущее количество бОльшего объема бутылей
		/// </summary>
		decimal Current { get; }
	}
}
