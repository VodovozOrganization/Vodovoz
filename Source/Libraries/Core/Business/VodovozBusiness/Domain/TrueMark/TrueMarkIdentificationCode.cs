using System.Collections.Generic;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Domain.TrueMark
{
	public class TrueMarkWaterIdentificationCode : TrueMarkWaterIdentificationCodeEntity, ITrueMarkWaterCode
	{

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
