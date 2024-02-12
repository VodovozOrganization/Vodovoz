using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum TapType
	{
		[Display(Name = "Нажим кнопкой")]
		ButtonPush,
		[Display(Name = "Нажим кружкой")]
		CupPush,
		[Display(Name = "Нажим рукой")]
		HandPush
	}
}
