using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Cash.CashRequest;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class CashlessRequestCommentFileInformationMap : ClassMap<CashlessRequestCommentFileInformation>
	{
		public CashlessRequestCommentFileInformationMap()
		{
			Table("cashless_request_comment_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CashlessRequestCommentId).Column("cashless_request_comment_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
