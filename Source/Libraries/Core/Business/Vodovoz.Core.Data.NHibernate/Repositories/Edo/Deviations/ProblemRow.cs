using System;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка активной проблемы задачи ЭДО.
	/// Читается мониторингом отклонений, чтобы понять, с какого момента проблема висит
	/// </summary>
	internal class ProblemRow
	{
		/// <summary>
		/// Код задачи ЭДО, по которой заведена проблема
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Время регистрации проблемы
		/// </summary>
		public DateTime CreationTime { get; set; }
	}
}
