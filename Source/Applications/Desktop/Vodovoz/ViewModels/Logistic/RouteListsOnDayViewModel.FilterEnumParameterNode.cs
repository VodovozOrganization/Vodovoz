using QS.DomainModel.Entity;
using System;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Logistic
{
	public partial class RouteListsOnDayViewModel
	{
		public class FilterEnumParameterNode<TEnum> : PropertyChangedBase
			where TEnum: Enum
		{
			private bool _isSelected;

			public FilterEnumParameterNode(TEnum parameter)
			{
				Parameter = parameter;
			}

			public virtual bool IsSelected
			{
				get => _isSelected;
				set => SetField(ref _isSelected, value);
			}

			public TEnum Parameter { get; }
			public string Title => Parameter.GetEnumDisplayName();
		}
	}
}
