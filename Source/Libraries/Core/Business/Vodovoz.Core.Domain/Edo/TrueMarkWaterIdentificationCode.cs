using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Interfaces.TrueMark;

namespace Vodovoz.Core.Domain.Edo
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
		private bool _isTagValid;
		private Tag1260CodeCheckResult _tag1260CodeCheckResult;

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
		
		[Display(Name = "Код валиден для тэга 1260")]
		public virtual bool IsTag1260Valid 
		{ 
			get => _isTagValid;
			set => SetField(ref _isTagValid, value);
		}
		
		[Display(Name = "Результаты проверки кода для тэга 1260")]
		public virtual Tag1260CodeCheckResult Tag1260CodeCheckResult
		{
			get => _tag1260CodeCheckResult;
			set => SetField(ref _tag1260CodeCheckResult, value);
		}

		public virtual string IdentificationCode => $"01{GTIN}21{SerialNumber}";

		public virtual string FullCode => $"\u001d01{GTIN}21{SerialNumber}\u001d93{CheckCode}";
		
		/// <summary>
		/// Получение КИ(кода идентификации) для документа по ЭДО
		/// </summary>
		/// <returns>КИ</returns>
		public virtual string ConvertToIdentificationCode() => GetIdentificationCodeForEdoDocument();
		/// <summary>
		/// Получение КИГУ(кода идентификации групповой упаковки) для документа по ЭДО
		/// </summary>
		/// <returns>КИ</returns>
		public virtual string ConvertToGroupPackagingIdentificationCode() => GetIdentificationCodeForEdoDocument();

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

		private string GetIdentificationCodeForEdoDocument()
		{
			var code = $"01{Get14CharsGtin()}21{_serialNumber}";
			
			return TryReplaceReservedChars(code);
		}

		private string TryReplaceReservedChars(string code)
		{
			const char lessThan = '<';
			const char greaterThan = '>';
			const char ampersand = '&';
			
			var sb = new StringBuilder(code);
			
			for(var i = 0; i < sb.Length; i++)
			{
				if(sb[i] == lessThan)
				{
					sb.Remove(i, 1);
					sb.Insert(i,"&lt;");
					continue;
				}
				
				if(sb[i] == greaterThan)
				{
					sb.Remove(i, 1);
					sb.Insert(i,"&gt;");
					continue;
				}
				
				if(sb[i] == ampersand)
				{
					sb.Remove(i, 1);
					sb.Insert(i,"&amp;");
				}
			}
			
			return sb.ToString();
		}

		private string Get14CharsGtin()
		{
			var diff = 14 - _gtin.Length;
			var sb = new StringBuilder();
			
			//Если Gtin меньше 14 символов, то дополняем его лидирующими нулями
			for(var i = 0; i < diff; i++)
			{
				sb.Append(0);
			}
			
			sb.Append(_gtin);

			return sb.ToString();
		}
	}
}
