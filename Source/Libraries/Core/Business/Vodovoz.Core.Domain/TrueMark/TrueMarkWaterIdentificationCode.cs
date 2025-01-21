using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Interfaces.TrueMark;

namespace Vodovoz.Core.Domain.TrueMark
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "код честного знака",
			Nominative = "код честного знака"
		)
	]
	public class TrueMarkWaterIdentificationCode : PropertyChangedBase, IDomainObject, ITrueMarkWaterCode
	{
		public virtual int Id { get; set; }
		private string _rawCode;
		private bool _isInvalid;
		private string _gtin;
		private string _serialNumber;
		private string _checkCode;
		private Tag1260CodeCheckResult _tag1260CodeCheckResult;
		private bool _isTagValid;

		[Display(Name = "Необработанный код")]
		public virtual string RawCode
		{
			get => _rawCode;
			set => SetField(ref _rawCode, value);
		}

		[Display(Name = "Код не валидный")]
		public virtual bool IsInvalid
		{
			get => _isInvalid;
			set => SetField(ref _isInvalid, value);
		}

		[Display(Name = "Код GTIN")]
		public virtual string GTIN
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		[Display(Name = "Серийный номер экземпляра")]
		public virtual string SerialNumber
		{
			get => _serialNumber;
			set => SetField(ref _serialNumber, value);
		}

		[Display(Name = "Код проверки")]
		public virtual string CheckCode
		{
			get => _checkCode;
			set => SetField(ref _checkCode, value);
		}

		[Display(Name = "Результаты проверки кода для тэга 1260")]
		public virtual Tag1260CodeCheckResult Tag1260CodeCheckResult
		{
			get => _tag1260CodeCheckResult;
			set => SetField(ref _tag1260CodeCheckResult, value);
		}

		[Display(Name = "Код валиден для тэга 1260")]
		public virtual bool IsTag1260Valid 
		{ 
			get => _isTagValid;
			set => SetField(ref _isTagValid, value);
		}

		public override bool Equals(object obj)
		{
			if(obj is TrueMarkWaterIdentificationCode)
			{
				var code = (TrueMarkWaterIdentificationCode)obj;
				var result = RawCode == code.RawCode;
				return result;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return -1155050507 + EqualityComparer<string>.Default.GetHashCode(RawCode);
		}
	}
}
