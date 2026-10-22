using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Edo.Common.Services;
using Edo.Documents.Services;
using Edo.Problems.Validation.Sources;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QS.DomainModel.UoW;
using Receipt.Dispatcher.Tests.Fixtures;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using VodovozInfrastructure.Cryptography;
using Xunit;

namespace Receipt.Dispatcher.Tests
{
	public class UpdKituDistributionRegressionTests
	{
		[Theory]
		[InlineData(4, 4)]
		[InlineData(4, 3)]
		[InlineData(100, 100)]
		[InlineData(100, 47)]
		public async Task DirectBottlesPreserveTransportOnlyForCompleteShipment(int total, int shipped)
		{
			var f = new Scenario(0, total, Enumerable.Range(1, shipped), true);
			await f.Builder.BuildForOwnNeedUpdDocumentAsync(f.Task, default);
			Assert.All(f.Bottles, x => Assert.Equal(f.Transport.Id, x.ParentTransportCodeId));
			await f.Service.DetachIncompleteTransportCodesAsync(f.Task, default);
			Assert.Equal(shipped, f.Codes.Sum(x => x.Quantity));
			Assert.Equal(shipped, f.Codes.Count(x => x.IndividualCode != null));
			Assert.All(f.Bottles, x => Assert.Equal(shipped == total ? (int?)f.Transport.Id : null, x.ParentTransportCodeId));
		}

		[Theory]
		[InlineData(100, true)]
		[InlineData(48, true)]
		[InlineData(47, true)]
		[InlineData(47, false)]
		public async Task HundredBottlesInFiftyGroups(int shipped, bool hasCheckCode)
		{
			var f = new Scenario(50, 0, Enumerable.Range(1, shipped), hasCheckCode);
			f.UseReplacement(f.Bottles[47]);
			await f.BuildAndDetach();
			Assert.Equal(shipped, f.Codes.Sum(x => x.Quantity));
			Assert.Equal(shipped / 2, f.Codes.Count(x => x.GroupCode != null));
			Assert.Equal(shipped % 2, f.Codes.Count(x => x.IndividualCode != null));
			Assert.All(f.Groups, g => Assert.Equal(shipped == 100 ? (int?)f.Transport.Id : null, g.ParentTransportCodeId));
			Assert.All(f.Bottles, w => Assert.Null(w.ParentTransportCodeId));
			foreach(var group in f.Groups)
			{
				var members = f.Bottles.Skip((group.Id - 1) * 2).Take(2);
				Assert.All(members, w => Assert.Equal(shipped == 47 && group.Id == 24 ? null : (int?)group.Id, w.ParentWaterGroupCodeId));
			}
			if(shipped == 47)
			{
				var unit = Assert.Single(f.Codes.Where(x => x.IndividualCode != null));
				Assert.Same(f.Bottles[hasCheckCode ? 46 : 47], unit.IndividualCode);
				Assert.Null(unit.IndividualCode.ParentWaterGroupCodeId);
				Assert.Null(unit.IndividualCode.ParentTransportCodeId);
				Assert.Equal(1, unit.Quantity);
			}
		}

		[Fact]
		public async Task HundredTaskCodesForFortySevenBottlesAreRejectedBeforeDistribution()
		{
			var f = new Scenario(50, 0, Enumerable.Range(1, 100), true);
			((OrderItemEntityFixture)f.Task.FormalEdoRequest.Order.OrderItems.Single()).SetActualCount(47);
			var services = Substitute.For<IServiceProvider>();
			var factory = new EdoNotificationMessageFactory(Substitute.For<IMD5HexHashFromString>());
			services.GetService(typeof(IEdoNotificationMessageFactory)).Returns(factory);
			var validator = new ExcessCodesEdoValidator();
			Assert.True(validator.IsApplicable(f.Task));
			var result = await validator.ValidateAsync(f.Task, services, default);
			Assert.False(result.IsValid);
			Assert.NotNull(result.Notification);
			Assert.Null(f.Task.Upd);
			Assert.All(f.Groups, g => Assert.Equal(f.Transport.Id, g.ParentTransportCodeId));
		}

		[Theory]
		[InlineData(4)]
		[InlineData(2)]
		public async Task CompleteGroupsRemainGrouped(int shipped)
		{
			var f = new Scenario(2, 0, Enumerable.Range(1, shipped), true);
			await f.BuildAndDetach();
			Assert.Equal(shipped / 2, f.Codes.Count(x => x.GroupCode != null));
			Assert.Equal(shipped, f.Codes.Sum(x => x.Quantity));
			Assert.All(f.Groups, x => Assert.Equal(shipped == 4 ? (int?)f.Transport.Id : null, x.ParentTransportCodeId));
			Assert.All(f.Groups, g => Assert.All(g.InnerWaterCodes, w => Assert.Equal(g.Id, w.ParentWaterGroupCodeId)));
		}

