using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Extensions;
using QS.DomainModel.UoW;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.TrueMark;
using VodovozBusiness.Services.TrueMark;
using Xunit;

namespace VodovozBusiness.TrueMark.Tests
{
	/// <summary>
	/// Тесты обработки кодов маркировки, отсканированных водителем для адреса маршрутного листа.
	/// </summary>
	public class RouteListItemTrueMarkProductCodesProcessingServiceTests
	{
		private const int _orderId = 1;
		private const int _orderItemId = 2;
		private const int _transferredIdentificationCodeId = 3;
		private const string _gtin = "04602009723186";

		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly ITrueMarkCodesPool _trueMarkCodesPool;
		private readonly RouteListItemTrueMarkProductCodesProcessingService _service;

		public RouteListItemTrueMarkProductCodesProcessingServiceTests()
		{
			_uow = Substitute.For<IUnitOfWork>();
			_trueMarkRepository = Substitute.For<ITrueMarkRepository>();
			_trueMarkCodesPool = Substitute.For<ITrueMarkCodesPool>();
			_trueMarkRepository
				.GetTransferredProductCode(Arg.Any<IUnitOfWork>(), Arg.Any<int>(), Arg.Any<string>())
				.Returns((AutoTrueMarkProductCode)null);

			var trueMarkCodesPoolFactory = Substitute.ForPartsOf<TrueMarkCodesPoolFactory>(
				Substitute.For<IUnitOfWorkFactory>());
			trueMarkCodesPoolFactory.Configure().Create(_uow).Returns(_trueMarkCodesPool);

			_service = new RouteListItemTrueMarkProductCodesProcessingService(
				Substitute.For<IOrderRepository>(),
				Substitute.For<IGenericRepository<RouteListItemEntity>>(),
				Substitute.For<IGenericRepository<StagingTrueMarkCode>>(),
				Substitute.For<ITrueMarkWaterCodeService>(),
				_trueMarkRepository,
				trueMarkCodesPoolFactory);
		}

		/// <summary>
		/// Проверяет, что принятый водительский код имеет приоритет,
		/// а ранее перенесенный код возвращается в пул.
		/// </summary>
		[Fact]
		public async Task AcceptedDriverCode_ReturnsTransferredCodeToPool()
		{
			var transferredProductCode = CreateTransferredProductCode();
			var routeListItem = CreateRouteListItem();
			var driverIdentificationCode = CreateDriverIdentificationCode();

			_trueMarkRepository
				.GetTransferredProductCode(_uow, _orderId, _gtin)
				.Returns(transferredProductCode);

			await _service.AddTrueMarkAnyCodeToRouteListItemNoCodeStatusCheck(
				_uow,
				routeListItem,
				_orderItemId,
				driverIdentificationCode,
				SourceProductCodeStatus.Accepted,
				ProductCodeProblem.None);

			await _trueMarkCodesPool
				.Received(1)
				.PutCodeAsync(_transferredIdentificationCodeId, Arg.Any<CancellationToken>());
			Assert.Null(transferredProductCode.ResultCode);
			Assert.Equal(SourceProductCodeStatus.SavedToPool, transferredProductCode.SourceCodeStatus);
			Assert.Same(driverIdentificationCode, routeListItem.TrueMarkCodes.Single().SourceCode);
		}

		/// <summary>
		/// Проверяет, что при отсутствии перенесенного кода пул не изменяется,
		/// а водительский код добавляется в адрес маршрутного листа.
		/// </summary>
		[Fact]
		public async Task AcceptedDriverCode_WithoutTransferredCode_DoesNotUsePool()
		{
			var routeListItem = CreateRouteListItem();
			var driverIdentificationCode = CreateDriverIdentificationCode();

			await _service.AddTrueMarkAnyCodeToRouteListItemNoCodeStatusCheck(
				_uow,
				routeListItem,
				_orderItemId,
				driverIdentificationCode,
				SourceProductCodeStatus.Accepted,
				ProductCodeProblem.None);

			await _trueMarkCodesPool
				.DidNotReceive()
				.PutCodeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
			Assert.Same(driverIdentificationCode, routeListItem.TrueMarkCodes.Single().SourceCode);
		}

		/// <summary>
		/// Проверяет, что отклоненный или проблемный водительский код
		/// не вытесняет ранее перенесенный код.
		/// </summary>
		[Theory]
		[InlineData(SourceProductCodeStatus.Rejected, ProductCodeProblem.None)]
		[InlineData(SourceProductCodeStatus.Accepted, ProductCodeProblem.Defect)]
		public async Task UnacceptedDriverCode_DoesNotReturnTransferredCodeToPool(
			SourceProductCodeStatus status,
			ProductCodeProblem problem)
		{
			var routeListItem = CreateRouteListItem();

			await _service.AddTrueMarkAnyCodeToRouteListItemNoCodeStatusCheck(
				_uow,
				routeListItem,
				_orderItemId,
				CreateDriverIdentificationCode(),
				status,
				problem);

			_trueMarkRepository
				.DidNotReceive()
				.GetTransferredProductCode(Arg.Any<IUnitOfWork>(), Arg.Any<int>(), Arg.Any<string>());
			await _trueMarkCodesPool
				.DidNotReceive()
				.PutCodeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
		}

		private static AutoTrueMarkProductCode CreateTransferredProductCode() =>
			new AutoTrueMarkProductCode
			{
				ResultCode = new TrueMarkWaterIdentificationCode
				{
					Id = _transferredIdentificationCodeId,
					Gtin = _gtin
				},
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				Problem = ProductCodeProblem.None
			};

		private static RouteListItemEntity CreateRouteListItem() =>
			new RouteListItemEntity
			{
				Order = new OrderEntity
				{
					Id = _orderId
				}
			};

		private static TrueMarkWaterIdentificationCode CreateDriverIdentificationCode() =>
			new TrueMarkWaterIdentificationCode
			{
				Id = 4,
				Gtin = _gtin,
				SerialNumber = "driver-code"
			};
	}
}
