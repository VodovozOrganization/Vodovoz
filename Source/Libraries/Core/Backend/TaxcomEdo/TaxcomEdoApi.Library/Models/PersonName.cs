namespace TaxcomEdoApi.Library.Models
{
	public class PersonName
	{
		//[Required("\"Имя\"")]
		//[RequiredLength(MinLength = 1, MaxLength = 60, PropertyName = "Имя")]
		public string FirstName { get; set; }

		//[Required("\"Фамилия\"")]
		//[RequiredLength(MinLength = 1, MaxLength = 60, PropertyName = "\"Фамилия\"")]
		public string LastName { get; set; }

		//[RequiredLength(MinLength = 1, MaxLength = 60, PropertyName = "\"Отчество \"")]
		public string MiddleName { get; set; }

		public string ToStringIndividualBusinessmanName()
		{
			return !string.IsNullOrWhiteSpace(this.LastName) || !string.IsNullOrWhiteSpace(this.FirstName) || !string.IsNullOrWhiteSpace(this.MiddleName) ? $"Индивидуальный предприниматель {this.LastName} {this.FirstName} {this.MiddleName}".Trim() : (string) null;
		}
	}
}
