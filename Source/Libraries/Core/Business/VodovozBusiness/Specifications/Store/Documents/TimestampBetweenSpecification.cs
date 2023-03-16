using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Specifications.Store.Documents
{
	public class TimestampBetweenSpecification<TDocument> : ISpecification<TDocument>
		where TDocument : IDocument
	{
		private readonly DateTime? _startDate;
		private readonly DateTime? _endDate;

		public TimestampBetweenSpecification(DateTime? startDate, DateTime? endDate)
		{
			_startDate = startDate;
			_endDate = endDate;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			return new TimestampAfterSpecification<TDocument>(_startDate)
				.And(new TimestampBeforeEndDateSpecification<TDocument>(_endDate)).IsSatisfiedBy();
		}
	}
}
