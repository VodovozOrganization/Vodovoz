using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Domain.TrueMark
{
	public class TrueMarkWaterIdentificationCode : TrueMarkWaterIdentificationCodeEntity, ITrueMarkWaterCode
	{
		private string _rawCode;
		private bool _isInvalid;
		private string _gtin;
		private string _serialNumber;
		private string _checkCode;

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

		public override bool Equals(object obj)
		{
			if(obj is TrueMarkWaterIdentificationCode)
			{
				var code = (TrueMarkWaterIdentificationCode)obj;
				var result = _rawCode == code.RawCode;
				return result;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return -1155050507 + EqualityComparer<string>.Default.GetHashCode(_rawCode);
		}
	}
}
