using NHibernate.Criterion;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;

namespace Vodovoz.ViewModels.Widgets
{
	public class SelectableParametersFilterViewModel : WidgetViewModelBase
	{
		private DelegateCommand<bool> _selectUnselectAllCommand;

		public SelectableParametersFilterViewModel(IParametersFactory parametersFactory, string title)
		{
			if(parametersFactory == null)
			{
				throw new ArgumentNullException(nameof(parametersFactory));
			}

			Parameters = new GenericObservableList<SelectableParameter>(
				parametersFactory.GetParameters(new Func<ICriterion>[] { () => null }));
			
			IsRecursiveParameters = parametersFactory.IsRecursiveFactory;
			Title = title;
		}

		public string Title { get; }
		public GenericObservableList<SelectableParameter> Parameters { get; }
		public bool IsRecursiveParameters { get; }
		public bool IsSelectAll { get; private set; }

		public DelegateCommand<bool> SelectUnselectAllCommand =>
			_selectUnselectAllCommand ?? (_selectUnselectAllCommand = new DelegateCommand<bool>(
				isSelectAll =>
				{
					SelectUnselectAll(Parameters, isSelectAll);
					IsSelectAll = isSelectAll;
				}));

		private void SelectUnselectAll(IList<SelectableParameter> parameters, bool isSelectedAll)
		{
			for(int i = 0; i < parameters.Count; i++)
			{
				parameters[i].Selected = isSelectedAll;
			}
		}

		public void SelectParameters<TEntity>(IList<TEntity> parameters)
			where TEntity : class, IDomainObject
		{
			for(int i = 0; i < parameters.Count; i++)
			{
				var parameterId = parameters[i].Id;

				for(int j = 0; j < Parameters.Count; j++)
				{
					var selectableParameter = GetParameterById(parameterId, Parameters[j]);

					if(selectableParameter == null)
					{
						continue;
					}

					selectableParameter.Selected = true;
					break;
				}
			}

			if(Parameters.All(x => x.Selected))
			{
				IsSelectAll = true;
			}
		}

		private SelectableParameter GetParameterById(int parameterId, SelectableParameter parameter)
		{
			if((int)parameter.Value == parameterId)
			{
				return parameter;
			}

			return !parameter.Children.Any()
				? null
				: parameter.Children
					.Select(children => GetParameterById(parameterId, children))
					.FirstOrDefault();
		}
	}
}
