using OneOf;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Domain.TrueMark
{
	/// <summary>
	/// Любой из кодов честного знака:
	/// <list type="bullet">
	/// <item>
	///		<term>КИ</term>
	///		<description>Код идентификации <see cref="TrueMarkWaterIdentificationCode"/></description>
	///	</item>
	/// <item>
	///		<term>КИГУ</term>
	///		<description>Код идентификации групповой упаковки <see cref="TrueMarkWaterGroupCode"/></description>
	///	</item>
	/// <item>
	///		<term>КИТУ</term>
	///		<description>Код идентификации транспортной упаковки <see cref="TrueMarkTransportCode"/></description>
	///	</item>
	/// </list>
	/// </summary>
	public class TrueMarkAnyCode : OneOfBase<TrueMarkTransportCode, TrueMarkWaterGroupCode, TrueMarkWaterIdentificationCode>
	{
		protected TrueMarkAnyCode(OneOf<TrueMarkTransportCode, TrueMarkWaterGroupCode, TrueMarkWaterIdentificationCode> oneOf) : base(oneOf)
		{
		}

		public static implicit operator TrueMarkAnyCode(TrueMarkTransportCode value) => new TrueMarkAnyCode(value);

		public static implicit operator TrueMarkAnyCode(TrueMarkWaterGroupCode value) => new TrueMarkAnyCode(value);

		public static implicit operator TrueMarkAnyCode(TrueMarkWaterIdentificationCode value) => new TrueMarkAnyCode(value);

		public bool IsTrueMarkTransportCode => IsT0;

		public bool IsTrueMarkWaterGroupCode => IsT1;

		public bool IsTrueMarkWaterIdentificationCode => IsT2;

		public TrueMarkTransportCode TrueMarkTransportCode => AsT0;

		public TrueMarkWaterGroupCode TrueMarkWaterGroupCode => AsT1;

		public TrueMarkWaterIdentificationCode TrueMarkWaterIdentificationCode => AsT2;
	}
}
