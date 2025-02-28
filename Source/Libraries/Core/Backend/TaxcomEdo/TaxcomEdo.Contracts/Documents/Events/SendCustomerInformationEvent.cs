using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Обертка для xml файла действия на отправку титула покупателя для подписания входящего документа
	/// </summary>
	[DesignerCategory("code")]
	[XmlRoot("Document", Namespace = "", IsNullable = false)]
	[Serializable]
	public class SendCustomerInformationEvent
	{
		/// <summary>
		/// Id документооборота, который принимается
		/// </summary>
		[XmlAttribute]
		public string InternalId { get; set; }
		/// <summary>
		/// Действие(отправка титула покупателя)
		/// </summary>
		[XmlAttribute]
		public string Action => "SendCustomerInformation";
		/// <summary>
		/// Дополнительные параметры
		/// </summary>
		public AdditionalParameter[] AdditionalData { get; set; }
		/// <summary>
		/// Подписанты
		/// </summary>
		public Signer[] Signers { get; set; }
		
		/// <summary>
		/// Перевод xml в строку
		/// </summary>
		/// <returns></returns>
		public string ToXmlString()
		{
			using(var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
			{
				new XmlSerializer(typeof(SendCustomerInformationEvent))
					.Serialize(stringWriter, this, new XmlSerializerNamespaces(new []
						{
							new XmlQualifiedName(string.Empty)
						}));
				return stringWriter.ToString();
			}
		}
	}
}
