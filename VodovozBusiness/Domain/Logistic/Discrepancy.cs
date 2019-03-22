using System;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic
{
	public class Discrepancy
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public Nomenclature Nomenclature { get; set; }

		/// <summary>
		/// Количество которое необходимо забрать у клиента
		/// </summary>
		/// <value>The picked up from client.</value>
		public decimal PickedUpFromClient { get; set; }

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

		public bool Trackable { get; set; }
		public bool UseFine { get; set; }

		/// <summary>
		/// Остаток
		/// </summary>
		public decimal Remainder => FromWarehouse + ToWarehouse - ClientRejected - PickedUpFromClient;

		/// <summary>
		/// Недовоз
		/// </summary>
		public string Returns => string.Format("{0}", ClientRejected);

		/// <summary>
		/// Серийный номер
		/// </summary>
		public string Serial {
			get {
				if(Trackable)
					return Id > 0 ? Id.ToString() : "(не определен)";
				return string.Empty;
			}
		}

		/// <summary>
		/// Ущерб
		/// </summary>
		public decimal SumOfDamage => Nomenclature == null ? 0 : Nomenclature.SumOfDamage * (-Remainder);
	}
}
