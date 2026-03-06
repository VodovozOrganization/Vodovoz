using OneOf;
using System;
using System.Collections.Generic;
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

		public void AddInnerTrueMarkAnyCodes(IEnumerable<TrueMarkAnyCode> innerCodes)
		{
			foreach(var code in innerCodes)
			{
				AddInnerTrueMarkAnyCode(code);
			}
		}

		public void AddInnerTrueMarkAnyCode(TrueMarkAnyCode innerCode)
		{
			Match(
				transportCode =>
				{
					innerCode.Match(
						innerTransportCode =>
						{
							transportCode.AddInnerTransportCode(innerTransportCode);
							return true;
						},
						innerWaterGroupCode =>
						{
							transportCode.AddInnerGroupCode(innerWaterGroupCode);
							return true;
						},
						innerWaterIdentificationCode =>
						{
							transportCode.AddInnerWaterCode(innerWaterIdentificationCode);
							return true;
						});

					return true;
				},
				waterGroupCode =>
				{
					innerCode.Match(
						innerTransportCode =>
						{
							throw new InvalidOperationException("Нельзя добавить транспортный код внуть группового кода");
						},
						innerWaterGroupCode =>
						{
							waterGroupCode.AddInnerGroupCode(innerWaterGroupCode);
							return true;
						},
						innerWaterIdentificationCode =>
						{
							waterGroupCode.AddInnerWaterCode(innerWaterIdentificationCode);
							return true;
						});

					return true;
				},
				waterIdentificationCode =>
				{
					throw new InvalidOperationException("Нельзя добавить коды внуть кода экземпляра");
				});
		}
	}
}
