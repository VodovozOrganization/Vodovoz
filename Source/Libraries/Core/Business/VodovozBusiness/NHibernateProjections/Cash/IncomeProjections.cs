using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Cash;
using Vodovoz.Tools;

namespace Vodovoz.NHibernateProjections.Cash
{
	public static class IncomeProjections
	{
		public static IProjection GetTitleProjection()
		{
			Income incomeAlias = null;

			return CustomProjections.Concat_WS(
				"",
				Projections.Constant(typeof(Income).GetClassUserFriendlyName().Nominative),
				Projections.Constant(" №"),
				Projections.Property(() => incomeAlias.Id),
				Projections.Constant(" от "),
				CustomProjections.Date(
					Projections.Property(() => incomeAlias.Date)));
		}
	}
}
