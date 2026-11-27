using System;
using System.Collections.Generic;

namespace Vodovoz.Models.Cash
{
	/// <summary>
	/// Строка дерева отчёта ДДС и ДДР
	/// </summary>
	public sealed class CashFlowReportLine
	{
		/// <summary>
		/// Создаёт строку отчёта с вычисленной суммой
		/// </summary>
		/// <param name="title">Название строки</param>
		/// <param name="money">Сумма строки</param>
		/// <param name="children">Дочерние строки</param>
		/// <param name="id">Идентификатор статьи, группы статей или подразделения</param>
		/// <param name="numbering">Номер группы или статьи</param>
		/// <param name="isArticle">Строка финансовой статьи</param>
		public CashFlowReportLine(
			string title,
			decimal money,
			IReadOnlyList<CashFlowReportLine> children = null,
			int? id = null,
			string numbering = null,
			bool isArticle = false)
		{
			Title = title;
			Money = money;
			Children = children ?? Array.Empty<CashFlowReportLine>();
			Id = id;
			Numbering = numbering;
			IsArticle = isArticle;
		}

		/// <summary>
		/// Идентификатор статьи, группы статей или подразделения
		/// </summary>
		public int? Id { get; }

		/// <summary>
		/// Название строки
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// Номер группы или статьи
		/// </summary>
		public string Numbering { get; }

		/// <summary>
		/// Сумма строки
		/// </summary>
		public decimal Money { get; }

		/// <summary>
		/// Дочерние строки
		/// </summary>
		public IReadOnlyList<CashFlowReportLine> Children { get; }

		/// <summary>
		/// Признак строки финансовой статьи
		/// </summary>
		public bool IsArticle { get; }
	}
}
