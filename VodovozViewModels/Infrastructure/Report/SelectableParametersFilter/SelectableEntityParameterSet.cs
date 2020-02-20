using System;
using QS.DomainModel.Entity;
using System.Linq.Expressions;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableEntityParameterSet<TEntity> : SelectableParameterSet
		where TEntity : class, IDomainObject
	{
		private readonly IParametersEntityFactory<TEntity> parametersFactory;

		public override object[] EmptyValue { get; set; } = new object[] { 0 };

		public SelectableEntityParameterSet(string name, IParametersEntityFactory<TEntity> parametersFactory, string parameterName, string includeSuffix = "_include", string excludeSuffix = "_exclude")
			: base(name, parametersFactory, parameterName, includeSuffix, excludeSuffix)
		{
			this.parametersFactory = parametersFactory ?? throw new ArgumentNullException(nameof(parametersFactory));
		}

		public void FilterOnSourceSelectionChanged(Expression<Func<TEntity, object>> filterExpression, SelectableParameterSet sourceParameterSet)
		{
			if(filterExpression == null) {
				throw new ArgumentNullException(nameof(filterExpression));
			}

			if(sourceParameterSet == null) {
				throw new ArgumentNullException(nameof(sourceParameterSet));
			}

			sourceParameterSet.SelectionChanged += (sender, e) => {
				Parameters = new GenericObservableList<SelectableParameter>(parametersFactory.GetParameters(filterExpression, sourceParameterSet));
			};
		}
	}
}
