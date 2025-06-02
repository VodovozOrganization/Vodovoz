using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class DocumentPrinterSettingMap : ClassMap<DocumentPrinterSetting>
	{
		public DocumentPrinterSettingMap()
		{
			Table("document_printer_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DocumentType).Column("document_type");
			Map(x => x.PrinterName).Column("printer_name");
			Map(x => x.NumberOfCopies).Column("number_of_copies");

			References(x => x.UserSettings).Column("user_settings_id");
		}
	}
}
