using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Тип населенного пункта.
	/// </summary>
	public enum LocalityType
	{
		[Display(Name = "Город", ShortName = "г.")]
		city,
		[Display(Name = "Город", ShortName = "г.")]
		town,
		[Display(Name = "Населенный пункт", ShortName = "н.п.")]
		village,
		[Display(Name = "Дачный поселок", ShortName = "д.п.")]
		allotments,
		[Display(Name = "Деревня", ShortName = "дер.")]
		hamlet,
		[Display(Name = "Ферма", ShortName = "фер.")]
		farm,
		[Display(Name = "Хутор", ShortName = "х.")]
		isolated_dwelling
	}
}
