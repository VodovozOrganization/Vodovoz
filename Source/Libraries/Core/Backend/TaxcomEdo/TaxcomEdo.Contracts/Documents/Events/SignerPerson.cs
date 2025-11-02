using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Данные подписанта
	/// </summary>
	[XmlType(AnonymousType = true)]
	[Serializable]
	public class SignerPerson
	{
		/// <summary>
		/// Фамилия
		/// </summary>
		[XmlAttribute]
		public string LastName { get; set; }
		/// <summary>
		/// Имя
		/// </summary>
		[XmlAttribute]
		public string FirstName { get; set; }
		/// <summary>
		/// Отчество
		/// </summary>
		[XmlAttribute]
		public string Patronymic { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		[XmlAttribute]
		public string Inn { get; set; }
		/// <summary>
		/// Статус
		/// Опционально. При отсутствии берется значение <see cref="AdditionalParameter"/> <c>"Подписант.ОблПолн"</c>
		/// </summary>
		[XmlAttribute]
		public string SignerPersonStatus { get; set; }
		/// <summary>
		/// Область полномочий
		/// Опционально. При отсутствии берется значение <see cref="AdditionalParameter"/> <c>"Подписант.Статус"</c>
		/// </summary>
		[XmlAttribute]
		public string AreaOfAuthority { get; set; }
		/// <summary>
		/// Основание полномочий (доверия)
		/// Опционально. При отсутствии берется значение <see cref="AdditionalParameter"/> <c>"Подписант.ОснПолн"</c>
		/// </summary>
		[XmlAttribute]
		public string ReasonTheAuthority { get; set; }
	}
}
