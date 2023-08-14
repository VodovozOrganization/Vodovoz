using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Cash;
using Vodovoz.Tools;

namespace Vodovoz.NHibernateProjections.Cash
{
	public static class ExpenseProjections
	{
		public static IProjection GetTitleProjection()
		{
			Expense expenseAlias = null;

			return CustomProjections.Concat_WS(
				"",
				Projections.Constant(typeof(Expense).GetClassUserFriendlyName().Nominative),
				Projections.Constant(" №"),
				Projections.Property(() => expenseAlias.Id),
				Projections.Constant(" от "),
				CustomProjections.Date(
					Projections.Property(() => expenseAlias.Date)));
		}
	}
}
