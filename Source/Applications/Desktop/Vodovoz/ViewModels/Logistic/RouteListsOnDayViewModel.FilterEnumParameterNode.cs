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

			public FilterEnumParameterNode(TEnum value)
			{
				Value = value;
			}

			public virtual bool IsSelected
			{
				get => _isSelected;
				set => SetField(ref _isSelected, value);
			}

			public TEnum Value { get; }
			public string Title => Value.GetEnumDisplayName();
		}
	}
}
