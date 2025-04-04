namespace Vodovoz.ViewModels.TrueMark.CodesPool
{
	public partial class CodesPoolViewModel
	{
		/// <summary>
		/// Строка с данным по количеству кодов
		/// </summary>
		public class CodesPoolDataNode
		{
			/// <summary>
			/// Значение Gtin
			/// </summary>
			public string Gtin { get; set; }

			/// <summary>
			/// Количество кодов в пуле
			/// </summary>
			public int CountInPool { get; set; }

			/// <summary>
			/// Количество кодов, необходимое для добавления в уже реализованные заказы
			/// </summary>
			public int MissingCodesInOrdersCount { get; set; }

			/// <summary>
			/// Количество проданных товаров за предыдущий день
			/// </summary>
			public int SoldYesterday { get; set; }

			/// <summary>
			/// Наименования номенклатур
			/// </summary>
			public string Nomenclatures { get; set; }

			/// <summary>
			/// Количество кодов в пуле недостаточно
			/// </summary>
			public bool IsNotEnoughCodes => CountInPool < SoldYesterday;
		}
	}
}
