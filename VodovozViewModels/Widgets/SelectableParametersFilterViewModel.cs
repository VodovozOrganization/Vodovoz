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

		public SelectableParametersFilterViewModel(IParametersFactory parametersFactory, string title)
		{
			if(parametersFactory == null)
			{
				throw new ArgumentNullException(nameof(parametersFactory));
			}

			Parameters = new GenericObservableList<SelectableParameter>(
				parametersFactory.GetParameters(new Func<ICriterion>[] { () => null }));
			IsRecursiveParameters = parametersFactory.IsRecursiveFactory;

			if(IsRecursiveParameters)
			{
				HighParents = Parameters.Where(x => x.Parent == null).ToList();
			}

			Title = title;
		}

		public string Title { get; }
		public GenericObservableList<SelectableParameter> Parameters { get; }
		public bool IsRecursiveParameters { get; }
		public bool IsSelectAll { get; set; }
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
			where TEntity: class, IDomainObject
		{
			for(int i = 0; i < parameters.Count; i++)
			{
				var needSelectParameter = Parameters.SingleOrDefault(x => (int)x.Value == parameters[i].Id);

				if(needSelectParameter != null)
				{
					needSelectParameter.Selected = true;
				}
			}
		}
	}
}
