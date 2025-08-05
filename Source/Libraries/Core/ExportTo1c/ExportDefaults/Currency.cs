using QS.DomainModel.Entity;

namespace ExportTo1c.Library.ExportDefaults
{
	/// <summary>
	/// Валюта
	/// </summary>
	public class Currency : IDomainObject
	{
		public int Id { get; set; }
		public int ExportId { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }

		public static Currency Default { get; private set; }

		static Currency()
		{
			Default = new Currency
			{
				Id = 643,
				ExportId = 643,
				Name = "руб.",
				FullName = "Российский рубль"
			};
		}
	}
}
