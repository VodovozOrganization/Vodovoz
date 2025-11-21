using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain;

namespace Vodovoz.Data.NHibernate.HibernateMapping
{
	public class CertificateMap : ClassMap<CertificateEntity>
	{
		public CertificateMap()
		{
			Table("certificates");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.TypeOfCertificate).Column("certificate_type");
			Map(x => x.ImageFile).Column("image_file").LazyLoad();
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.ExpirationDate).Column("expiration_date");
			HasManyToMany(x => x.Nomenclatures)
								.Table("certificates_to_entities")
								.ParentKeyColumn("certificate_id")
								.ChildKeyColumn("nomenclature_id")
								.LazyLoad();
		}
	}
}
