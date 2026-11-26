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
	///		<description>Код идентификации <see cref="TrueMarkIdentificationCode"/></description>
	///	</item>
	/// <item>
	///		<term>КИГУ</term>
	///		<description>Код идентификации групповой упаковки <see cref="TrueMarkGroupCode"/></description>
	///	</item>
	/// <item>
	///		<term>КИТУ</term>
	///		<description>Код идентификации транспортной упаковки <see cref="TrueMarkTransportCode"/></description>
	///	</item>
	/// </list>
	/// </summary>
	public class TrueMarkAnyCode : OneOfBase<TrueMarkTransportCode, TrueMarkGroupCode, TrueMarkIdentificationCode>
	{
		protected TrueMarkAnyCode(OneOf<TrueMarkTransportCode, TrueMarkGroupCode, TrueMarkIdentificationCode> oneOf) : base(oneOf)
		{
		}

		public static implicit operator TrueMarkAnyCode(TrueMarkTransportCode value) => new TrueMarkAnyCode(value);

		public static implicit operator TrueMarkAnyCode(TrueMarkGroupCode value) => new TrueMarkAnyCode(value);

		public static implicit operator TrueMarkAnyCode(TrueMarkIdentificationCode value) => new TrueMarkAnyCode(value);

		public bool IsTrueMarkTransportCode => IsT0;

		public bool IsTrueMarkGroupCode => IsT1;

		public bool IsTrueMarkIdentificationCode => IsT2;

		public TrueMarkTransportCode TrueMarkTransportCode => AsT0;

		public TrueMarkGroupCode TrueMarkGroupCode => AsT1;

		public TrueMarkIdentificationCode TrueMarkIdentificationCode => AsT2;

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
						innerGroupCode =>
						{
							transportCode.AddInnerGroupCode(innerGroupCode);
							return true;
						},
						innerIdentificationCode =>
						{
							transportCode.AddInnerIdentificationCode(innerIdentificationCode);
							return true;
						});

					return true;
				},
				groupCode =>
				{
					innerCode.Match(
						innerTransportCode =>
						{
							throw new InvalidOperationException("Нельзя добавить транспортный код внуть группового кода");
						},
						innerGroupCode =>
						{
							groupCode.AddInnerGroupCode(innerGroupCode);
							return true;
						},
						innerIdentificationCode =>
						{
							groupCode.AddInnerIdentificationCode(innerIdentificationCode);
							return true;
						});

					return true;
				},
				identificationCode =>
				{
					throw new InvalidOperationException("Нельзя добавить коды внуть кода экземпляра");
				});
		}
	}
}
