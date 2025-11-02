using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class CallTaskFilterViewModel
	{
		public enum SortingDirectionType
		{
			[Display(Name = "От меньшего к большему")]
			FromSmallerToBigger,
			[Display(Name = "От большего к меньшему")]
			FromBiggerToSmaller
		}
	}
}
