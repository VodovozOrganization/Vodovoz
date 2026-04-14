using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml.Documents;
using Edo.Contracts.Xml.Documents.FormalizedDocuments;
using Edo.Contracts.Xml.Documents.FormalizedDocuments.UPD;
using TaxcomEdoApi.Library.Builders.Format5_03;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Converters.Format5_03;
using TaxcomEdoApi.Library.Parsers;
using ФайлДокументПодписант = Edo.Contracts.Xml.Documents.FormalizedDocuments.UPD.ФайлДокументПодписант;

namespace TaxcomEdoApi.Library.Factories.Format5_03
{
	public class EdoTaxcomDocumentsFactory5_03 : IEdoTaxcomDocumentsFactory5_03
	{
		private const string _productsShipped = "Товары переданы";
		private const string _servicesHaveBeenProvided = "Услуги оказаны в полном объеме";
		private const string _jobResponsibilities = "Должностные обязанности";
		private const string _upd = "Универсальный передаточный документ";
		
		private readonly IErpDocumentInfoConverter5_03 _erpDocumentInfoConverter;
		private readonly IUpdProductConverter5_03 _updProductConverter;
		private readonly ICertificateParser _certificateParser;

		public EdoTaxcomDocumentsFactory5_03(
			IErpDocumentInfoConverter5_03 erpDocumentInfoConverter,
			IUpdProductConverter5_03 updProductConverter,
			ICertificateParser certificateParser)
		{
			_erpDocumentInfoConverter = erpDocumentInfoConverter ?? throw new ArgumentNullException(nameof(erpDocumentInfoConverter));
			_updProductConverter = updProductConverter ?? throw new ArgumentNullException(nameof(updProductConverter));
			_certificateParser = certificateParser ?? throw new ArgumentNullException(nameof(certificateParser));
		}

		#region УПД

