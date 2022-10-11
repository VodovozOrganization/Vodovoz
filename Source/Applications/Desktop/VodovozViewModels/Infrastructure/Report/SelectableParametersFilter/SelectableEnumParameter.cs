using System;
using Gamma.Utilities;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableEnumParameter<TEnum> : SelectableParameter
	{
		public override string Title {
			get {
				Enum enumV = (enumValue as Enum);
				if(enumV == null) {
					throw new InvalidOperationException($"Необходимо указать тип Enum, указанный тип: {typeof(TEnum).FullName}");
				}
				string title = enumV.GetEnumTitle();
				return title;
			}
		}

		public override Func<object> ValueFunc => () => enumValue;

		private readonly TEnum enumValue;

		public SelectableEnumParameter(TEnum enumValue)
		{
			this.enumValue = enumValue;
		}
	}
}
