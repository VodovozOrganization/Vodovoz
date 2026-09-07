using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.Specifications.Edo
{
	/// <summary>
	/// Спецификации отбора описаний отклонений документооборота ЭДО
	/// </summary>
	public class EdoDeviationSourceSpecification : ExpressionSpecification<EdoDeviationSource>
	{
		private EdoDeviationSourceSpecification(Expression<Func<EdoDeviationSource, bool>> expression)
			: base(expression)
		{
		}

		/// <summary>
		/// Описания отклонений, по которым выполняется валидация
		/// </summary>
		public static EdoDeviationSourceSpecification CreateActive()
			=> new EdoDeviationSourceSpecification(x => x.IsActive);
	}
}
