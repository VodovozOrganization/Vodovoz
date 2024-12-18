using System;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExcludeElement<TId, T> : IncludeExcludeElement
	{
		public TId Id { get; set; }

		public override string Number => Id.ToString();

		public Type Type => typeof(T);

		public IncludeExcludeElement(bool isRadio = false)
		{
			IsRadio = isRadio;
		}
	}
}
