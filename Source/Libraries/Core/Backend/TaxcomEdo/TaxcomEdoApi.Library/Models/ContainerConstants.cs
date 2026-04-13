namespace TaxcomEdoApi.Library.Models
{
	public static class WarrantConstants
	{
		public const string InfoXmlNamespace = "http://api-invoice.taxcom.ru/warrant";
		public const string DefaultLink = "https://m4d.nalog.gov.ru/";
		public const string XmlName = "warrant.xml";
		public const string DefaultXmlPath = "WarrantPath";
		public const string ValidFrom = "МЧДДействительнаС";
		public const string ValidTo = "МЧДДействительнаПо";
	}
	
	public static class MetaConstants
	{
		public const string InfoXmlNamespace = "http://api-invoice.taxcom.ru/meta";
		public const string XmlName = "meta.xml";
	}
	
	public static class CardConstants
	{
		public const string DocumentType = "DocumentType";
		public const string LinkedDocument = "LinkedDocument";
		public const string SenderDepartment = "SenderDepartment";
		public const string SenderFullName = "SenderFullName";
		public const string SenderContact = "SenderContact";
		public const string ReceiverDepartment = "ReceiverDepartment";
		public const string ReceiverFullName = "ReceiverFullName";
		public const string ReceiverContact = "ReceiverContact";
		public const string DealNumber = "DealNumber";
		public const string OwnerDepartmentId = "OwnerDepartmentId";
		public const string WarrantMetaId = "WarrantMetaID";
	}
}
