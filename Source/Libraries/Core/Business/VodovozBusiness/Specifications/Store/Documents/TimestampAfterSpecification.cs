using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Specifications.Store.Documents
{
	public class TimestampAfterSpecification<TDocument> : ISpecification<TDocument>
		where TDocument : Document
	{
		private readonly DateTime? _startDate;

		public TimestampAfterSpecification(DateTime? startDate)
		{
			_startDate = startDate;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			return (doc) => _startDate == null || doc.TimeStamp >= _startDate;
		}
	}
}
