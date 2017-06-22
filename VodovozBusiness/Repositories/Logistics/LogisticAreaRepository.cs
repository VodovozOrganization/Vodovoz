using System;
using NHibernate.Criterion;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class LogisticAreaRepository
	{
		public static QueryOver<LogisticsArea> ActiveAreaQuery()
		{
			return QueryOver.Of<LogisticsArea>();
		}
	}
}
