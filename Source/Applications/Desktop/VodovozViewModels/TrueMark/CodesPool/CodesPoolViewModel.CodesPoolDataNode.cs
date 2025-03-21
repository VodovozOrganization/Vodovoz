namespace Vodovoz.ViewModels.TrueMark.CodesPool
{
	public partial class CodesPoolViewModel
	{
		public class CodesPoolDataNode
		{
			public string Gtin { get; set; }
			public int CountInPool { get; set; }
			public int SoldYesterday { get; set; }
			public string Nomenclatures { get; set; }
			public bool IsNotEnoughCodes => CountInPool < SoldYesterday;
		}
	}
}
