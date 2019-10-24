using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using System.Linq.Expressions;

namespace Vodovoz.JournalViewModels
{
	public interface IAdditionalJournalRestriction<T> where T : IDomainObject
	{
		IEnumerable<Expression<Func<T, bool>>> ExternalRestrictions { get; }
	}
}