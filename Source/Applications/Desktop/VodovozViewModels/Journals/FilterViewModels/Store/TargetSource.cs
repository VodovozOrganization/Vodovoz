using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public enum TargetSource
	{
		[Display(Name = "Источник")]
		Source,
		[Display(Name = "Получатель")]
		Target,
		[Display(Name = "Оба")]
		Both
	}
}
