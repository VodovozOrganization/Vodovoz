using System;
using System.Collections.Generic;
using QS.ViewModels;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using System.Data.Bindings.Collections.Generic;
using NHibernate.Criterion;
using QS.Commands;
using System.Linq;
using QS.DomainModel.Entity;
namespace Vodovoz.ViewModels.Widgets
{
	public class SelectableParametersFilterViewModel : WidgetViewModelBase
	{
		private DelegateCommand<bool> _selectUnselectAllCommand;

		public SelectableParametersFilterViewModel(IParametersFactory parametersFactory)
		{
			if(parametersFactory == null)
			{
				throw new ArgumentNullException(nameof(parametersFactory));
			}

			Parameters = new GenericObservableList<SelectableParameter>(
				parametersFactory.GetParameters(new Func<ICriterion>[] { () => { return null; } }));
			IsRecursiveParameters = parametersFactory.IsRecursiveFactory;

			if(IsRecursiveParameters)
			{
				HighParents = Parameters.Where(x => x.Parent == null).ToList();
			}
		}

		public IList<SelectableParameter> Parameters { get; }
		public bool IsRecursiveParameters { get; }
		public IList<SelectableParameter> HighParents { get; }

		public DelegateCommand<bool> SelectUnselectAllCommand => 
			_selectUnselectAllCommand ?? (_selectUnselectAllCommand = new DelegateCommand<bool>(
				isSelectAll =>
				{
					if(IsRecursiveParameters)
					{
						SelectUnselectAll(HighParents, isSelectAll);
					}
					else
					{
						SelectUnselectAll(Parameters, isSelectAll);
					}
				}));

		private void SelectUnselectAll(IList<SelectableParameter> parameters, bool isSelectedAll)
		{
			for(int i = 0; i < parameters.Count; i++)
			{
				parameters[i].Selected = isSelectedAll;
			}
		}

		public void SelectParameters(IList<IDomainObject> parameters)
		{
			for(int i = 0; i < Parameters.Count; i++)
			{
				if(Parameters[i] is IDomainObject parameter && parameters.Contains(parameter))
				{
					Parameters[i].Selected = true;
				}
			}
		}
	}
}
