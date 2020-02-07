using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum VAT
	{
		[Display(Name = "Без НДС")]
		[Value1c("БезНДС")]
		[Value1cType("БезНДС")]
		No,
		[Display(Name = "НДС 10%")]
		[Value1c("НДС10")]
		[Value1cType("Пониженная")]
		Vat10,
		[Display(Name = "НДС 18%")]
		[Value1c("НДС18")]
		[Value1cType("Общая")]
		Vat18,
		[Display(Name = "НДС 20%")]
		[Value1c("НДС20")]
		[Value1cType("Общая")]
		Vat20
	}

	public class VATStringType : NHibernate.Type.EnumStringType
	{
		public VATStringType() : base(typeof(VAT)) { }
	}
}

namespace Vodovoz.Domain.Client
{
	public enum PaymentType
	{
		[Display(Name = "Наличная", ShortName = "нал.")]
		cash,
		[Display(Name = "Мир напитков", ShortName = "нал.")]
		BeveragesWorld,
		[Display(Name = "Безналичная", ShortName = "б/н.")]
		cashless,
		[Display(Name = "Бартер", ShortName = "бар.")]
		barter,
		[Display(Name = "По карте", ShortName = "карта")]
		ByCard,
		[Display(Name = "Контрактная документация", ShortName = "контрактн.")]
		ContractDoc
	}

	public class PaymentTypeStringType : NHibernate.Type.EnumStringType
	{
		public PaymentTypeStringType() : base(typeof(PaymentType)) { }
	}

	public enum ContractType
	{
		[Display(Name = "Безналичная")]
		Cashless,
		[Display(Name = "Наличная ФЛ")]
		CashFL,
		[Display(Name = "Наличная ЮЛ")]
		CashUL,
		[Display(Name = "Мир Напитков Наличная ФЛ")]
		CashBeveragesFL,
		[Display(Name = "Мир Напитков Наличная ЮЛ")]
		CashBeveragesUL,
		[Display(Name = "Бартер")]
		Barter
	}

	public class ContractTypeStringType : NHibernate.Type.EnumStringType
	{
		public ContractTypeStringType() : base(typeof(ContractType)) { }
	}
}
