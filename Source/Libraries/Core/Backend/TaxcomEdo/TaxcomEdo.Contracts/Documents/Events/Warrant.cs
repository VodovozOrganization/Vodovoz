using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Доверенность для подписанта
	/// </summary>
	[XmlType(AnonymousType = true)]
	[Serializable]
	public class Warrant
	{
		/// <summary>
		/// Регистрационный номер
		/// </summary>
		[XmlAttribute]
		public string MetaID { get; set; }
	}
}
