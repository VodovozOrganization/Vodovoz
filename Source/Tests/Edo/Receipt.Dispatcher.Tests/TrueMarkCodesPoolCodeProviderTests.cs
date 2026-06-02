using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Microsoft.Extensions.Logging;
using NHibernate;
using NSubstitute;
using QS.DomainModel.UoW;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Xunit;

namespace Receipt.Dispatcher.Tests
{
	public class TrueMarkCodesPoolCodeProviderTests
	{
		private const string _organizationInn = "1234567890";

		private readonly ISession _session;
		private readonly ITrueMarkCodesValidator _trueMarkCodesValidator;
		private readonly ITrueMarkCodesPool _codesPool;
		private readonly TrueMarkCodesPoolCodeProvider _provider;

		public TrueMarkCodesPoolCodeProviderTests()
		{
			var uow = Substitute.For<IUnitOfWork>();
			_session = Substitute.For<ISession>();
			_trueMarkCodesValidator = Substitute.For<ITrueMarkCodesValidator>();
			_codesPool = Substitute.For<ITrueMarkCodesPool>();
			var logger = Substitute.For<ILogger<TrueMarkCodesPoolCodeProvider>>();
			
			uow.Session.Returns(_session);

			_provider = new TrueMarkCodesPoolCodeProvider(
				uow,
				_trueMarkCodesValidator,
				logger);
		}

		[Fact]
		public async Task TakeValidCodeAsync_WhenPoolReturnsValidCode_ReturnsCode()
		{
			// Arrange
			var cancellationToken = CancellationToken.None;
			var gtin = CreateGtin("4600000000001");
			var code = CreateCode(1, gtin.GtinNumber);

			_codesPool.TakeCode(gtin.GtinNumber, cancellationToken).Returns(Task.FromResult(code.Id));
			_session.GetAsync<TrueMarkWaterIdentificationCode>(code.Id, cancellationToken).Returns(Task.FromResult(code));
			_trueMarkCodesValidator
				.ValidateAsync(Arg.Any<IEnumerable<TrueMarkWaterIdentificationCode>>(), _organizationInn, cancellationToken)
				.Returns(Task.FromResult(CreateValidationResult(code, isValid: true)));

			// Act
			var result = await _provider.TakeValidCodeAsync(_codesPool, gtin, _organizationInn, cancellationToken);

			// Assert
			Assert.Same(code, result);
			await _codesPool.Received(1).TakeCode(gtin.GtinNumber, cancellationToken);
			await _trueMarkCodesValidator.Received(1)
				.ValidateAsync(Arg.Any<IEnumerable<TrueMarkWaterIdentificationCode>>(), _organizationInn, cancellationToken);
		}

		[Fact]
		public async Task TakeValidCodeAsync_WhenFirstCodeIsInvalid_TakesNextCode()
		{
			// Arrange
			var cancellationToken = CancellationToken.None;
			var gtin = CreateGtin("4600000000001");
			var invalidCode = CreateCode(1, gtin.GtinNumber);
			var validCode = CreateCode(2, gtin.GtinNumber);

			_codesPool.TakeCode(gtin.GtinNumber, cancellationToken)
				.Returns(Task.FromResult(invalidCode.Id), Task.FromResult(validCode.Id));
			_session.GetAsync<TrueMarkWaterIdentificationCode>(invalidCode.Id, cancellationToken)
				.Returns(Task.FromResult(invalidCode));
			_session.GetAsync<TrueMarkWaterIdentificationCode>(validCode.Id, cancellationToken)
				.Returns(Task.FromResult(validCode));
			_trueMarkCodesValidator
				.ValidateAsync(Arg.Any<IEnumerable<TrueMarkWaterIdentificationCode>>(), _organizationInn, cancellationToken)
				.Returns(
					Task.FromResult(CreateValidationResult(invalidCode, isValid: false)),
					Task.FromResult(CreateValidationResult(validCode, isValid: true)));

			// Act
			var result = await _provider.TakeValidCodeAsync(_codesPool, gtin, _organizationInn, cancellationToken);

			// Assert
			Assert.Same(validCode, result);
			await _codesPool.Received(2).TakeCode(gtin.GtinNumber, cancellationToken);
			await _trueMarkCodesValidator.Received(2)
				.ValidateAsync(Arg.Any<IEnumerable<TrueMarkWaterIdentificationCode>>(), _organizationInn, cancellationToken);
		}

		[Fact]
		public async Task TakeValidCodeAsync_WhenFirstGtinPoolIsEmpty_TriesNextGtin()
		{
			// Arrange
			var cancellationToken = CancellationToken.None;
			var firstGtin = CreateGtin("4600000000001");
			var secondGtin = CreateGtin("4600000000002");
			var code = CreateCode(2, secondGtin.GtinNumber);

			_codesPool.TakeCode(firstGtin.GtinNumber, cancellationToken)
				.Returns(Task.FromException<int>(new EdoCodePoolMissingCodeException("missing")));
			_codesPool.TakeCode(secondGtin.GtinNumber, cancellationToken)
				.Returns(Task.FromResult(code.Id));
			_session.GetAsync<TrueMarkWaterIdentificationCode>(code.Id, cancellationToken)
				.Returns(Task.FromResult(code));
			_trueMarkCodesValidator
				.ValidateAsync(Arg.Any<IEnumerable<TrueMarkWaterIdentificationCode>>(), _organizationInn, cancellationToken)
				.Returns(Task.FromResult(CreateValidationResult(code, isValid: true)));

			// Act
			var result = await _provider.TakeValidCodeAsync(
				_codesPool,
				new[] { firstGtin, secondGtin },
				_organizationInn,
				cancellationToken);

			// Assert
			Assert.Same(code, result);
			await _codesPool.Received(1).TakeCode(firstGtin.GtinNumber, cancellationToken);
			await _codesPool.Received(1).TakeCode(secondGtin.GtinNumber, cancellationToken);
		}

		[Fact]
		public async Task TakeValidCodeAsync_WhenCodeFromPoolIsNotFound_ThrowsInvalidOperationException()
		{
			// Arrange
			var cancellationToken = CancellationToken.None;
			var gtin = CreateGtin("4600000000001");

			_codesPool.TakeCode(gtin.GtinNumber, cancellationToken).Returns(Task.FromResult(1));
			_session.GetAsync<TrueMarkWaterIdentificationCode>(1, cancellationToken)
				.Returns(Task.FromResult<TrueMarkWaterIdentificationCode>(null));

			// Act & Assert
			await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
				_provider.TakeValidCodeAsync(_codesPool, gtin, _organizationInn, cancellationToken));

			await _trueMarkCodesValidator.DidNotReceive()
				.ValidateAsync(Arg.Any<IEnumerable<TrueMarkWaterIdentificationCode>>(), _organizationInn, cancellationToken);
		}

		private static GtinEntity CreateGtin(string gtinNumber)
		{
			return new GtinEntity { GtinNumber = gtinNumber };
		}

		private static TrueMarkWaterIdentificationCode CreateCode(int id, string gtin)
		{
			return new TrueMarkWaterIdentificationCode
			{
				Id = id,
				Gtin = gtin
			};
		}

		private static TrueMarkTaskValidationResult CreateValidationResult(TrueMarkWaterIdentificationCode code, bool isValid)
		{
			return new TrueMarkTaskValidationResult(new[]
			{
				new TrueMarkCodeValidationResult
				{
					Code = code,
					IsValid = isValid,
					IsExpired = !isValid
				}
			});
		}
	}
}
