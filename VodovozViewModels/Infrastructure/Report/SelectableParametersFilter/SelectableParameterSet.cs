using System;
using QS.DomainModel.Entity;
using System.Collections.Generic;
namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParameterSet : PropertyChangedBase
	{
		private SelectableFilterType filterType;
		public virtual SelectableFilterType FilterType {
			get => filterType;
			set => SetField(ref filterType, value);
		}

		private IList<SelectableParameter> parameters;
		public virtual IList<SelectableParameter> Parameters {
			get => parameters;
			set => SetField(ref parameters, value);
		}

		public string ParameterName { get; }

		public SelectableParameterSet(IList<SelectableParameter> parameters, string parameterName)
		{
			if(string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			Parameters = parameters;
			ParameterName = parameterName;
		}

		public void SelectAll()
		{
			foreach(var value in Parameters) {
				value.Selected = true;
			}
		}

		public void UnselectAll()
		{
			foreach(var value in Parameters) {
				value.Selected = false;
			}
		}
	}
}
