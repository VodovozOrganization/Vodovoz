using Edo.Admin;
using Edo.Common;
using Edo.Common.Services;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using Edo.Receipt.Dispatcher;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using TrueMark.Library;
using TrueMarkApi.Client;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Organizations;
using Xunit;

namespace Receipt.Dispatcher.Tests
{
	public class CreateMarkedFiscalDocumentsTests
	{
		private GenericRepositoryFixture<TrueMarkWaterGroupCode> _waterGroupCodeRepository;
		private ForOwnNeedsReceiptEdoTaskHandler _forOwnNeedsReceiptEdoTaskHandler;
		private ReceiptTrueMarkCodesPool _trueMarkCodesPool;

		public CreateMarkedFiscalDocumentsTests()
		{
			_waterGroupCodeRepository = new GenericRepositoryFixture<TrueMarkWaterGroupCode>();
			_forOwnNeedsReceiptEdoTaskHandler = CreateForOwnNeedsReceiptEdoTaskHandlerFixture(_waterGroupCodeRepository);
		}

		[Fact]
		public async Task CreateMarkedFiscalDocuments_Should_Create_Equal_Count_Of_FiscalInventPositions_As_OrderItems_Count_And_Have_Correct_Price_For_TrueMarkAccountableItems()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(new [] { 3, 4 }, new [] { 3, 4 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems
				new (int nomenclatureId, decimal count, decimal price, decimal discount)[]
				{
					(1, 7m, 20m, 5m),
					(1, 7m, 20m, 5m),
					(3, 1m, 20m, 5m)
				},
				// Identification Codes
				new (bool isInValid, int gtinId)[]
				{
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{1, 2, 3, 4, 5, 6}),
					(null, 2, false, new []{7, 8, 9, 10, 11, 12})
				},
				// CarLoad Document Items for IdentificationCodes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
				});

