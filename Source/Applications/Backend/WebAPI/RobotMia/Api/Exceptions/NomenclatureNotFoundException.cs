using System;

namespace Vodovoz.RobotMia.Api.Exceptions
{
	/// <summary>
	/// Номенклатура не найдена
	/// </summary>
	public class NomenclatureNotFoundException : Exception
	{
		/// <summary>
		/// Конструктор исключения
		/// </summary>
		/// <param name="nomenclatureId">Идентификатор номенклатуры</param>
		public NomenclatureNotFoundException(int nomenclatureId)
		{
			NomenclatureId = nomenclatureId;
		}

		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		public int NomenclatureId { get; }

		/// <summary>
		/// Сообщение исключения
		/// </summary>
		public override string Message => $"Номенклатура #{NomenclatureId} не найдена";
	}
}
