namespace TaxcomEdoApi.Library.Models
{
	public class Name
	{
		public Name() => Person = new PersonName();

		public PersonName Person { get; internal set; }

		//[RequiredLength(MinLength = 1, MaxLength = 1000, PropertyName = "\"Наименование\"")]
		public string Organization { get; set; }

		public string Abonent { get; set; }
	}
}
