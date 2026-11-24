using System.Collections.Generic;
using Vodovoz.Core.Data.Repositories;

namespace Vodovoz.ViewModels.TrueMark
{
	/// <summary>
	/// Тип узла дерева истории кода маркировки.
	/// </summary>
	public enum TrueMarkCodeHistoryNodeType
	{
		/// <summary>
		/// Группа истории.
		/// </summary>
		Group,

		/// <summary>
		/// Экземплярный код ЧЗ.
		/// </summary>
		Code,

		/// <summary>
		/// Агрегация (групповой или транспортный код).
		/// </summary>
		Aggregation,

		/// <summary>
		/// Код товара ЧЗ.
		/// </summary>
		ProductCode,

		/// <summary>
		/// Задача ЭДО.
		/// </summary>
		Task,

		/// <summary>
		/// Заказ.
		/// </summary>
		Order,

		/// <summary>
		/// Документооборот.
		/// </summary>
		DocumentFlow,

		/// <summary>
		/// Документ, в котором использован код (МЛ, талон погрузки, самовывоз).
		/// </summary>
		SourceDocument
	}

	/// <summary>
	/// Узел дерева истории кода маркировки в ДВ.
	/// </summary>
	public class TrueMarkCodeHistoryNode
	{
		public TrueMarkCodeHistoryNode Parent { get; set; }

		public IList<TrueMarkCodeHistoryNode> Children { get; set; } = new List<TrueMarkCodeHistoryNode>();

		/// <summary>
		/// Наименование события.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Подробности события.
		/// </summary>
		public string Details { get; set; }

		/// <summary>
		/// Дата события.
		/// </summary>
		public string Date { get; set; }

		public TrueMarkCodeHistoryNodeType NodeType { get; set; }

		/// <summary>
		/// Номер связанного заказа.
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// Номер связанной задачи ЭДО.
		/// </summary>
		public int? TaskId { get; set; }

		/// <summary>
		/// Номер документа, в котором использован код.
		/// </summary>
		public int? SourceDocumentId { get; set; }

		/// <summary>
		/// Тип документа, в котором использован код.
		/// </summary>
		public TrueMarkCodeHistoryProductCodeDocumentType SourceDocumentType { get; set; }

		/// <summary>
		/// Код маркировки для копирования.
		/// </summary>
		public string Code { get; set; }
	}

	/// <summary>
	/// Узел дерева текущей информации из ЧЗ.
	/// </summary>
	public class TrueMarkCodeInfoNode
	{
		public TrueMarkCodeInfoNode Parent { get; set; }

		public IList<TrueMarkCodeInfoNode> Children { get; set; } = new List<TrueMarkCodeInfoNode>();

		/// <summary>
		/// Наименование параметра.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Значение параметра.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Код маркировки, которому соответствует узел.
		/// </summary>
		public string Code { get; set; }
	}
}
