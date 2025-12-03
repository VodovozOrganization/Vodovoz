namespace Vodovoz.Settings.Nomenclature
{
	public interface IRecomendationSettings
	{
		/// <summary>
		/// Количество рекомендованных товаров при заказе через робота Миа
		/// </summary>
		int RobotCount { get; }
		/// <summary>
		/// Количество рекомендованных товаров при заказе через оператора (ДВ)
		/// </summary>
		int OperatorCount { get; }
		/// <summary>
		/// Количество рекомендованных товаров при заказе через ИПЗ
		/// </summary>
		int IpzCount { get; }

		/// <summary>
		/// Установить количество рекомендованных товаров при заказе через робота Миа
		/// </summary>
		/// <param name="count">Количество</param>
		void SetRobotCount(int count);
		/// <summary>
		/// Установить количество рекомендованных товаров при заказе через оператора (ДВ)
		/// </summary>
		/// <param name="count">Количество</param>
		void SetOperatorCount(int count);
		/// <summary>
		/// Установить количество рекомендованных товаров при заказе через ИПЗ
		/// </summary>
		/// <param name="count">Количество</param>
		void SetIpzCount(int count);
	}
}
