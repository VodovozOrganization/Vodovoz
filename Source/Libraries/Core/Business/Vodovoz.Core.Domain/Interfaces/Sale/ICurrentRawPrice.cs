namespace Vodovoz.Core.Domain.Interfaces.Sale
{
	public interface ICurrentRawPrice
	{
		/// <summary>
		/// Текущая цена по прайсу
		/// </summary>
		decimal CurrentRawPrice { get; }
	}
}
