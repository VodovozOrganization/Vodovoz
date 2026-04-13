namespace TaxcomEdo.Contracts.Xml.Container
{
	/// <summary>
	/// Направление документооборота
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/meta")]
	public enum DocFlowDirection
	{
		/// <summary>
		/// Входящий
		/// </summary>
		Incoming,
		/// <summary>
		/// Исходящий
		/// </summary>
		Outgoing,
	}
}
