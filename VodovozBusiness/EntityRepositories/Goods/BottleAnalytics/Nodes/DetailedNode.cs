using System;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class DetailedNode
	{
		public DateTime Date { get; set; }
		public string DateString => Date.ToString("dd.MM");
		public string AuthorOrDriver { get; set; }
		public int DocumentNumber { get; set; }
		public string DocumentName { get; set; }
		public string NomenclatureName { get; set; }
		public int Amount { get; set; }
		public string FineString { get; set; }
		public string Comment { get; set; }
	}
}
