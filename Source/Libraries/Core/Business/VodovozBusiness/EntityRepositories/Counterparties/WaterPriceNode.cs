using System;
using System.ComponentModel.DataAnnotations;
using QS.DocTemplates;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class WaterPriceNode : PatternField
	{
		[Display(Name = "Количество")]
		public string Count { get; set; }

		string stringCount = null;
		
		[Display(Name = "Количество строка")]
		public string StringCount
		{
			get => stringCount ?? String.Format("от {0} шт.", Count);
			set => stringCount = value;
		}
		
		[Display(Name = "Вода1")]
		public string Water1 { get; set; }
		
		[Display(Name = "Идентификатор1")]
		public int Id1 { get; set; }
		
		[Display(Name = "Вода2")]
		public string Water2 { get; set; }
		
		[Display(Name = "Идентификатор2")]
		public int Id2 { get; set; }
		
		[Display(Name = "Вода3")]
		public string Water3 { get; set; }
		
		[Display(Name = "Идентификатор3")]
		public int Id3 { get; set; }
		
		[Display(Name = "Вода4")]
		public string Water4 { get; set; }
		
		[Display(Name = "Идентификатор4")]
		public int Id4 { get; set; }
		
		[Display(Name = "Вода5")]
		public string Water5 { get; set; }
		
		[Display(Name = "Идентификатор5")]
		public int Id5 { get; set; }
	}
}