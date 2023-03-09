using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Specifications.Store.Documents
{
	public class DocumentDriverIdSpecification<TDocument> : ISpecification<TDocument>
		where TDocument : IDocument
	{
		private readonly int? _driverId;

		public DocumentDriverIdSpecification(int? driverId)
		{
			_driverId = driverId;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			//return (doc) => _driverId == null || doc.Id == _driverId;
			throw new NotImplementedException();
		}
	}
}
