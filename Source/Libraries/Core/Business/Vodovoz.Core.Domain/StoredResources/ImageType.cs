using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.StoredResources
{
	public enum ImageType
	{
		[Display(Name = "Подпись")]
		Signature,
		[Display(Name = "Прочее")]
		Other
	}
}