		[Theory]
		[InlineData(true, false)]
		[InlineData(false, false)]
		[InlineData(false, true)]
		public async Task PartialGroupWithCompleteSibling(bool hasCheckCode, bool replacementFromOtherGroup)
		{
			var f = new Scenario(3, 0, new[] { 1, 3, 4 }, hasCheckCode);
			var replacement = f.Bottles[replacementFromOtherGroup ? 4 : 1];
			f.UseReplacement(replacement);
			await f.BuildAndDetach();
			Assert.Equal(3, f.Codes.Sum(x => x.Quantity));
			Assert.Same(f.Groups[1], Assert.Single(f.Codes.Where(x => x.GroupCode != null)).GroupCode);
			var unit = Assert.Single(f.Codes.Where(x => x.IndividualCode != null)).IndividualCode;
			Assert.Same(hasCheckCode ? f.Bottles[0] : replacement, unit);
			Assert.Null(unit.ParentWaterGroupCodeId);
			Assert.Null(unit.ParentTransportCodeId);
			Assert.All(f.Groups[1].InnerWaterCodes, w => Assert.Equal(f.Groups[1].Id, w.ParentWaterGroupCodeId));
			Assert.All(f.Groups, g => Assert.Null(g.ParentTransportCodeId));
			Assert.All(f.Groups[(hasCheckCode || !replacementFromOtherGroup) ? 0 : 2].InnerWaterCodes,
				w => Assert.Null(w.ParentWaterGroupCodeId));
		}

		[Theory]
		[InlineData(4)]
		[InlineData(3)]
		public async Task MixedTransportPreservesFullGroupsAndCompleteTransport(int shipped)
		{
			var f = new Scenario(1, 2, Enumerable.Range(1, shipped), true);
			await f.BuildAndDetach();
			Assert.Single(f.Codes.Where(x => x.GroupCode != null));
			Assert.Equal(shipped - 2, f.Codes.Count(x => x.IndividualCode != null));
			Assert.Equal(shipped, f.Codes.Sum(x => x.Quantity));
			Assert.Equal(shipped == 4 ? (int?)f.Transport.Id : null, f.Groups[0].ParentTransportCodeId);
			Assert.All(f.Bottles.Skip(2), w => Assert.Equal(shipped == 4 ? (int?)f.Transport.Id : null, w.ParentTransportCodeId));
			Assert.All(f.Groups[0].InnerWaterCodes, w => Assert.Equal(f.Groups[0].Id, w.ParentWaterGroupCodeId));
		}

		[Fact]
		public async Task Order105From108Uses52GroupsAndOneReplacement()
		{
			var f = new Scenario(54, 0, Enumerable.Range(4, 105), false);
			f.UseReplacement(f.Bottles[0]);
			await f.BuildAndDetach();
			Assert.Equal(105, f.Codes.Sum(x => x.Quantity));
			Assert.Equal(52, f.Codes.Count(x => x.GroupCode != null));
			Assert.All(f.Codes.Where(x => x.GroupCode != null), c =>
			{
				Assert.Equal(2, c.Quantity);
				Assert.Equal(2, f.Bottles.Count(w => w.ParentWaterGroupCodeId == c.GroupCode.Id));
			});
			var unit = Assert.Single(f.Codes.Where(x => x.IndividualCode != null)).IndividualCode;
			Assert.Same(f.Bottles[0], unit);
			Assert.Null(unit.ParentWaterGroupCodeId);
			Assert.Null(unit.ParentTransportCodeId);
			Assert.All(f.Groups, g => Assert.Null(g.ParentTransportCodeId));
		}

		private class Scenario
		{
			public readonly TrueMarkTransportCode Transport = new TrueMarkTransportCode { Id = 1000, RawCode = "transport" };
			public readonly List<TrueMarkWaterGroupCode> Groups = new List<TrueMarkWaterGroupCode>();
			public readonly List<TrueMarkWaterIdentificationCode> Bottles = new List<TrueMarkWaterIdentificationCode>();
			public readonly DocumentEdoTask Task;
			public readonly UpdDocumentBuilder Builder;
			public readonly UpdTransportCodeService Service;
			private readonly ITrueMarkCodesPool _pool = Substitute.For<ITrueMarkCodesPool>();
			private readonly ITrueMarkCodesPoolCodeProvider _provider = Substitute.For<ITrueMarkCodesPoolCodeProvider>();
			public UpdProductCode[] Codes => Task.Upd.InventPositions.SelectMany(x => x.Codes).ToArray();

