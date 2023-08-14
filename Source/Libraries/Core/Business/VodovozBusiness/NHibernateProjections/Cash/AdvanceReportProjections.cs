using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Cash;
using Vodovoz.Tools;

namespace Vodovoz.NHibernateProjections.Cash
{
	public static class AdvanceReportProjections
	{
		public static IProjection GetTitleProjection()
		{
			AdvanceReport advanceReportAlias = null;

			return CustomProjections.Concat_WS(
				"",
				Projections.Constant(typeof(AdvanceReport).GetClassUserFriendlyName().Nominative),
				Projections.Constant(" №"),
				Projections.Property(() => advanceReportAlias.Id),
				Projections.Constant(" от "),
				CustomProjections.Date(
					Projections.Property(() => advanceReportAlias.Date)));
		}
	}
}