		public UniversalTransferDocument5_03 CreateUpdXml5_03(
			UniversalTransferDocumentInfo updInfo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject,
			string certificateThumbprint)
		{
			var org = updInfo.Seller.Organization;
			var updDate = updInfo.Date.ToShortDateString();
			
			var upd = new UniversalTransferDocument5_03
			{
				Format = Format.Format5_03,
				ProgramVersion = "ВерсПрог"
			};

			var hasMarkGoods = updInfo.Products.Any(x => x.TrueMarkCodes.Any());

			var updNameBuilder =
				EdoSellerUpdNameBuilder5_03
					.Create()
					.DocumentId(updInfo.DocumentId.ToString("D").ToUpper())
					.ReceiverId(updInfo.Customer.Organization.EdoAccountId)
					.SenderId(organizationAccountId)
					.Date(updInfo.Date);

			if(hasMarkGoods)
			{
				updNameBuilder.ControlMarkGoods();
			}
				
			upd.Id = updNameBuilder.ToString();
			
			upd.UniversalTransferDocument = new UniversalTransferDocument
			{
				Функция = ФайлДокументФункция.СЧФДОП,
				ПоФактХЖ = "Документ об отгрузке товаров (выполнении работ), передаче имущественных прав (документ об оказании услуг)",
				НаимДокОпр = _upd,
				FiscalDocumentClassifiers = FiscalDocumentClassifiers.KND1115131,
				ДатаИнфПр = DateTime.Now.ToShortDateString(),
				ВремИнфПр = $"{DateTime.Now:HH.mm.ss}",
				НаимЭконСубСост = $"{org.Name}, ИНН/КПП {org.Inn}/{org.Kpp}",
				СвСчФакт = new ФайлДокументСвСчФакт
				{
					НомерДок = updInfo.Number.ToString(),
					ДатаДок = updDate,
					
					ДенИзм = new ФайлДокументСвСчФактДенИзм
					{
						КодОКВ = "643",
						НаимОКВ = "Российский рубль"
					},
					
					//Сведения о продавце
					СвПрод = new[]
					{
						_erpDocumentInfoConverter.ConvertOrganizationToSellerInfo(org)
					},
					
					//Грузоотправитель
					ГрузОт = new[]
					{
						new ФайлДокументСвСчФактГрузОт
						{
							Item = ФайлДокументСвСчФактГрузОтОнЖе.онже
						}
					},
					
					//Сведения о покупателе
					СвПокуп = new[]
					{
						_erpDocumentInfoConverter.ConvertCounterpartyToCustomerInfo(updInfo.Customer)
					},
					
					//Грузополучатель
					ГрузПолуч = new[]
					{
						_erpDocumentInfoConverter.ConvertCounterpartyToConsigneeInfo(updInfo.Consignee)
					}
				}
			};

			var invoiceInfo = upd.UniversalTransferDocument.СвСчФакт;

			//К платежно-расчетному документу
			if(updInfo.Payments.Any())
			{
				invoiceInfo.СвПРД = updInfo.Payments
					.Select(p => new ФайлДокументСвСчФактСвПРД
					{
						НомерПРД = p.PaymentNum,
						ДатаПРД = p.PaymentDate
					})
					.ToArray();
			}

			invoiceInfo.ДокПодтвОтгрНом = new[]
			{
				new РеквДокТип
				{
					РеквНаимДок = _upd,
					РеквНомерДок = updInfo.Number.ToString(),
					РеквДатаДок = updDate
				}
			};

			if(updInfo.AdditionalInformation.Any())
			{
				invoiceInfo.ИнфПолФХЖ1 = new ФайлДокументСвСчФактИнфПолФХЖ1
				{
					ТекстИнф = updInfo.AdditionalInformation.Select(
						x => new TextInformation
							{
								Key = x.Id,
								Value = x.Value
							})
						.ToArray()
				};
			}

			var products = updInfo.Products.Where(x => x.Count > 0).ToList();

			var taxesSum = products.Sum(x => x.IncludeVat);
			upd.UniversalTransferDocument.ТаблСчФакт = new ФайлДокументТаблСчФакт
			{
				СведТов = _updProductConverter.ConvertProductsToUpdProducts(products),
				ВсегоОпл = new ФайлДокументТаблСчФактВсегоОпл
				{
					СтТовБезНДСВсего = products.Sum(x => x.SumWithoutVat),
					СтТовБезНДСВсегоSpecified = true,
					СтТовУчНалВсего = updInfo.Sum,
					СтТовУчНалВсегоSpecified = true,
					СумНалВсего = new СумНДСТип
					{
						Item = taxesSum == 0m ? СумНДСТипБезНДС.безНДС : taxesSum
					}
				}
			};

			var certDetails = _certificateParser.Parse(certificateSubject, certificateThumbprint);
			var firstNameAndPatronymic = certDetails.GivenName.Split(' ');
			var patronymic = firstNameAndPatronymic.Length == 2 ? firstNameAndPatronymic[1] : null;

			upd.UniversalTransferDocument.СвПродПер = new ФайлДокументСвПродПер
			{
				СвПер = new ФайлДокументСвПродПерСвПер
				{
					СодОпер = GetOperationName(products),
					ДатаПер = updDate,

					Items = new[]
					{
						GetDocumentConfirmingShipment(updInfo.DocumentConfirmingShipment)
					},
					
					СвЛицПер = new ФайлДокументСвПродПерСвПерСвЛицПер
					{
						Item = new ФайлДокументСвПродПерСвПерСвЛицПерРабОргПрод
						{
							ФИО = new FullName
							{
								LastName = certDetails.SurName,
								Name = firstNameAndPatronymic[0],
								Patronymic = patronymic
							},
							Должность = warrantOptions.JobPosition
						}
					}
				}
			};
			
			//Уточнить насчет даты подписания документа в случае переотправки в другой день
			upd.UniversalTransferDocument.Подписант = new[]
			{
				new ФайлДокументПодписант
				{
					СпосПодтПолном = ФайлДокументПодписантСпосПодтПолном.Item1,
					ДатаПодДок = updDate,
					Должн = warrantOptions.JobPosition,
					ФИО = new FullName
					{
						LastName = certDetails.SurName,
						Name = firstNameAndPatronymic[0],
						Patronymic = patronymic
					}
				}
			};

			return upd;
		}
		
		private object GetDocumentConfirmingShipment(DocumentConfirmingShipmentInfo documentConfirmingShipmentInfo)
		{
			if(string.IsNullOrWhiteSpace(documentConfirmingShipmentInfo.Number))
			{
				return ФайлДокументСвПродПерСвПерБезДокОснПер.Item1;
			}

			return new РеквДокТип
			{
				РеквНаимДок = documentConfirmingShipmentInfo.Document,
				РеквНомерДок = documentConfirmingShipmentInfo.Number,
				РеквДатаДок = documentConfirmingShipmentInfo.Date
			};
		}
		
		private string GetOperationName(IEnumerable<ProductInfo> products)
		{
			var sb = new StringBuilder();
			
			if(products.Any(x => !x.IsService))
			{
				sb.Append(_productsShipped);
			}

			if(products.Any(x => x.IsService))
			{
				if(sb.Length > 0)
				{
					sb.Append(", ");
					sb.Append(_servicesHaveBeenProvided.ToLower());
				}
				else
				{
					sb.Append(_servicesHaveBeenProvided);
				}
			}
			
			return sb.ToString();
		}
		
		#endregion УПД
	}
}
