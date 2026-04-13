namespace TaxcomEdoApi.Library.Models
{
	public class PersonSigner
	{
		//[Required("\"Имя подписанта\"")]
		//[RequiredLength(MinLength = 1, MaxLength = 60, PropertyName = "\"Имя подписанта\"")]
		public string Name { get; set; }

		//[Required("\"Фамилия подписанта\"")]
		//[RequiredLength(MinLength = 1, MaxLength = 60, PropertyName = "\"Фамилия подписанта\"")]
		public string LastName { get; set; }

		//[RequiredLength(MinLength = 1, MaxLength = 60, PropertyName = "\"Отчество подписанта\"")]
		public string Patronymic { get; set; }

		//[Required("Поле \"Должность подписанта\" обязательно для заполнения")]
		//[RequiredLength(MinLength = 1, MaxLength = 1000, PropertyName = "\"Должность подписанта\"")]
		public string JobPosition { get; set; }
	}
}
