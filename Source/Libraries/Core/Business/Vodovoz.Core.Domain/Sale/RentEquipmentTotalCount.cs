namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Общее количество арендных позиций в оборудовании
	/// </summary>
	public class RentEquipmentTotalCount
	{
		private RentEquipmentTotalCount(int serviceItemCount, int depositItemCount)
		{
			ServiceItemCount = serviceItemCount;
			DepositItemCount = depositItemCount;
		}
		
		/// <summary>
		/// Количество сервисных 
		/// </summary>
		public int ServiceItemCount { get; }
		/// <summary>
		/// Количество залогов
		/// </summary>
		public int DepositItemCount { get; }

		public static RentEquipmentTotalCount Create(int serviceItemCount, int depositItemCount) =>
			new RentEquipmentTotalCount(serviceItemCount, depositItemCount);
	}
}
