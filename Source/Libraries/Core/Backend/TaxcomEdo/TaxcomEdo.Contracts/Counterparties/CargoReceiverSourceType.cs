namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Способ заполнения грузополучателя
	/// </summary>
	public enum CargoReceiverSourceType
	{
		/// <summary>
		/// Из контрагента
		/// </summary>
		FromCounterparty,
		/// <summary>
		/// Из точки доставки
		/// </summary>
		FromDeliveryPoint,
		/// <summary>
		/// Особый
		/// </summary>
		Special
	}
}
