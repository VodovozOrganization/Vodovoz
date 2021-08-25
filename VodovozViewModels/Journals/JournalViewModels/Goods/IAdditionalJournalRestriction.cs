using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using QS.DomainModel.Entity;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public interface IAdditionalJournalRestriction<T> where T : IDomainObject
	{
		IEnumerable<Expression<Func<T, bool>>> ExternalRestrictions { get; }
	}
}