			public Scenario(int groupCount, int directCount, IEnumerable<int> selectedIds, bool hasCheckCode)
			{
				var ids = selectedIds.ToArray();
				var uow = Substitute.For<IUnitOfWork>();
				var nomenclature = new NomenclatureEntity { Id = 1, IsAccountableInTrueMark = true };
				nomenclature.Gtins.Add(new GtinEntity { Id = 1, GtinNumber = "unit-gtin", Nomenclature = nomenclature });
				nomenclature.GroupGtins.Add(new GroupGtinEntity { Id = 1, GtinNumber = "group-gtin", CodesCount = 2, Nomenclature = nomenclature });
				nomenclature.VatRateVersions.Add(new VatRateVersion { StartDate = new DateTime(2000, 1, 1), VatRate = new VatRate { VatRateValue = 0m } });
				var order = new OrderEntityFixture { Id = 1, Contract = new CounterpartyContractEntity { Organization = new OrganizationEntity { INN = "0000000000" } } };
				var item = new OrderItemEntityFixture { Id = 1, Order = order, Nomenclature = nomenclature };
				item.SetCount(ids.Length);
				item.SetPrice(100);
				order.OrderItems.Add(item);
				Task = new DocumentEdoTask { DocumentType = EdoDocumentType.UPD, FormalEdoRequest = new PrimaryEdoRequest { Order = order } };
				UpdDocumentBuilderFixture.PrepareForUpd(Task);
				var groupRepo = new GenericRepositoryFixture<TrueMarkWaterGroupCode>();
				var transportRepo = new GenericRepositoryFixture<TrueMarkTransportCode>();
				transportRepo.Data.Add(Transport);
				for(var i = 0; i < groupCount; i++)
				{
					var group = new TrueMarkWaterGroupCode { Id = i + 1, GTIN = "group-gtin", RawCode = "group-" + i };
					group.AddInnerWaterCode(NewBottle(hasCheckCode));
					group.AddInnerWaterCode(NewBottle(hasCheckCode));
					Transport.AddInnerGroupCode(group);
					Groups.Add(group);
					groupRepo.Data.Add(group);
				}
				for(var i = 0; i < directCount; i++) Transport.AddInnerWaterCode(NewBottle(hasCheckCode));
				foreach(var id in ids)
					Task.Items.Add(new EdoTaskItem { Id = id, CustomerEdoTask = Task, ProductCode = new AutoTrueMarkProductCode { Id = id, SourceCode = Bottles[id - 1] } });
				var common = new TrueMarkWaterCodeService(Substitute.For<ILogger<TrueMarkWaterCodeService>>(),
					uow, groupRepo, transportRepo, new GenericRepositoryFixture<EdoUpdInventPositionCode>(),
					new GenericRepositoryFixture<FiscalInventPosition>(), new GenericRepositoryFixture<TransferOrderTrueMarkCode>());
				Service = new UpdTransportCodeService(uow, common);
				var repository = Substitute.For<ITrueMarkCodeRepository>();
				repository.GetGroupCode(Arg.Any<int>(), Arg.Any<CancellationToken>())
					.Returns(call => System.Threading.Tasks.Task.FromResult(Groups.Single(x => x.Id == (int)call[0])));
				Builder = UpdDocumentBuilderFixture.CreateBuilder(uow, _pool, repository, _provider, Service);
			}

			public void UseReplacement(TrueMarkWaterIdentificationCode replacement)
			{
				_provider.TakeValidCodesBatchAsync(_pool, Arg.Any<IDictionary<string, int>>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
					.Returns(Result.Success<IDictionary<string, IList<TrueMarkWaterIdentificationCode>>>(
						new Dictionary<string, IList<TrueMarkWaterIdentificationCode>> { [replacement.Gtin] = new[] { replacement } }));
			}

			public async Task BuildAndDetach()
			{
				var validation = await new ExcessCodesEdoValidator().ValidateAsync(Task, null, default);
				Assert.True(validation.IsValid);
				await Builder.BuildForOwnNeedUpdDocumentAsync(Task, default);
				await Service.DetachIncompleteTransportCodesAsync(Task, default);
			}

			private TrueMarkWaterIdentificationCode NewBottle(bool hasCheckCode)
			{
				var bottle = new TrueMarkWaterIdentificationCode { Id = Bottles.Count + 1, Gtin = "unit-gtin", RawCode = "bottle-" + Bottles.Count, CheckCode = hasCheckCode ? "check" : null };
				Bottles.Add(bottle);
				return bottle;
			}
		}
	}
}
