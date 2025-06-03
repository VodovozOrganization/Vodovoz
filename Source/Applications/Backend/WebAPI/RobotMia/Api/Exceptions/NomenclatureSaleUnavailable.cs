using System;

namespace Vodovoz.RobotMia.Api.Exceptions
{
	/// <summary>
	/// Номенклатура не может быть добавлена в заказ
	/// </summary>
	public class NomenclatureSaleUnavailableException : Exception
	{
		/// <summary>
		/// Конструктор исключения для номенклатуры, которая не может быть добавлена в заказ
		/// </summary>
		/// <param name="nomenclatureId"></param>
		/// <param name="nomenclatureName"></param>
		public NomenclatureSaleUnavailableException(int nomenclatureId, string nomenclatureName)
		{
			NomenclatureId = nomenclatureId;
			NomenclatureName = nomenclatureName;
		}

		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		public int NomenclatureId { get; }

		/// <summary>
		/// Название номенклатуры
		/// </summary>
		public string NomenclatureName { get; }

		/// <summary>
		/// Сообщение исключения
		/// </summary>
		public override string Message => $"Номенклатура #{NomenclatureId}: \"{NomenclatureName}\" не может быть добавлена. В заказ может быть добавлена либо номенклатура, одобренная для продажи, либо неустойка";
	}
}
