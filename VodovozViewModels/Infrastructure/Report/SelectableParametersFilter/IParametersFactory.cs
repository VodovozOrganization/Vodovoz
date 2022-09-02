using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public interface IParametersFactory
	{
		bool IsRecursiveFactory { get; }
		IList<SelectableParameter> GetParameters(IEnumerable<Func<ICriterion>> filterRelations);
	}

	public interface IParametersFactory<TEntity> : IParametersFactory
		where TEntity: class, IDomainObject
	{
		IList<ISelectableParameter<TEntity>> GetEntityParameters(IEnumerable<Func<ICriterion>> filterRelations);
	}

	public interface ISelectableParameter
	{
		bool Selected { get; set; }
		string Title { get; }
		object Entity { get; }
		SelectableParameter Parent { get; set; }
		GenericObservableList<SelectableParameter> Children { get; }
	}
	
	public interface ISelectableParameter<out TEntity> : ISelectableParameter
		where TEntity: class, IDomainObject
	{
		TEntity Entity { get; }
	}
}
