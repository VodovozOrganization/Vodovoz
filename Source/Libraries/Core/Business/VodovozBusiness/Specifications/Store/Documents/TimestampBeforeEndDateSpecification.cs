using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Specifications.Store.Documents
{
	public class TimestampBeforeEndDateSpecification<TDocument> : ISpecification<TDocument>
		where TDocument : IDocument
	{
		private readonly DateTime? _endDate;

		public TimestampBeforeEndDateSpecification(DateTime? endDate)
			: base()
		{
			_endDate = endDate;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			return (doc) => _endDate == null || doc.TimeStamp < _endDate;
		}
	}
}
