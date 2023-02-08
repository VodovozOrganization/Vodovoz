using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic
{
	public class Discrepancy
	{
		public string Name { get; set; }
		public Nomenclature Nomenclature { get; set; }

		/// <summary>
		/// Количество которое необходимо забрать у клиента
		/// </summary>
		/// <value>The picked up from client.</value>
		public decimal PickedUpFromClient { get; set; }

		/// <summary>
		/// Доставлено клиенту
		/// </summary>
		/// <value>The picked up from client.</value>
		public decimal DeliveredToClient { get; set; }

		/// <summary>
		/// Недовезенное количество
		/// </summary>
		/// <value>The client rejected.</value>
		public decimal ClientRejected { get; set; }

		/// <summary>
		/// Выгружено на склад
		/// </summary>
		/// <value>To warehouse.</value>
		public decimal ToWarehouse { get; set; }

		/// <summary>
		/// Погружено на складе
		/// </summary>
		public decimal FromWarehouse { get; set; }

		public bool UseFine { get; set; }

		/// <summary>
		/// Свободные остатки
		/// </summary>
		public decimal FreeBalance { get; set; }

		/// <summary>
		/// Получено от водителей
		/// </summary>
		public decimal TransferedFromDrivers { get; set; }

		/// <summary>
		/// Передано другим водителям
		/// </summary>
		public decimal TransferedToAnotherDrivers { get; set; }

		/// <summary>
		/// Остаток
		/// </summary>
		public decimal Remainder => FreeBalance;

		/// <summary>
		/// Ущерб
		/// </summary>
		public decimal SumOfDamage => Nomenclature == null ? 0 : Nomenclature.SumOfDamage * (-Remainder);
	}
}
