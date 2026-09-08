using System;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo.Deviations
{
	/// <summary>
	/// Строка активной проблемы задачи ЭДО
	/// </summary>
	internal class ProblemRow
	{
		/// <summary>
		/// Идентификатор задачи ЭДО, по которой заведена проблема
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Время регистрации проблемы
		/// </summary>
		public DateTime CreationTime { get; set; }
	}
}
