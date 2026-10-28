using System.Text.Json.Serialization;

namespace Vodovoz.Mindbox.Contracts
{
	/// <summary>
	/// Данные клиента Mindbox
	/// </summary>
	public sealed class MindboxCustomer
	{
		[JsonPropertyName("ids")]
		public MindboxCustomerIds Ids { get; set; }

		[JsonPropertyName("lastName")]
		public string LastName { get; set; }

		[JsonPropertyName("firstName")]
		public string FirstName { get; set; }

		[JsonPropertyName("middleName")]
		public string MiddleName { get; set; }

		[JsonPropertyName("fullName")]
		public string FullName { get; set; }

		[JsonPropertyName("mobilePhone")]
		public string MobilePhone { get; set; }

		[JsonPropertyName("email")]
		public string Email { get; set; }

		[JsonPropertyName("customFields")]
		public MindboxCustomerCustomFields CustomFields { get; set; }
	}
}
