using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.Upd;

namespace Vodovoz.Core.Data.NHibernate.Edo
{
	public class EdoUpdDocumentMap : ClassMap<EdoUpdDocument>
	{
		public EdoUpdDocumentMap()
		{
			Table("edo_upd_documents");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Guid)
				.Column("guid");

			Map(x => x.Number)
				.Column("number");

			Map(x => x.Date)
				.Column("date");

			Map(x => x.Sum)
				.Column("sum");

			Map(x => x.GovContract)
				.Column("gov_contract");

			Component(x => x.Seller, c =>
			{
				c.Map(x => x.Name)
					.Column("seller_name");

				c.Map(x => x.CountryCode)
					.Column("seller_country_code");

				c.Map(x => x.CountryName)
					.Column("seller_country_name");

				c.Map(x => x.Address)
					.Column("seller_address");

				c.Map(x => x.Inn)
					.Column("seller_inn");

				c.Map(x => x.Kpp)
					.Column("seller_kpp");

				c.Map(x => x.Ogrn)
					.Column("seller_ogrn");

				c.Map(x => x.OgrnDate)
					.Column("seller_ogrn_date");

				c.Map(x => x.EdoAccountId)
					.Column("seller_edo_account_id");

				c.Map(x => x.IsWithoutVat)
					.Column("seller_is_without_vat");
			});

			Component(x => x.Shipper, c =>
			{
				c.Map(x => x.Name)
					.Column("shipper_name");

				c.Map(x => x.CountryCode)
					.Column("shipper_country_code");

				c.Map(x => x.CountryName)
					.Column("shipper_country_name");

				c.Map(x => x.Address)
					.Column("shipper_address");

				c.Map(x => x.Inn)
					.Column("shipper_inn");

				c.Map(x => x.Kpp)
					.Column("shipper_kpp");

				c.Map(x => x.Ogrn)
					.Column("shipper_ogrn");

				c.Map(x => x.OgrnDate)
					.Column("shipper_ogrn_date");

				c.Map(x => x.EdoAccountId)
					.Column("shipper_edo_account_id");

				c.Map(x => x.IsWithoutVat)
					.Column("shipper_is_without_vat");
			});

			Component(x => x.Customer, c =>
			{
				c.Map(x => x.Name)
					.Column("customer_name");

				c.Map(x => x.CountryCode)
					.Column("customer_country_code");

				c.Map(x => x.CountryName)
					.Column("customer_country_name");

				c.Map(x => x.Address)
					.Column("customer_address");

				c.Map(x => x.Inn)
					.Column("customer_inn");

				c.Map(x => x.Kpp)
					.Column("customer_kpp");

				c.Map(x => x.Ogrn)
					.Column("customer_ogrn");

				c.Map(x => x.OgrnDate)
					.Column("customer_ogrn_date");

				c.Map(x => x.EdoAccountId)
					.Column("customer_edo_account_id");

				c.Map(x => x.IsWithoutVat)
					.Column("customer_is_without_vat");
			});

			Component(x => x.Consignee, c =>
			{
				c.Map(x => x.Name)
					.Column("consignee_name");

				c.Map(x => x.CountryCode)
					.Column("consignee_country_code");

				c.Map(x => x.CountryName)
					.Column("consignee_country_name");

				c.Map(x => x.Address)
					.Column("consignee_address");

				c.Map(x => x.Inn)
					.Column("consignee_inn");

				c.Map(x => x.Kpp)
					.Column("consignee_kpp");

				c.Map(x => x.Ogrn)
					.Column("consignee_ogrn");

				c.Map(x => x.OgrnDate)
					.Column("consignee_ogrn_date");

				c.Map(x => x.EdoAccountId)
					.Column("consignee_edo_account_id");

				c.Map(x => x.IsWithoutVat)
					.Column("consignee_is_without_vat");
			});

			Map(x => x.SpecialConsignee)
				.Column("special_consignee");

			Map(x => x.ShippingDocument)
				.Column("shipping_document");

			Map(x => x.ShippingDocumentDate)
				.Column("shipping_document_date");

			Map(x => x.ShippingDocumentNumber)
				.Column("shipping_document_number");

			Map(x => x.ShippingReasonDocument)
				.Column("shipping_reason_document");

			Map(x => x.ShippingReasonDocumentDate)
				.Column("shipping_reason_document_date");

			Map(x => x.ShippingReasonDocumentNumber)
				.Column("shipping_reason_document_number");

			Map(x => x.CustomerExternalOrderId)
				.Column("customer_external_order_id");

			Map(x => x.ReasonForLeaving)
				.Column("reason_for_leaving");

			Map(x => x.OrderId)
				.Column("order_id");

			HasMany(x => x.Payments)
				.Cascade.AllDeleteOrphan()
				.KeyColumn("edo_upd_document_id");

			HasMany(x => x.InventPositions)
				.Cascade.AllDeleteOrphan()
				.KeyColumn("edo_upd_document_id");
		}
	}
}
