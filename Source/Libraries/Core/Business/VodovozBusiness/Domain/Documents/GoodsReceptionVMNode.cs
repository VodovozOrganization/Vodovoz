using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
	/// <summary>
	/// Представление строк документа разгрузки
	/// </summary>
	public class GoodsReceptionVMNode : PropertyChangedBase
	{
		private decimal _amount;

		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Наименование номенклатуры
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		/// <summary>
		/// Ожидаемое количество
		/// </summary>
		public int ExpectedAmount { get; set; }

		/// <summary>
		/// Категория номенклатуры
		/// </summary>
		public NomenclatureCategory Category { get; set; }

		/// <summary>
		/// Направление
		/// </summary>
		public Direction? Direction { get; set; }

		/// <summary>
		/// Причина направления
		/// </summary>
		public DirectionReason DirectionReason { get; set; }

		/// <summary>
		/// Тип владения
		/// </summary>
		public OwnTypes OwnType { get; set; }
	}
}
