using System.Xml.Serialization;

namespace TaxcomEdo.Api.Dto
{
	[Serializable]
	[XmlRoot(ElementName = "State")]
	public class ContactState
	{
		[XmlAttribute(AttributeName = "Code")]
		public ContactStateCode Code { get; set; }

		[XmlIgnore]
		public ContactError? ErrorCode { get; set; }

		[XmlAttribute(AttributeName = "ErrorCode")]
		public string ErrorCodeAsString
		{
			get
			{
				if(ErrorCode.HasValue)
				{
					return ErrorCode.ToString();
				}

				return string.Empty;
			}
			set
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				if(!string.IsNullOrEmpty(ErrorCodeAsString) && Enum.TryParse<ContactError>(value, ignoreCase: true, out ContactError result))
				{
					ErrorCode = result;
				}
			}
		}

		[XmlText]
		public string Description { get; set; }

		[XmlAttribute(AttributeName = "Changed")]
		public DateTime Changed { get; set; }
	}
}
