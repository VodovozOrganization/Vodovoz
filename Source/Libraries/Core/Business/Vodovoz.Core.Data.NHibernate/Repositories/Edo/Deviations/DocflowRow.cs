using System;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка документооборота, заведенного у провайдера ЭДО по исходящему документу
	/// </summary>
	internal class DocflowRow
	{
		/// <summary>
		/// Код документооборота
		/// </summary>
		public int DocflowId { get; set; }

		/// <summary>
		/// Код исходящего документа, по которому заведен документооборот
		/// </summary>
		public int DocumentId { get; set; }

		/// <summary>
		/// Время создания документооборота
		/// </summary>
		public DateTime CreationTime { get; set; }
	}
}
