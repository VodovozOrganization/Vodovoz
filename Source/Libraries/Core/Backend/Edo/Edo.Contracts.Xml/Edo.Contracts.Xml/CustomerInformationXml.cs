using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml
{
	// Пока все в одном файле. При дальнейшей реализации все будет раскидано по классам
	[GeneratedCode("xsd", "2.0.50727.3038")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("Файл", Namespace = "", IsNullable = false)]
	[Serializable]
	public class CustomerInformationTitleDocumentXml
	{
		/// <summary>
		/// Документ об отгрузке товаров (выполнении работ),
		/// передаче имущественных прав (документ об оказании услуг),
		/// включающий счет-фактуру (информация покупателя),
		/// или документ об отгрузке товаров (выполнении работ),
		/// передаче имущественных прав (документ об оказании услуг) (информация покупателя)
		/// </summary>
		[XmlElement("Документ")]
		public CustomerInformation CustomerInformation { get; set; }
	
		/// <summary>
		/// <para>
		/// Идентификатор файла
		/// </para>
		/// Содержит (повторяет) имя сформированного файла (без расширения)
		/// </summary>
		[XmlAttribute("ИдФайл")]
		public string Id { get; set; }
	
		/// <summary>
		/// Версия формата
		/// </summary>
		[XmlAttribute("ВерсФорм")]
		public Format Format { get; set; }
	
		/// <summary>
		/// Версия программы, с помощью которой сформирован файл
		/// </summary>
		[XmlAttribute("ВерсПрог")]
		public string ProgramVersion { get; set; }
	}

	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокумент")]
	[Serializable]
	public class CustomerInformation
	{
		/// <summary>
		/// Код документа по КНД (классификаторы налоговых документов)
		/// </summary>
		[XmlAttribute("КНД")]
		public FiscalDocumentClassifiers FiscalDocumentClassifier { get; set; } = FiscalDocumentClassifiers.KND1115132;
		
		/// <summary>
		/// Дата формирования файла обмена информации покупателя
		/// в формате <c>ДД.ММ.ГГГГ</c>
		/// </summary>
		[XmlAttribute("ДатаИнфПок")]
		public string DocumentDate { get; set; }
		
		/// <summary>
		/// Время формирования файла обмена информации покупателя
		/// в формате <c>ЧЧ.ММ.СС</c>
		/// </summary>
		[XmlAttribute("ВремИнфПок")]
		public string DocumentTime { get; set; }
		
		/// <summary>
		/// Наименование экономического субъекта – составителя файла обмена информации покупателя
		/// </summary>
		[XmlAttribute("НаимЭконСубСост")]
		public string Customer { get; set; }
		
		/// <summary>
		/// Идентификация файла обмена счета-фактуры (информации продавца) или файла обмена информации продавца
		/// </summary>
		[XmlElement("ИдИнфПрод")]
		public DataSellerDocumentId DataSellerDocumentId { get; set; }
		
		/// <summary>
		/// Содержание факта хозяйственной жизни 4 - сведения о принятии товаров (результатов выполненных работ),
		/// имущественных прав (о подтверждении факта оказания услуг)
		/// </summary>
		[XmlElement("СодФХЖ4")]
		public ContentOfFactOfEconomicLife4 ContentOfFactOfEconomicLife4 { get; set; }
		
		/// <summary>
		/// Информация покупателя об обстоятельствах закупок для государственных и муниципальных нужд
		/// (для учета Федеральным казначейством денежных обязательств)
		/// </summary>
		[XmlElement("ИнфПокЗаГоскКазн")]
		public InformationOfPurchasesForStateNeeds InformationOfPurchasesForStateNeeds { get; set; }
		
		/// <summary>
		/// Сведения о лице, подписывающем файл обмена информации покупателя в электронной форме
		/// </summary>
		[XmlElement("Подписант")]
		public CustomerInformationSigner5_03[] Signers { get; set; }
		
		/// <summary>
		/// Основание, по которому экономический субъект является составителем файла обмена информации покупателя
		/// </summary>
		[XmlElement("ОснДоверОргСост")]
		public DocumentDetails Basis { get; set; }
	}
	
	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументПодписант")]
	[Serializable]
	public class CustomerInformationSigner5_03
	{
		/// <summary>
		/// Должность
		/// </summary>
		[XmlAttribute("Должн")]
		public string JobPosition { get; set; }

		/// <summary>
		/// Тип подписи
		/// </summary>
		[XmlAttribute("ТипПодпис")]
		public CustomerInformationSignatureType SignType { get; set; }
		
		public bool ShouldSerializeSignType()
		{
			return SignType != CustomerInformationSignatureType.Default;
		}
		
		/// <summary>
		/// Дата подписания документа
		/// в формате <c>ДД.ММ.ГГГГ</c>
		/// </summary>
		[XmlAttribute("ДатаПодДок")]
		public string SigningDate { get; set; }

		/// <summary>
		/// Способ подтверждения полномочий представителя на подписание документа
		/// </summary>
		[XmlAttribute("СпосПодтПолном")]
		public MethodOfVerifyingCredentials MethodOfVerifyingCredentials { get; set; }
		
		/// <summary>
		/// Дополнительные сведения о подписанте
		/// </summary>
		[XmlAttribute("ДопСведПодп")]
		public string AdditionalInformation { get; set; }
		
		/// <summary>
		/// Фамилия, имя, отчество (при наличии)
		/// </summary>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }
		
		/// <summary>
		/// <para>
		/// Сведения о доверенности в электронной форме в машиночитаемом виде <c>"СвДоверЭл"</c>,
		/// используемой для подтверждения полномочий представителя <see cref="ElectronicPowerOfAuthority"/>
		/// Элемент обязателен и формируется только при <c>"СпосПодтПолном"</c> = 3 
		/// </para>
		/// <para>
		/// Сведения о доверенности в форме документа на бумажном носителе,
		/// используемой для подтверждения полномочий представителя <see cref="PaperPowerOfAuthority"/>
		/// Элемент обязателен и формируется только при <c>"СпосПодтПолном"</c> = 5
		/// </para>
		/// </summary>
		[XmlElement("СвДоверЭл", typeof(ElectronicPowerOfAuthority))]
		[XmlElement("СвДоверБум", typeof(PaperPowerOfAuthority))]
		public object[] Items { get; set; }
	}
	
	/// <summary>
	/// Сведения о доверенности в электронной форме в машиночитаемом виде
	/// </summary>
	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументПодписантСвДоверЭл")]
	[Serializable]
	public class ElectronicPowerOfAuthority
	{
		/// <summary>
		/// Единый регистрационный номер доверенности
		/// </summary>
		[XmlAttribute("НомДовер")]
		public Guid Number { get; set; }
		
		/// <summary>
		/// Дата совершения (выдачи) доверенности
		/// в формате <c>ДД.ММ.ГГГГ</c>
		/// </summary>
		[XmlAttribute("ДатаВыдДовер")]
		public string IssueDate { get; set; }

		/// <summary>
		/// Внутренний номер доверенности
		/// При отсутствии номера принимает значение: без номера (б/н)
		/// </summary>
		[XmlAttribute("ВнНомДовер")]
		public string InnerNumber { get; set; }

		/// <summary>
		/// Дата внутренней регистрации доверенности
		/// в формате <c>ДД.ММ.ГГГГ</c>
		/// </summary>
		[XmlAttribute("ДатаВнРегДовер")]
		public string InnerRegistrationDate { get; set; }
		
		/// <summary>
		/// Идентифицирующая информация об информационной системе, в которой осуществляется хранение доверенности,
		/// необходимая для запроса информации из информационной системы
		/// </summary>
		[XmlAttribute("ИдСистХран")]
		public string InformationSystemId  { get; set; }
		
		/// <summary>
		/// Сведения в формате URL об информационной системе,
		/// которая предоставляет техническую возможность получения информации о доверенности
		/// </summary>
		[XmlAttribute("УРЛСист")]
		public string UrlInformationSystem  { get; set; }
	}
	
	/// <summary>
	/// Сведения о доверенности в форме документа на бумажном носителе
	/// </summary>
	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументПодписантСвДоверБум")]
	[Serializable]
	public class PaperPowerOfAuthority
	{
		/// <summary>
		/// Фамилия, имя, отчество (при наличии) лица, подписавшего доверенность
		/// </summary>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }

		/// <summary>
		/// Дата совершения (выдачи) доверенности
		/// в формате <c>ДД.ММ.ГГГГ</c>
		/// </summary>
		[XmlAttribute("ДатаВыдДовер")]
		public string IssueDate { get; set; }

		/// <summary>
		/// Внутренний номер доверенности
		/// При отсутствии номера принимает значение: без номера (б/н)
		/// </summary>
		[XmlAttribute("ВнНомДовер")]
		public string InnerNumber { get; set; }

		/// <summary>
		/// Сведения, идентифицирующие доверителя
		/// </summary>
		[XmlAttribute("СвИдДовер")]
		public string PrincipalId { get; set; }
	}
	
	/// <summary>
	/// Способ подтверждения полномочий представителя на подписание документа
	/// </summary>
	[GeneratedCode("xsd", "4.7.2558.0")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументПодписантСпосПодтПолном")]
	[Serializable]
	public enum MethodOfVerifyingCredentials
	{
		/// <summary>
		/// В соответствии с данными, содержащимися в электронной подписи   
		/// </summary>
		[XmlEnum("1")]
		FromElectronicSignature,
		
		/// <summary>
		/// В соответствии с доверенностью в электронной форме в машиночитаемом виде,
		/// если представление доверенности осуществляется посредством включения в каждый пакет электронных документов,
		/// подписываемых представителем   
		/// </summary>
		[XmlEnum("2")]
		FromElectronicFormMachineReadablePowerOfAttorneyInPackageElectronicDocuments,
		
		/// <summary>
		/// В соответствии с доверенностью в электронной форме в машиночитаемом виде,
		/// если представление доверенности осуществляется из информационной системы.
		/// При этом необходимая информация для запроса доверенности из информационной системы, указана в электронном документе   
		/// </summary>
		[XmlEnum("3")]
		FromElectronicFormMachineReadablePowerOfAttorneyInInformationSystem,
		
		/// <summary>
		/// В соответствии с доверенностью в электронной форме в машиночитаемом виде,
		/// если представление доверенности осуществляется из информационной системы.
		/// При этом, необходимая информация для запроса доверенности из информационной системы, представляется способом,
		/// отличным от указания в электронном документе 
		/// </summary>
		[XmlEnum("4")]
		FromElectronicFormMachineReadablePowerOfAttorneyInInformationSystemNotFromElectronicDocument,
		
		/// <summary>
		/// В соответствии с доверенностью в форме документа на бумажном носителе
		/// </summary>
		[XmlEnum("5")]
		FromPaperPowerOfAttorney,
		
		/// <summary>
		/// Иное
		/// </summary>
		[XmlEnum("6")]
		Other
	}
	
	/// <summary>
	/// Тип подписи
	/// <example>
	/// Значения <see cref="SimpleElectronicSignature"/> и <see cref="EnhancedUnqualifiedElectronicSignature"/> применяются,
	/// если иное не предусмотрено законодательством Российской Федерации
	/// </example>
	/// </summary>
	[GeneratedCode("xsd", "4.7.2558.0")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументПодписантТипПодпис")]
	[Serializable]
	public enum CustomerInformationSignatureType
	{
		/// <summary>
		/// Значение по умолчанию
		/// </summary>
		[XmlEnum("")]
		Default = 0,
		
		/// <summary>
		/// Усиленная квалифицированная электронная подпись
		/// </summary>
		[XmlEnum("1")]
		EnhancedQualifiedElectronicSignature = 1,
		
		/// <summary>
		/// Простая электронная подпись
		/// </summary>
		[XmlEnum("2")]
		SimpleElectronicSignature = 2,
		
		/// <summary>
		/// Усиленная неквалифицированная электронная подпись
		/// </summary>
		[XmlEnum("3")]
		EnhancedUnqualifiedElectronicSignature = 3
	}

	/// <summary>
	/// Полное имя
	/// </summary>
	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlRoot("ФИОТип")]
	[Serializable]
	public class FullName
	{
		/// <summary>
		/// Фамилия
		/// </summary>
		[XmlAttribute("Фамилия")]
		public string LastName { get; set; }
		
		/// <summary>
		/// Имя
		/// </summary>
		[XmlAttribute("Имя")]
		public string Name { get; set; }

		/// <summary>
		/// Отчество
		/// </summary>
		[XmlAttribute("Отчество")]
		public string Patronymic { get; set; }
	}


	[Serializable]
	public class DocumentDetails
	{
		/// <summary>
		/// Идентифицирующие реквизиты экономических субъектов, составивших (сформировавших) документ
		/// </summary>
		[XmlElement("РеквИдРекСост")]
		public EconomicSubjectDetails[] EconomicSubjectsDetails { get; set; }
		
		/// <summary>
		/// Наименование документа
		/// </summary>
		[XmlAttribute("РеквНаимДок")]
		public string Name { get; set; }

		/// <summary>
		/// Номер документа
		/// </summary>
		[XmlAttribute("РеквНомерДок")]
		public string Number { get; set; } = "Без номера";
		
		/// <summary>
		/// Дата документа
		/// в формате <c>ДД.ММ.ГГГГ</c>
		/// </summary>
		[XmlAttribute("РеквДатаДок")]
		public string Date { get; set; }
		
		/// <summary>
		/// Идентификатор файла обмена документа, подписанного первой стороной
		/// Содержит (повторяет)имя сформированного файла (без расширения). Указывается идентификатор файла обмена,
		/// в котором значения <c>РеквНаимДок</c>, <c>РеквНомерДок</c>, <c>РеквДатаДок</c> совпадают с одноименными элементами,
		/// указанными в создаваемом файле обмена
		/// </summary>
		[XmlAttribute("РеквИдФайлДок")]
		public string ExchangeFileId { get; set; }

		/// <summary>
		/// Идентификатор документа
		/// Указывается идентификатор документа, содержащийся в информации документа, в том числе регистрационный номер,
		/// если документ/сведения о таком документе содержатся в государственной информационной системе
		/// </summary>
		[XmlAttribute("РеквИдДок")]
		public string Id { get; set; }
		
		/// <summary>
		/// Идентифицирующая информация об информационной системе, в которой осуществляется хранение документа,
		/// необходимая для запроса информации из информационной системы
		/// </summary>
		[XmlAttribute("РИдСистХранД")]
		public string InformationSystemId { get; set; }
		
		/// <summary>
		/// Сведения в формате URL об информационной системе,
		/// которая предоставляет техническую возможность получения информации о документе
		/// </summary>
		[XmlAttribute("РеквУРЛСистДок")]
		public string InformationSystemUrl { get; set; }
		
		/// <summary>
		/// Дополнительные сведения
		/// </summary>
		[XmlAttribute("РеквДопСведДок")]
		public string AdditionalInformation { get; set; }
	}

	[Serializable]
	public class EconomicSubjectDetails
	{
		[XmlElement("ДаннИно", typeof(ForeignOrganizationDetails))]
		[XmlElement("ИННФЛ", typeof(string))]
		[XmlElement("ИННЮЛ", typeof(string))]
		[XmlElement("НаимОИВ", typeof(string))]
		[XmlChoiceIdentifier("ItemElementName")]
		public object Item { get; set; }
		
		public EconomicSubjectDetailsType ItemElementName { get; set; }
	}

	/// <summary>
	/// Сведения об иностранной организации (иностранном гражданине),
	/// не состоящей на учете в налоговых органах <c>СвИнНеУчТип</c>
	/// </summary>
	[Serializable]
	public class ForeignOrganizationDetails
	{
		/// <summary>
		/// Идентификация статуса
		/// </summary>
		[XmlAttribute("ИдСтат")]
		public ForeignOrganizationType ForeignOrganizationType  { get; set; }
		
		/// <summary>
		/// Код страны
		/// </summary>
		[XmlAttribute("КодСтр")]
		public string CountryCode  { get; set; }
		
		/// <summary>
		/// Наименование страны
		/// </summary>
		[XmlAttribute("НаимСтран")]
		public string Country  { get; set; }
		
		/// <summary>
		/// Наименование иностранной организации полное/Фамилия, имя, отчество (при наличии) иностранного гражданина
		/// </summary>
		[XmlAttribute("Наим")]
		public string FullName  { get; set; }
		
		/// <summary>
		/// Идентификатор иностранной организации (иностранного гражданина)
		/// Элемент обязателен при отсутствии <see cref="OtherIdentifyingInformation"/>
		/// </summary>
		[XmlAttribute("Идентиф")]
		public string Id  { get; set; }
		
		/// <summary>
		/// Иные сведения для однозначной идентификации иностранной организации (иностранного гражданина)
		/// Элемент обязателен при отсутствии <see cref="Id"/>
		/// </summary>
		[XmlAttribute("ИныеСвед")]
		public string OtherIdentifyingInformation  { get; set; }
	}

	[Serializable]
	public enum ForeignOrganizationType
	{
		/// <summary>
		/// Иностранная организация
		/// </summary>
		[XmlEnum("ИО")]
		ForeignOrganization,

		/// <summary>
		/// Иностранный гражданин
		/// </summary>
		[XmlEnum("ИГ")]
		ForeignCitizen
	}

	[Serializable]
	public enum EconomicSubjectDetailsType {
    
		/// <summary>
		/// Данные об иностранной организации (иностранном гражданине), не состоящей на учете в налоговых органах
		/// </summary>
		[XmlEnum("ДаннИно")]
		ForeignOrganization,
    
		/// <summary>
		/// ИНН физического лица, в том числе индивидуального предпринимателя
		/// </summary>
		[XmlEnum("ИННФЛ")]
		IndividualInn,
    
		/// <summary>
		/// ИНН организации, состоящей на учете в налоговом органе
		/// </summary>
		[XmlEnum("ИННЮЛ")]
		LegalInn,
    
		/// <summary>
		/// Краткое наименование органа исполнительной власти (специализированной уполномоченной организации), выдавшего документ
		/// </summary>
		[XmlEnum("НаимОИВ")]
		ExecutiveAuthorityShort,
	}

	[Serializable]
	public class InformationOfPurchasesForStateNeeds
	{
		/// <summary>
		/// Идентификационный код закупки
		/// </summary>
		[XmlAttribute("ИдКодЗак")]
		public string Id { get; set; }
		
		/// <summary>
		/// Номер лицевого счета покупателя
		/// </summary>
		[XmlAttribute("ЛицСчетПок")]
		public string PersonalAccount { get; set; }
		
		/// <summary>
		/// <para>
		/// Наименование финансового органа покупателя
		/// </para>
		/// <para>
		/// В случае, если покупатель является участником бюджетного процесса федерального уровня,
		/// то принимает значение: Министерство финансов Российской Федерации.
		/// </para>
		/// <para>
		/// В случае, если покупатель является участником бюджетного процесса субъекта Российской Федерации
		/// или муниципального образования указывается наименование финансового органа соответствующего бюджета
		/// </para>
		/// </summary>
		[XmlAttribute("НаимФинОргПок")]
		public string CustomerFinancialAuthority { get; set; }
		
		/// <summary>
		/// Номер реестровой записи покупателя по Реестру участников бюджетного процесса,
		/// а также юридических лиц, не являющихся участниками бюджетного процесса
		/// </summary>
		[XmlAttribute("НомРеестрЗапПок")]
		public string CustomerRegistrationRecord { get; set; }
	}

	[Serializable]
	public class DataSellerDocumentId
	{
		/// <summary>
		/// <para>
		/// Идентификатор файла обмена информации продавца
		/// </para>
		/// <para>
		/// Содержит (повторяет) имя файла обмена счета-фактуры (информации продавца) или файла обмена информации продавца (без расширения)
		/// </para>
		/// </summary>
		[XmlAttribute("ИдФайлИнфПр")]
		public string Id { get; set; }
		
		/// <summary>
		/// Указывается (повторяет) значение <c>ДатаИнфПр</c>,
		/// указанное в файле обмена счета-фактуры (информации продавца) или файле обмена информации продавца 
		/// в формате <c>ДД.ММ.ГГГГ</c>
		/// </summary>
		[XmlAttribute("ДатаФайлИнфПр")]
		public string DocumentDate { get; set; }
		
		/// <summary>
		/// Указывается (повторяет) значение <c>ВремИнфПр</c>,
		/// указанное в файле обмена счета-фактуры (информации продавца) или файле обмена информации продавца 
		/// в формате <c>ЧЧ.ММ.СС</c>
		/// </summary>
		[XmlAttribute("ВремФайлИнфПр")]
		public string DocumentTime { get; set; }
		
		/// <summary>
		/// Электронная подпись файла обмена информации продавца
		/// </summary>
		[XmlElement("ЭП")]
		public string[] SignatureStringBase64 { get; set; }
	}

	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументСодФХЖ4")]
	[Serializable]
	public class ContentOfFactOfEconomicLife4
	{
		/// <summary>
		/// Наименование первичного документа, согласованное сторонами сделки
		/// Указывается (повторяет) значение <c>НаимДокОпр</c>, указанное в файле обмена счета-фактуры (информации продавца) или файле обмена информации продавца
		/// </summary>
		[XmlAttribute("НаимДокОпрПр")]
		public string SellerFileName { get; set; }
		
		/// <summary>
		/// Указывается (повторяет) значение <c>Функция</c>, указанное в файле обмена счета-фактуры (информации продавца) или файле обмена информации продавца
		/// </summary>
		[XmlAttribute("Функция")]
		public string Function { get; set; }
		
		/// <summary>
		/// Дата в формате <c>ДД.ММ.ГГГГ</c>
		/// Дата поступившего на подпись документа об отгрузке товаров (выполнении работ),
		/// передаче имущественных прав (об оказании услуг).
		/// Указывается (повторяет) значение <c>ДатаДок</c>, указанное в файле обмена счета-фактуры (информации продавца) или файле обмена информации продавца
		/// </summary>
		[XmlAttribute("ДатаДокИнфПр")]
		public string SellerFileDate { get; set; }
		
		/// <summary>
		/// <para>
		/// Порядковый номер (строка 1 счета-фактуры),
		/// документа об отгрузке товаров (выполнении работ),
		/// передаче имущественных прав (документа об оказании услуг)
		/// </para>
		/// Указывается (повторяет) значение <c>НомерДок</c>, указанное в файле обмена счета-фактуры (информации продавца) или файле обмена информации продавца
		/// </summary>
		[XmlAttribute("ПорНомДокИнфПр")]
		public string SellerDocumentNumber { get; set; }
		
		/// <summary>
		/// <para>
		/// Вид операции
		/// </para>
		/// Дополнительная информация, позволяющая в автоматизированном режиме определять необходимый
		/// для конкретного случая порядок использования информации документа у покупателя
		/// </summary>
		[XmlAttribute("ВидОпер")]
		public string Operation { get; set; }
		
		/// <summary>
		/// Сведения о принятии товаров (результатов выполненных работ),
		/// имущественных прав (о подтверждении факта оказания услуг)
		/// </summary>
		[XmlElement("СвПрин")]
		public CompletedActionsInformation CompletedActions { get; set; }
		
		/// <summary>
		/// Информационное поле факта хозяйственной жизни 4
		/// </summary>
		[XmlElement("ИнфПолФХЖ4")]
		public AdditionalContentOfFactOfEconomicLife4 AdditionalData { get; set; }
	}

	[Serializable]
	public class AdditionalContentOfFactOfEconomicLife4
	{
		[XmlAttribute("ИдФайлИнфПол")]
		public Guid Id { get; set; }
		
		[XmlElement("ТекстИнф")]
		public TextInformation[] TextInformation { get; set; }
	}
	
	[Serializable]
	public class TextInformation
	{
		[XmlAttribute("Идентиф")]
		public string Key { get; set; }
		
		[XmlAttribute("Значен")]
		public string Value { get; set; }
	}

	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументСодФХЖ4СвПрин")]
	[Serializable]
	public class CompletedActionsInformation
	{
		/// <summary>
		/// Дата принятия товаров (результатов выполненных работ),
		/// имущественных прав (подтверждения факта оказания услуг)
		/// в формате ДД.ММ.ГГГГ
		/// </summary>
		[XmlAttribute("ДатаПрин")]
		public string Date { get; set; }
		
		/// <summary>
		/// Содержание операции (текст, подтверждающий событие, повлекшее за собой изменение финансового состояния покупателя)
		/// При <see cref="OperationCodeType.NothingIsAccepted"/> элемент не формируется.
		/// Соответствует значению <see cref="OperationCodeType"/> 
		/// </summary>
		[XmlAttribute("СодОпер")]
		public string Operation { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		[XmlElement("КодСодОпер")]
		public OperationCode OperationCode { get; set; }
		
		/// <summary>
		/// Сведения о лице, принявшем товары
		/// </summary>
		[XmlElement("СвЛицПрин")]
		public AcceptedGoodsPerson AcceptedGoodsPerson { get; set; }
	}

	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументСодФХЖ4СвПринСвЛицПрин")]
	[Serializable]
	public class AcceptedGoodsPerson
	{
		/// <summary>
		/// Работник организации покупателя <see cref="CustomerEmployee"/>
		/// или Иное лицо <see cref="AnotherPerson"/>
		/// </summary>
		[XmlElement("ИнЛицо", typeof(AnotherPerson))]
		[XmlElement("РабОргПок", typeof(CustomerEmployee))]
		public object Item { get; set; }
	}

	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументСодФХЖ4СвПринСвЛицПринРабОргПок")]
	[Serializable]
	public class CustomerEmployee
	{
		/// <summary>
		/// Должность
		/// </summary>
		[XmlAttribute("Должность")]
		public string JobPosition { get; set; }
		
		/// <summary>
		/// Иные сведения, идентифицирующие физическое лицо
		/// </summary>
		[XmlAttribute("ИныеСвед")]
		public string OtherIdentifyingInformation  { get; set; }
		
		/// <summary>
		/// Фамилия, имя, отчество (при наличии)
		/// </summary>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }
	}

	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументСодФХЖ4СвПринСвЛицПринИнЛицо")]
	[Serializable]
	public class AnotherPerson
	{
		/// <summary>
		/// Представитель организации, которой доверено принятие товаров <see cref="RepresentativeOrganization"/>
		/// или Физическое лицо, которому доверено принятие товаров <see cref="IndividualPerson"/>
		/// </summary>
		[XmlElement("ПредОргПрин", typeof(RepresentativeOrganization))]
		[XmlElement("ФЛПрин", typeof(IndividualPerson))]
		public object Item { get; set; }
	}

	[Serializable]
	public class RepresentativeOrganization
	{
		/// <summary>
		/// Основание, по которому организации доверено принятие товаров
		/// Обязателен при отсутствии <see cref="Inn"/>
		/// </summary>
		[XmlElement("ОснДоверОргПрин")]
		public DocumentDetails BasisOrganizationAcceptanceGoods  { get; set; }
		
		/// <summary>
		/// Основание полномочий представителя организации на принятие товаров
		/// </summary>
		[XmlElement("ОснПолнПредПрин")]
		public DocumentDetails BasisRepresentativeOrganization  { get; set; }
		
		/// <summary>
		/// Фамилия, имя, отчество (при наличии)
		/// </summary>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }
		
		/// <summary>
		/// Наименование организации
		/// </summary>
		[XmlAttribute("НаимОргПрин")]
		public string Organization { get; set; }
		
		/// <summary>
		/// Должность
		/// </summary>
		[XmlAttribute("Должность")]
		public string JobPosition { get; set; }
		
		/// <summary>
		/// Иные сведения, идентифицирующие физическое лицо
		/// </summary>
		[XmlAttribute("ИныеСвед")]
		public string OtherIdentifyingInformation  { get; set; }
		
		/// <summary>
		/// ИНН юридического лица, которому доверен прием
		/// Обязателен при отсутствии <see cref="BasisOrganizationAcceptanceGoods"/> 
		/// </summary>
		[XmlAttribute("ИННОргПрин")]
		public string Inn  { get; set; }
	}

	[GeneratedCode("xsd", "4.7.2558.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot("ФайлДокументСодФХЖ4СвПринСвЛицПринИнЛицоФЛПрин")]
	[Serializable]
	public class IndividualPerson
	{
		/// <summary>
		/// ИНН физического лица, в том числе индивидуального предпринимателя, которому доверен прием
		/// Обязателен при отсутствии <see cref="Basis"/>
		/// </summary>
		[XmlAttribute("ИННФЛПрин")]
		public string Inn  { get; set; }
		
		/// <summary>
		/// Основание, по которому физическому лицу доверено принятие товаров
		/// Обязателен при отсутствии <see cref="Inn"/>
		/// </summary>
		[XmlElement("ОснДоверФЛ")]
		public DocumentDetails Basis  { get; set; }
		
		/// <summary>
		/// Иные сведения, идентифицирующие физическое лицо
		/// </summary>
		[XmlAttribute("ИныеСвед")]
		public string OtherIdentifyingInformation  { get; set; }
		
		/// <summary>
		/// Фамилия, имя, отчество (при наличии)
		/// </summary>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }
	}

	[Serializable]
	public class OperationCode
	{
		/// <summary>
		/// Код, обозначающий итог приемки товара (работ, услуг, прав)
		/// </summary>
		[XmlAttribute("КодИтога")]
		public OperationCodeType OperationCodeType { get; set; }
	}

	/// <summary>
	/// Итог приемки товара (работ, услуг, прав)
	/// </summary>
	[Serializable]
	public enum OperationCodeType
	{
		/// <summary>
		/// Товары (работы, услуги, права) приняты без расхождений (претензий)
		/// </summary>
		[XmlEnum("1")]
		EverythingAcceptedWithoutDiscrepancies,
		/// <summary>
		/// Товары (работы, услуги, права) приняты с расхождениями (претензией)
		/// </summary>
		[XmlEnum("2")]
		EverythingAcceptedWithDiscrepancies ,
		/// <summary>
		/// Товары (работы, услуги, права) не приняты
		/// Код элемента может принимать значение «3» только при отсутствии элемента <c>ИспрДок</c>
		/// </summary>
		[XmlEnum("3")]
		NothingIsAccepted
	}

	[Serializable]
	public enum Format
	{
		[XmlEnum("5.01")] Format5_01,
		[XmlEnum("5.03")] Format5_03
	}

	[Serializable]
	public enum FiscalDocumentClassifiers
	{
		[XmlEnum("1115131")] KND1115131,
		[XmlEnum("1115132")] KND1115132,
	}
}
