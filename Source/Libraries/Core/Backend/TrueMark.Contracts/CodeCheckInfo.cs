using System;
using System.Collections.Generic;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Информация по коду после проверки в ЧЗ
	/// </summary>
	public class CodeCheckInfo
	{
		/// <summary>
		/// Код маркировки из запроса
		/// </summary>
		public string Cis { get; set; }

		/// <summary>
		/// Результат проверки валидности структуры КМ
		/// </summary>
		public bool Valid { get; set; }

		/// <summary>
		/// КМ без крипто-подписи
		/// </summary>
		public string PrintView { get; set; }

		/// <summary>
		/// Код товара
		/// </summary>
		public string Gtin { get; set; }

		/// <summary>
		/// Массив идентификаторов товарных групп
		/// </summary>
		public IEnumerable<int> GroupIds { get; set; }

		/// <summary>
		/// Результат проверки криптоподписи КМ
		/// </summary>
		public bool Verified { get; set; }

		/// <summary>
		/// Признак наличия кода
		/// </summary>
		public bool Found { get; set; }

		/// <summary>
		/// Признак ввода в оборот
		/// </summary>
		public bool Realizable { get; set; }

		/// <summary>
		/// Признак нанесения КИ на упаковку
		/// </summary>
		public bool Utilised { get; set; }

		/// <summary>
		/// Признак того, что розничная продажа продукции заблокирована по решению ОГВ
		/// </summary>
		public bool IsBlocked { get; set; }

		/// <summary>
		/// Дата и время истечения срока годности
		/// </summary>
		public DateTime ExpireDate { get; set; }

		/// <summary>
		/// Дата производства продукции
		/// </summary>
		public DateTime ProductionDate { get; set; }

		/// <summary>
		/// Признак, определяющий, что запрос направлен владельцем кода (определяется по аутентификационному токену)
		/// </summary>
		public bool IsOwner { get; set; }

		/// <summary>
		/// Код ошибки
		/// </summary>
		public int ErrorCode { get; set; }

		/// <summary>
		/// Признак контроля прослеживаемости в товарной группе
		/// </summary>
		public bool IsTracking { get; set; }

		/// <summary>
		/// Признак вывода из оборота или множественных продаж товара
		/// </summary>
		public bool Sold { get; set; }

		/// <summary>
		/// Тип упаковки
		/// </summary>
		public string PackageType { get; set; }

		/// <summary>
		/// ИНН производителя
		/// </summary>
		public string ProducerInn { get; set; }

		/// <summary>
		/// Признак принадлежности табачной продукции к «серой зоне»
		/// </summary>
		public bool GrayZone { get; set; }
	}
}
