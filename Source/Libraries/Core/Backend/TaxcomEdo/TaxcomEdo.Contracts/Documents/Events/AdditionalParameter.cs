using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Доп параметр в форме ключ - значение
	/// </summary>
	[Serializable]
	public class AdditionalParameter
	{
		/// <summary>
		/// Название параметра
		/// </summary>
		[XmlAttribute]
		public string Name { get; set; }
		/// <summary>
		/// Значение
		/// </summary>
		[XmlAttribute]
		public string Value { get; set; }
	}
}