			var mainFiscalDocument = new EdoFiscalDocument { Index = 0 };
			receiptEdoTask.FiscalDocuments.Add(mainFiscalDocument);

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			Assert.Equal(receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Quantity)),
				receiptEdoTask.FormalEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count));

			Assert.Equal(
				receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)),
				receiptEdoTask.FormalEdoRequest.Order.OrderItems
					.Where(x =>
						x.Nomenclature.IsAccountableInTrueMark
						&& x.Count > 0)
					.Sum(x => x.Sum));
		}

		// Групповые коды должны распределиться на несколько строк заказа с одной номенклатурой, но разными по стоимости и количеству
		// суммарное количество кодов, входящих в состав групповых равно сумме количества строк заказа
		// количество строк заказа меньше количества групп кодов
		// итоговая стоимость в чеке должна быть как в исходном заказе
		[Fact]
		public async Task CreateMarkedFiscalDocuments_ShoudDistributeGroupCodes_WhenSameItemsOfOneNomenclatureButDifferentPricesExists_AndGroupCodesCountMoreThanOrderItemsCount()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems
				new (int nomenclatureId, decimal count, decimal price, decimal discount)[]
				{
					(1, 4m, 100m, 10m),
					(1, 4m, 140m, 15m),
					(1, 2m, 125m, 5m)
				},
				// Identification Codes
				new (bool isInValid, int gtinId)[]
				{
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{ 1, 2, 3 }),
					(null, 1, false, new []{ 4, 5, 6 }),
					(null, 1, false, new []{ 7, 8, 9 })
				},
				// Edo Task Item Identification Codes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10
				});

			var mainFiscalDocument = new EdoFiscalDocument { Index = 0 };
			receiptEdoTask.FiscalDocuments.Add(mainFiscalDocument);

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert
			//Ожидаем 4 строки чека: 3 с групповыми кодами, 1 с индивидуальным
			Assert.Equal(4, receiptEdoTask.FiscalDocuments.SelectMany(x => x.InventPositions).Count());

			Assert.Equal(receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Quantity)),
				receiptEdoTask.FormalEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count));

			Assert.Equal(
				receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)),
				receiptEdoTask.FormalEdoRequest.Order.OrderItems
					.Where(x =>
						x.Nomenclature.IsAccountableInTrueMark
						&& x.Count > 0)
					.Sum(x => x.Sum));
		}

		// Проверка, что если не получается распределить группы - все возьмется из пула
		[Fact]
		public async Task CreateMarkedFiscalDocuments_Should_Create_Equal_Count_Of_FiscalInventPositions_As_OrderItems_Count_And_Have_Correct_Price_For_TrueMarkAccountableItems_And_Use_Codes_From_Pool()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(new [] { 3, 4 }, new [] { 3, 4 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems
				new[]
				{
					(1, 5m, 2m, 1m),
					(1, 5m, 2m, 1m),
					(3, 1m, 2m, 1m)
				},
				// Identification Codes
				new (bool isInValid, int gtinId)[]
				{
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 1),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
					(false, 2),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }),
				},
				// CarLoad Document Items for IdentificationCodes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
				});

			var mainFiscalDocument = new EdoFiscalDocument { Index = 0 };
			receiptEdoTask.FiscalDocuments.Add(mainFiscalDocument);

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			Assert.Equal(receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Quantity)),
				receiptEdoTask.FormalEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count));

			Assert.Equal(
				receiptEdoTask.FiscalDocuments.Sum(x => x.InventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum)),
				receiptEdoTask.FormalEdoRequest.Order.OrderItems
					.Where(x =>
						x.Nomenclature.IsAccountableInTrueMark
						&& x.Count > 0)
					.Sum(x => x.Sum));

			Assert.All(
				receiptEdoTask.Items
					.Where(x => x.ProductCode.SourceCode != null)
					.Select(x => x.ProductCode.SourceCode.Gtin),
				x => Assert
					.DoesNotContain(x, receiptEdoTask.FiscalDocuments
					.SelectMany(x => x.InventPositions.Select(x => x.EdoTaskItem.ProductCode.ResultCode.Gtin))));
		}

		// Если чистую стоимость за единицу товара для группового кода нельзя получить без остатка (до копеек),
		// групповой код не должен использоваться: товары становятся отдельными строками чека,
		// а входящие в групповой код штучные коды возвращаются в пул
		[Fact]
		public async Task CreateMarkedFiscalDocuments_ShouldNotGroupAndReturnGroupCodeToPool_WhenNetSumPerItemHasRemainder()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems: 11 шт по 30 руб без скидки + 1 шт по 30 руб со скидкой 29 руб.
				// Чистая сумма 11*30 + (30-29) = 331 руб не делится на 12 без остатка (331/12 = 27,58(3))
				new (int nomenclatureId, decimal count, decimal price, decimal discount)[]
				{
					(1, 11m, 30m, 0m),
					(1, 1m, 30m, 29m)
				},
				// Identification Codes: 1-12 входят в групповой код, 13-24 — штучные для индивидуальной обработки
				new (bool isInValid, int gtinId)[]
				{
					(false, 1), (false, 1), (false, 1), (false, 1), (false, 1), (false, 1),
					(false, 1), (false, 1), (false, 1), (false, 1), (false, 1), (false, 1),
					(false, 1), (false, 1), (false, 1), (false, 1), (false, 1), (false, 1),
					(false, 1), (false, 1), (false, 1), (false, 1), (false, 1), (false, 1),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }),
				},
				// Edo Task Item Identification Codes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12,
					13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24
				});

			var mainFiscalDocument = new EdoFiscalDocument { Index = 0 };
			receiptEdoTask.FiscalDocuments.Add(mainFiscalDocument);

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			var inventPositions = receiptEdoTask.FiscalDocuments.SelectMany(x => x.InventPositions).ToList();

			// групповой код не должен использоваться ни в одной строке чека
			Assert.All(inventPositions, x => Assert.Null(x.GroupCode));

			// все 12 единиц товара стали отдельными строками чека с индивидуальными кодами
			Assert.Equal(12, inventPositions.Count);
			Assert.All(inventPositions, x => Assert.Equal(1m, x.Quantity));
			Assert.All(inventPositions, x => Assert.NotNull(x.EdoTaskItem));

			// итоговое количество и стоимость соответствуют исходному заказу
			Assert.Equal(
				receiptEdoTask.FormalEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count),
				inventPositions.Sum(x => x.Quantity));

			Assert.Equal(
				receiptEdoTask.FormalEdoRequest.Order.OrderItems
					.Where(x => x.Nomenclature.IsAccountableInTrueMark && x.Count > 0)
					.Sum(x => x.Sum),
				inventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum));

			// входящие в отклонённый групповой код 12 штучных кодов возвращены в пул
			await _trueMarkCodesPool.Received(12).PutCodeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
		}

		// Проверка расформирования группы, один групповой код в одной строке заказа: количества в строке заказа
		// хватает ровно на один групповой код, но сумма скидки такова, что чистую стоимость за единицу
		// нельзя получить без остатка (до копеек). Групповой код не применяется — товары становятся
		// отдельными строками чека, а входящие в него штучные коды возвращаются в пул
		[Fact]
		public async Task CreateMarkedFiscalDocuments_ShouldNotApplyGroupCodeToSingleOrderItem_WhenDiscountMakesNetSumPerItemHaveRemainder()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems: 3 шт по 10 руб со скидкой 1 руб на строку.
				// Скидка распределяется по единицам как [0,3; 0,3; 0,4].
				// Чистая сумма 3*10 - 1 = 29 руб не делится на 3 без остатка (29/3 = 9,66(6))
				new (int nomenclatureId, decimal count, decimal price, decimal discount)[]
				{
					(1, 3m, 10m, 1m)
				},
				// Identification Codes: 1-3 входят в групповой код, 4-6 — штучные для индивидуальной обработки
				new (bool isInValid, int gtinId)[]
				{
					(false, 1), (false, 1), (false, 1),
					(false, 1), (false, 1), (false, 1),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{ 1, 2, 3 }),
				},
				// Edo Task Item Identification Codes
				new[]
				{
					1, 2, 3, 4, 5, 6
				});

			var mainFiscalDocument = new EdoFiscalDocument { Index = 0 };
			receiptEdoTask.FiscalDocuments.Add(mainFiscalDocument);

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			var inventPositions = receiptEdoTask.FiscalDocuments.SelectMany(x => x.InventPositions).ToList();

			// групповой код не применён ни в одной строке чека
			Assert.All(inventPositions, x => Assert.Null(x.GroupCode));

			// все 3 единицы товара стали отдельными строками чека с индивидуальными кодами
			Assert.Equal(3, inventPositions.Count);
			Assert.All(inventPositions, x => Assert.Equal(1m, x.Quantity));
			Assert.All(inventPositions, x => Assert.NotNull(x.EdoTaskItem));

			// итоговое количество и стоимость соответствуют исходному заказу
			Assert.Equal(
				receiptEdoTask.FormalEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count),
				inventPositions.Sum(x => x.Quantity));

			Assert.Equal(
				receiptEdoTask.FormalEdoRequest.Order.OrderItems
					.Where(x => x.Nomenclature.IsAccountableInTrueMark && x.Count > 0)
					.Sum(x => x.Sum),
				inventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum));

			// входящие в отклонённый групповой код 3 штучных кода возвращены в пул
			await _trueMarkCodesPool.Received(3).PutCodeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
		}

		// Проверка расформирования группы, два групповых кода в одной строке заказа: количества хватает на два
		// групповых кода, но по сумме скидки без остатка можно применить только один. Один групповой
		// код применяется (объединённая строка чека на 3 единицы), второй отклоняется — его товары
		// становятся отдельными строками, а входящие в него штучные коды возвращаются в пул
		[Fact]
		public async Task CreateMarkedFiscalDocuments_ShouldApplyOnlyOneOfTwoGroupCodesToSingleOrderItem_WhenSecondGroupNetSumPerItemHasRemainder()
		{
			// Arrange

			var receiptEdoTask = CreateTestReceiptEdoTaskForTest(
				// Nomenclatures
				new (IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)[]
				{
					(new [] { 1, 2 }, new [] { 1, 2 }, true),
					(Array.Empty<int>(), Array.Empty<int>(), false)
				},
				// OrderItems: 6 шт по 10 руб со скидкой 1 руб на строку.
				// Скидка распределяется по единицам как [0,2; 0,2; 0,2; 0,2; 0,2; 0,0].
				// Первый групповой код (первые 3 единицы): чистая сумма 30 - 0,6 = 29,4 делится на 3 (9,8) — применяется.
				// Второй групповой код (оставшиеся 3 единицы): чистая сумма 30 - 0,4 = 29,6 не делится на 3 — отклоняется.
				new (int nomenclatureId, decimal count, decimal price, decimal discount)[]
				{
					(1, 6m, 10m, 1m)
				},
				// Identification Codes: 1-3 и 4-6 входят в два групповых кода, 7-9 — штучные
				new (bool isInValid, int gtinId)[]
				{
					(false, 1), (false, 1), (false, 1),
					(false, 1), (false, 1), (false, 1),
					(false, 1), (false, 1), (false, 1),
				},
				// Group Codes
				new (int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)[]
				{
					(null, 1, false, new []{ 1, 2, 3 }),
					(null, 1, false, new []{ 4, 5, 6 }),
				},
				// Edo Task Item Identification Codes
				new[]
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9
				});

			var mainFiscalDocument = new EdoFiscalDocument { Index = 0 };
			receiptEdoTask.FiscalDocuments.Add(mainFiscalDocument);

			// Act

			await _forOwnNeedsReceiptEdoTaskHandler.UpdateMarkedFiscalDocuments(receiptEdoTask, mainFiscalDocument, default);

			// Assert

			var inventPositions = receiptEdoTask.FiscalDocuments.SelectMany(x => x.InventPositions).ToList();

			// ровно один групповой код применён — одной объединённой строкой на 3 единицы
			var groupPositions = inventPositions.Where(x => x.GroupCode != null).ToList();
			Assert.Single(groupPositions);
			Assert.Equal(3m, groupPositions[0].Quantity);

			// оставшиеся 3 единицы (из отклонённого группового кода) стали отдельными строками чека
			var individualPositions = inventPositions.Where(x => x.GroupCode == null).ToList();
			Assert.Equal(3, individualPositions.Count);
			Assert.All(individualPositions, x => Assert.Equal(1m, x.Quantity));
			Assert.All(individualPositions, x => Assert.NotNull(x.EdoTaskItem));

			// итоговое количество и стоимость соответствуют исходному заказу
			Assert.Equal(
				receiptEdoTask.FormalEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.Count),
				inventPositions.Sum(x => x.Quantity));

			Assert.Equal(
				receiptEdoTask.FormalEdoRequest.Order.OrderItems
					.Where(x => x.Nomenclature.IsAccountableInTrueMark && x.Count > 0)
					.Sum(x => x.Sum),
				inventPositions.Sum(x => x.Price * x.Quantity - x.DiscountSum));

			// входящие в отклонённый групповой код 3 штучных кода возвращены в пул
			await _trueMarkCodesPool.Received(3).PutCodeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
		}

		private ReceiptEdoTask CreateTestReceiptEdoTaskForTest(
			IEnumerable<(IEnumerable<int> gtinIds, IEnumerable<int> groupGtinIds, bool isAccountableInTrueMark)> nomenclaturesParameters,
			IEnumerable<(int nomenclatureId, decimal count, decimal price, decimal discount)> orderItemsParameters,
			IEnumerable<(bool isValid, int gtinId)> waterIdentificationCodesParameters,
			IEnumerable<(int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds)> waterGroupCodesParameters,
			IEnumerable<int> edoTaskItemIdentificationCodesParameters)
		{
			var orderItemAutoIncrementId = 1;
			var nomenclatureAutoIncrementId = 1;
			var gtinAutoIncrementId = 1;
			var groupGtinAutoIncrementId = 1;
			var carLoadDocumentItemAutoIncrementId = 1;
			var edoTaskItemAutoIncrementId = 1;
			var carLoadDocumentItemTrueMarkProductCodeAutoIncrementId = 1;
			var waterIdentificationCodeAutoIncrementId = 1;
			var waterGroupCodeAutoIncrementId = 1;

			var gtins = new List<GtinEntity>();

			var maxGtinId = nomenclaturesParameters.Max(x => x.gtinIds.Any() ? x.gtinIds.Max() : 0);

			for(var i = 1; i < maxGtinId; i++)
			{
				gtins.Add(CreateGtin(gtinAutoIncrementId++));
			}

			var groupGtins = new List<GroupGtinEntity>();

			var maxGroupGtinId = nomenclaturesParameters.Max(x => x.groupGtinIds.Any() ? x.groupGtinIds.Max() : 0);

			for(var i = 1; i < maxGroupGtinId; i++)
			{
				groupGtins.Add(CreateGroupGtin(groupGtinAutoIncrementId++));
			}

			var nomenclatures = new List<NomenclatureEntity>();

			foreach(var (gtinIds, groupGtinIds, isAccountableInTrueMark) in nomenclaturesParameters)
			{
				nomenclatures.Add(CreateNomenclature(
					nomenclatureAutoIncrementId++,
					isAccountableInTrueMark,
					gtins.Where(x => gtinIds.Contains(x.Id)),
					groupGtins.Where(x => groupGtinIds.Contains(x.Id))));
			}

			var orderItems = new List<OrderItemEntity>();

			foreach(var (nomenclatureId, count, price, discount) in orderItemsParameters)
			{
				orderItems.Add(CreateOrderItem(orderItemAutoIncrementId++, nomenclatures[nomenclatureId - 1], count, price, discount));
			}

			var order = CreateOrderForTest(orderItems);

			var receiptEdoTask = new ReceiptEdoTask
			{
				FormalEdoRequest = new PrimaryEdoRequest
				{
					Order = order
				},
			};

			var trueMarkWaterIdentificationCodes = new List<TrueMarkWaterIdentificationCode>();

			foreach((var isValid, var gtinId) in waterIdentificationCodesParameters)
			{
				trueMarkWaterIdentificationCodes.Add(CreateTrueMarkWaterIdentificationCode(waterIdentificationCodeAutoIncrementId++, CreateNewGtinCode(gtinId), isValid));
			}

			foreach(var waterIdentificationCodeId in edoTaskItemIdentificationCodesParameters)
			{
				receiptEdoTask.Items.Add(CreateEdoTaskItem(
					edoTaskItemAutoIncrementId++,
					carLoadDocumentItemTrueMarkProductCodeAutoIncrementId++,
					carLoadDocumentItemAutoIncrementId++,
					order.Id,
					trueMarkWaterIdentificationCodes[waterIdentificationCodeId - 1]));
			}

			var trueMarkWaterGroupCodes = new List<TrueMarkWaterGroupCode>();

			var groupCodesCounter = 1;

			foreach((int? parentWaterCodeId, int groupGtinId, bool isInValid, IEnumerable<int> childWaterCodeIds) in waterGroupCodesParameters.OrderBy(x => x.parentWaterCodeId))
			{
				var groupCode = new TrueMarkWaterGroupCode
				{
					Id = waterGroupCodeAutoIncrementId++,
					IsInvalid = isInValid,
					GTIN = CreateNewGroupGtinCode(groupGtinId),
				};

				if(parentWaterCodeId != null)
				{
					trueMarkWaterGroupCodes[parentWaterCodeId.Value - 1].AddInnerGroupCode(groupCode);
				}

				foreach(var childId in childWaterCodeIds)
				{
					var currentWaterIdentificationCode = trueMarkWaterIdentificationCodes[childId - 1];

					if(groupCode.GTIN == null)
					{
						groupCode.GTIN = currentWaterIdentificationCode.Gtin;
					}

					groupCode.RawCode ??= $"{groupCode.GTIN}Raw{groupCodesCounter++}";

					groupCode.AddInnerWaterCode(currentWaterIdentificationCode);
				}

				trueMarkWaterGroupCodes.Add(groupCode);
				_waterGroupCodeRepository.Data.Add(groupCode);
			}

			return receiptEdoTask;
		}

		private EdoTaskItem CreateEdoTaskItem(
			int edoTaskItemAutoIncrementId,
			int carLoadDocumentItemTrueMarkProductCodeAutoIncrementId,
			int carLoadDocumentItemAutoIncrementId,
			int orderId,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode)
		{
			var sourceCode = trueMarkWaterIdentificationCode;

			return new EdoTaskItem
			{
				Id = edoTaskItemAutoIncrementId,
				ProductCode = new CarLoadDocumentItemTrueMarkProductCode
				{
					Id = carLoadDocumentItemTrueMarkProductCodeAutoIncrementId,
					CarLoadDocumentItem = new CarLoadDocumentItemEntity
					{
						Id = carLoadDocumentItemAutoIncrementId,
						OrderId = orderId
					},
					SourceCode = sourceCode
				}
			};
		}

		private GtinEntity CreateGtin(int id)
		{
			return new GtinEntity
			{
				Id = id,
				GtinNumber = CreateNewGtinCode(id)
			};
		}

		private GroupGtinEntity CreateGroupGtin(int id)
		{
			return new GroupGtinEntity
			{
				Id = id,
				GtinNumber = CreateNewGroupGtinCode(id)
			};
		}

		private TrueMarkWaterIdentificationCode CreateTrueMarkWaterIdentificationCode(int id, string gtin, bool isInvalid)
		{
			return new TrueMarkWaterIdentificationCode
			{
				Id = id,
				Gtin = gtin,
				SerialNumber = CreateNewGtinSerial(id),
				CheckCode = CreateNewGtinCheckCode(id),
				IsInvalid = isInvalid
			};
		}

		private string CreateNewGtinCode(int id) => $"Gtin#Test#{id}";
		private string CreateNewGroupGtinCode(int id) => $"GroupGtin#Test#{id}";
		private string CreateNewGtinSerial(int id) => $"Gtin#Test.Serial#{id}";
		private string CreateNewGtinCheckCode(int id) => $"Gtin#Test.CheckCode#{id}";

		private OrderEntity CreateOrderForTest(IEnumerable<OrderItemEntity> orderItems)
		{
			var order = new OrderEntity
			{
				Id = 1,
				Contract = new CounterpartyContractEntity
				{
					Organization = new OrganizationEntity { INN = "0000000000" }
				}
			};

			foreach(var orderItem in orderItems)
			{
				orderItem.Order = order;

				order.OrderItems.Add(orderItem);
			}

			return order;
		}

		private NomenclatureEntity CreateNomenclature(int id, bool isAccountableInTrueMark, IEnumerable<GtinEntity> gtins, IEnumerable<GroupGtinEntity> groupGtins)
		{
			var nomenclature = new NomenclatureEntity
			{
				Id = id,
				IsAccountableInTrueMark = isAccountableInTrueMark,
				Gtins = new ObservableList<GtinEntity>(),
				GroupGtins = new ObservableList<GroupGtinEntity>()
			};

			foreach(var gtin in gtins)
			{
				gtin.Nomenclature = nomenclature;
				nomenclature.Gtins.Add(gtin);
			}

			foreach(var groupGtin in groupGtins)
			{
				groupGtin.Nomenclature = nomenclature;
				nomenclature.GroupGtins.Add(groupGtin);
			}

			nomenclature.VatRateVersions.Add(new VatRateVersion
			{
				StartDate = new DateTime(2000, 1, 1),
				EndDate = null,
				VatRate = new VatRate { VatRateValue = 0m }
			});

			return nomenclature;
		}

		private OrderItemEntity CreateOrderItem(int id, NomenclatureEntity nomenclature, decimal count, decimal price, decimal discount)
		{
			var orderItem = new OrderItemEntityFixture()
			{
				Id = id,
				Nomenclature = nomenclature
			};

			orderItem.SetCount(count);
			orderItem.SetPrice(price);
			orderItem.SetMoneyDiscount(discount);

			return orderItem;
		}

		private ForOwnNeedsReceiptEdoTaskHandler CreateForOwnNeedsReceiptEdoTaskHandlerFixture(
			IGenericRepository<TrueMarkWaterGroupCode> waterGroupCodeRepository,
			IGenericRepository<TrueMarkProductCode> productCodeRepository = null)
		{
			var logger = Substitute.For<ILogger<ForOwnNeedsReceiptEdoTaskHandler>>();
			var unitOfWork = Substitute.For<IUnitOfWork>();
			var unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
			var edoRepository = Substitute.For<IEdoRepository>();
			var httpClientFactory = Substitute.For<IHttpClientFactory>();
			var edoProblemRegistrar = CreateEdoProblemRegistrarFixture(unitOfWork, unitOfWorkFactory);
			var edoTaskValidator = CreateEdoTaskValidatorFixture(unitOfWorkFactory, edoProblemRegistrar);
			var edoTaskTrueMarkCodeCheckerFactory = Substitute.For<EdoTaskItemTrueMarkStatusProviderFactory>(Substitute.For<ITrueMarkApiClient>());
			var transferRequestCreator = CreateTransferRequestCreatorFixture(edoRepository);
			var edoReceiptSettings = Substitute.For<IEdoReceiptSettings>();
			edoReceiptSettings.MaxCodesInReceiptCount.Returns(1000);
			var localCodesValidator = CreateTrueMarkTaskCodesValidatorFixture(edoRepository, Substitute.For<ITrueMarkApiClient>());
			var tag1260Checker = CreateTag1260CheckerFixture(httpClientFactory);
			var trueMarkCodeRepository = Substitute.For<ITrueMarkCodeRepository>();
			trueMarkCodeRepository
				.GetGroupCode(Arg.Any<int>(), Arg.Any<CancellationToken>())
				.Returns(callInfo => Task.FromResult(
					_waterGroupCodeRepository.Data.FirstOrDefault(x => x.Id == (int)callInfo[0])));
			var bus = Substitute.For<IBus>();
			var edoCancellationService = new EdoCancellationService(
				Substitute.For<ILogger<EdoCancellationService>>(),
				unitOfWork,
				Substitute.For<IEdoCancellationValidator>(),
				edoProblemRegistrar,
				Substitute.For<IPublishEndpoint>());
			var trueMarkWaterCodeService = Substitute.For<ITrueMarkWaterCodeService>();
			_trueMarkCodesPool = Substitute.For<ReceiptTrueMarkCodesPool>(unitOfWork);
			var saveCodesService = new SaveCodesService(
				Substitute.For<ILogger<SaveCodesService>>(),
				_trueMarkCodesPool);

			return new ForOwnNeedsReceiptEdoTaskHandler(
				logger,
				unitOfWork,
				edoTaskValidator,
				edoProblemRegistrar,
				edoTaskTrueMarkCodeCheckerFactory,
				transferRequestCreator,
				edoRepository,
				edoReceiptSettings,
				localCodesValidator,
				_trueMarkCodesPool,
				Substitute.For<ITrueMarkCodesPoolCodeProvider>(),
				tag1260Checker,
				trueMarkCodeRepository,
				productCodeRepository ?? Substitute.For<IGenericRepository<TrueMarkProductCode>>(),
				Substitute.For<IEdoOrderContactProvider>(),
				saveCodesService,
				Substitute.For<IOrganizationSettings>(),
				bus,
				edoCancellationService,
				trueMarkWaterCodeService);
		}

		private EdoTaskValidator CreateEdoTaskValidatorFixture(IUnitOfWorkFactory unitOfWorkFactory, EdoProblemRegistrar edoProblemRegistrar)
		{
			var logger = Substitute.For<ILogger<EdoTaskValidator>>();
			var edoTaskValidatorsProvider = CreateEdoTaskValidatorsProviderFixture(CreateEdoTaskValidatorsPersisterFixture(unitOfWorkFactory));
			var serviceProvider = Substitute.For<IServiceProvider>();

			return new EdoTaskValidator(logger, edoTaskValidatorsProvider, serviceProvider, edoProblemRegistrar);
		}

		private EdoProblemRegistrar CreateEdoProblemRegistrarFixture(IUnitOfWork unitOfWork, IUnitOfWorkFactory unitOfWorkFactory)
		{
			var customSourcesPersister = CreateEdoTaskCustomSourcesPersisterFixture(unitOfWorkFactory);

			var exceptionSourcesPersister = CreateEdoTaskExceptionSourcesPersisterFixture(unitOfWorkFactory);

			return new EdoProblemRegistrar(unitOfWork, unitOfWorkFactory, customSourcesPersister, exceptionSourcesPersister);
		}

		private EdoTaskCustomSourcesPersister CreateEdoTaskCustomSourcesPersisterFixture(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new EdoTaskCustomSourcesPersister(unitOfWorkFactory, Enumerable.Empty<EdoTaskProblemCustomSource>());
		}

		private EdoTaskExceptionSourcesPersister CreateEdoTaskExceptionSourcesPersisterFixture(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new EdoTaskExceptionSourcesPersister(unitOfWorkFactory, Enumerable.Empty<EdoTaskProblemExceptionSource>());
		}

		private EdoTaskValidatorsProvider CreateEdoTaskValidatorsProviderFixture(EdoTaskValidatorsPersister edoTaskValidatorsPersister)
		{
			return new EdoTaskValidatorsProvider(edoTaskValidatorsPersister);
		}

		private EdoTaskValidatorsPersister CreateEdoTaskValidatorsPersisterFixture(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new EdoTaskValidatorsPersister(unitOfWorkFactory, Enumerable.Empty<IEdoTaskValidator>());
		}

		private TransferRequestCreator CreateTransferRequestCreatorFixture(IEdoRepository edoRepository)
		{
			return new TransferRequestCreator(edoRepository);
		}

		private TrueMarkTaskCodesValidator CreateTrueMarkTaskCodesValidatorFixture(IEdoRepository edoRepository, ITrueMarkApiClient trueMarkApiClient)
		{
			return new TrueMarkTaskCodesValidator(edoRepository, trueMarkApiClient);
		}

		private TrueMarkCodesPool CreateTrueMarkCodesPoolFixture(IUnitOfWork unitOfWork)
		{
			return new TrueMarkCodesPoolFixture(unitOfWork);
		}

		private Tag1260Checker CreateTag1260CheckerFixture(IHttpClientFactory httpClientFactory)
		{
			return new Tag1260Checker(httpClientFactory);
		}

		[Fact]
		public void CheckProductCodesForDuplicatesAndUpdateIfNeed_ShouldUpdateProductCode_WhenDuplicatesExist()
		{
			// Arrange
			var productCodes = new List<TrueMarkProductCode>
			{
				new RouteListItemTrueMarkProductCode { Id = 1, SourceCode = new TrueMarkWaterIdentificationCode { Id = 11 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11 } },
				new RouteListItemTrueMarkProductCode { Id = 2, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 11 } },
				new RouteListItemTrueMarkProductCode { Id = 3, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 12 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11 } }
			};

			var mockWaterGroupCodeRepository = Substitute.For<IGenericRepository<TrueMarkWaterGroupCode>>();
			var mockProductCodeRepository = Substitute.For<IGenericRepository<TrueMarkProductCode>>();
			
			mockProductCodeRepository
				.Get(Arg.Any<IUnitOfWork>(), Arg.Any<Expression<Func<TrueMarkProductCode, bool>>>(), Arg.Any<int>())
				.Returns(new List<TrueMarkProductCode>()
				{
					new RouteListItemTrueMarkProductCode { Id = 11, SourceCode = new TrueMarkWaterIdentificationCode { Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 11 } },
					new RouteListItemTrueMarkProductCode { Id = 12, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 12 } },
					new RouteListItemTrueMarkProductCode { Id = 13, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 13 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 11 } }
				});


			var handler = CreateForOwnNeedsReceiptEdoTaskHandlerFixture(mockWaterGroupCodeRepository, mockProductCodeRepository);

			// Act
			handler.CheckProductCodesForDuplicatesAndUpdateIfNeed(productCodes);

			// Assert
			Assert.Equal(SourceProductCodeStatus.Problem, productCodes[0].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.Duplicate, productCodes[0].Problem);
			Assert.Equal(2, productCodes[0].DuplicatesCount);
			Assert.Null(productCodes[0].ResultCode);

			Assert.Equal(SourceProductCodeStatus.Problem, productCodes[1].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.Duplicate, productCodes[1].Problem);
			Assert.Equal(2, productCodes[1].DuplicatesCount);
			Assert.Null(productCodes[1].ResultCode);

			Assert.Equal(SourceProductCodeStatus.Problem, productCodes[2].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.Duplicate, productCodes[2].Problem);
			Assert.Equal(1, productCodes[2].DuplicatesCount);
			Assert.Null(productCodes[2].ResultCode);
		}

		[Fact]
		public void CheckProductCodesForDuplicatesAndUpdateIfNeed_ShouldNotUpdateProductCode_WhenNoDuplicatesExist()
		{
			// Arrange
			var productCodes = new List<TrueMarkProductCode>
			{
				new RouteListItemTrueMarkProductCode { Id = 1, SourceCode = new TrueMarkWaterIdentificationCode { Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11} },
				new RouteListItemTrueMarkProductCode { Id = 2, SourceCode = new TrueMarkWaterIdentificationCode { Id = 12 } },
				new RouteListItemTrueMarkProductCode { Id = 3, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 13 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 13 } },
			};

			var mockWaterGroupCodeRepository = Substitute.For<IGenericRepository<TrueMarkWaterGroupCode>>();
			var mockProductCodeRepository = Substitute.For<IGenericRepository<TrueMarkProductCode>>();

			mockProductCodeRepository
				.Get(Arg.Any<IUnitOfWork>(), Arg.Any<Expression<Func<TrueMarkProductCode, bool>>>(), Arg.Any<int>())
				.Returns(new List<TrueMarkProductCode>()
				{
					new RouteListItemTrueMarkProductCode { Id = 1, SourceCode = new TrueMarkWaterIdentificationCode { Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{Id = 11} },
					new RouteListItemTrueMarkProductCode { Id = 12, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 10 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 14 } },
					new RouteListItemTrueMarkProductCode { Id = 13, SourceCode = new TrueMarkWaterIdentificationCode{ Id = 13 }, ResultCode = new TrueMarkWaterIdentificationCode{ Id = 15 } }
				});

			var handler = CreateForOwnNeedsReceiptEdoTaskHandlerFixture(mockWaterGroupCodeRepository, mockProductCodeRepository);

			// Act
			handler.CheckProductCodesForDuplicatesAndUpdateIfNeed(productCodes);

			// Assert
			Assert.Equal(SourceProductCodeStatus.New, productCodes[0].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.None, productCodes[0].Problem);
			Assert.Equal(0, productCodes[0].DuplicatesCount);
			Assert.NotNull(productCodes[0].ResultCode);

			Assert.Equal(SourceProductCodeStatus.New, productCodes[1].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.None, productCodes[1].Problem);
			Assert.Equal(0, productCodes[1].DuplicatesCount);
			Assert.Null(productCodes[1].ResultCode);

			Assert.Equal(SourceProductCodeStatus.New, productCodes[2].SourceCodeStatus);
			Assert.Equal(ProductCodeProblem.None, productCodes[2].Problem);
			Assert.Equal(0, productCodes[2].DuplicatesCount);
			Assert.NotNull(productCodes[2].ResultCode);
		}

		[Fact]
		public void GetProductCodesWithoutConsolidatedIdentificationCodes_ShouldReturnProductCodes_WhichNotContainsConsolidatedIdentificationCodes()
		{
			// Arrange
			var edoTaskItems = new List<EdoTaskItem>
			{
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = 1 },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = 2 }
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null }
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = 3 }
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = null
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentTransportCodeId = 3 },
						ResultCode = null
					}
				},
				new()
				{
					ProductCode = new RouteListItemTrueMarkProductCode
					{
						SourceCode = new TrueMarkWaterIdentificationCode { ParentWaterGroupCodeId = null, ParentTransportCodeId = null },
						ResultCode = new TrueMarkWaterIdentificationCode { ParentTransportCodeId = 3 }
					}
				},
			};

			// Act
			var result = _forOwnNeedsReceiptEdoTaskHandler.GetProductCodesWithoutConsolidatedIdentificationCodes(edoTaskItems);

			// Assert
			Assert.Equal(2, result.Count);
			Assert.Contains(result, x => x.Id == edoTaskItems[1].ProductCode.Id);
			Assert.Contains(result, x => x.Id == edoTaskItems[3].ProductCode.Id);
		}
	}
}
