using TrueMark.Contracts;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using System;
using System.Linq;
using Vodovoz.Core.Data.NHibernate.Clients;
using Vodovoz.Core.Data.NHibernate.Mapping.TrueMark;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Data.NHibernate.HibernateMapping.Counterparty;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.Tests
{
	public class TrueMarkMappingTests
	{
		[Fact]
		public void RegisteredTrueMarkProductGroups_MappingType_ShouldBeOneToManyCollection()
		{
			var configuration = Fluently.Configure()
				.Database(SQLiteConfiguration.Standard.InMemory())
				.Mappings(m =>
				{
					m.FluentMappings.Add<CounterpartyMap>();
					m.FluentMappings.Add<CounterpartyTrueMarkProductGroupMap>();
				})
				.BuildConfiguration();

			var classMapping = configuration.GetClassMapping(typeof(CounterpartyEntity));
			var property = classMapping.GetProperty(nameof(CounterpartyEntity.RegisteredTrueMarkProductGroups));

			Assert.NotNull(property);
			Assert.True(property.Value is global::NHibernate.Mapping.Collection collection && collection.IsOneToMany);

			var childMapping = configuration.GetClassMapping(typeof(CounterpartyTrueMarkProductGroup));
			var productTypeProp = childMapping.GetProperty(nameof(CounterpartyTrueMarkProductGroup.ProductGroup));
			Assert.NotNull(productTypeProp);
			Assert.Contains("GenericEnumMapper", productTypeProp.Type.GetType().Name);
		}

		[Fact]
		public void ProductGroup_InTrueMarkCodes_MappingType_ShouldBeStringGenericEnumMapper()
		{
			var configuration = Fluently.Configure()
				.Database(SQLiteConfiguration.Standard.InMemory())
				.Mappings(m =>
				{
					m.FluentMappings.Add<TrueMarkIdentificationCodeMap>();
					m.FluentMappings.Add<StagingTrueMarkCodeMap>();
				})
				.BuildConfiguration();

			var codeMapping = configuration.GetClassMapping(typeof(TrueMarkIdentificationCode));
			var property = codeMapping.GetProperty(nameof(TrueMarkIdentificationCode.ProductGroup));

			Assert.NotNull(property);
			Assert.Contains("GenericEnumMapper", property.Type.GetType().Name);

			var stagingMapping = configuration.GetClassMapping(typeof(StagingTrueMarkCode));
			var stagingProperty = stagingMapping.GetProperty(nameof(StagingTrueMarkCode.ProductGroup));

			Assert.NotNull(stagingProperty);
			Assert.Contains("GenericEnumMapper", stagingProperty.Type.GetType().Name);
		}

		[Fact]
		public void ProductGroup_InTrueMarkApiDocuments_MappingType_ShouldBeStringGenericEnumMapper()
		{
			var configuration = Fluently.Configure()
				.Database(SQLiteConfiguration.Standard.InMemory())
				.Mappings(m =>
				{
					m.FluentMappings.Add<Vodovoz.Core.Data.NHibernate.Mapping.Orders.TrueMarkDocumentMap>();
				})
				.BuildConfiguration();

			var docMapping = configuration.GetClassMapping(typeof(Vodovoz.Core.Domain.Orders.TrueMarkDocument));
			var property = docMapping.GetProperty(nameof(Vodovoz.Core.Domain.Orders.TrueMarkDocument.ProductGroup));

			Assert.NotNull(property);
			Assert.Contains("GenericEnumMapper", property.Type.GetType().Name);
		}

		[Fact]
		public void CounterpartyEntity_UpdateRegisteredTrueMarkProductGroups_DiffBehavior_AddsAndRemovesCorrectly()
		{
			var counterparty = new CounterpartyEntity();

			// 1. Initial set: Water
			counterparty.UpdateRegisteredTrueMarkProductGroups(new[] { TrueMarkProductGroup.Water });
			Assert.Single(counterparty.RegisteredTrueMarkProductGroups);
			var waterItem = counterparty.RegisteredTrueMarkProductGroups[0];
			Assert.Equal(TrueMarkProductGroup.Water, waterItem.ProductGroup);
			Assert.Same(counterparty, waterItem.Counterparty);

			// 2. Add PetFood: existing Water instance should be preserved (Assert.Same)
			counterparty.UpdateRegisteredTrueMarkProductGroups(new[] { TrueMarkProductGroup.Water, TrueMarkProductGroup.PetFood });
			Assert.Equal(2, counterparty.RegisteredTrueMarkProductGroups.Count);
			Assert.Same(waterItem, counterparty.RegisteredTrueMarkProductGroups.First(x => x.ProductGroup == TrueMarkProductGroup.Water));
			var petFoodItem = counterparty.RegisteredTrueMarkProductGroups.First(x => x.ProductGroup == TrueMarkProductGroup.PetFood);
			Assert.Same(counterparty, petFoodItem.Counterparty);

			// 3. Remove Water, keep PetFood: PetFood instance should be preserved (Assert.Same)
			counterparty.UpdateRegisteredTrueMarkProductGroups(new[] { TrueMarkProductGroup.PetFood });
			Assert.Single(counterparty.RegisteredTrueMarkProductGroups);
			Assert.Same(petFoodItem, counterparty.RegisteredTrueMarkProductGroups[0]);

			// 4. Pass duplicates [Water, Water]: should result in single item
			counterparty.UpdateRegisteredTrueMarkProductGroups(new[] { TrueMarkProductGroup.Water, TrueMarkProductGroup.Water });
			Assert.Single(counterparty.RegisteredTrueMarkProductGroups);
			Assert.Equal(TrueMarkProductGroup.Water, counterparty.RegisteredTrueMarkProductGroups[0].ProductGroup);

			// 5. Pass null or empty: should clear collection without exceptions
			counterparty.UpdateRegisteredTrueMarkProductGroups(null);
			Assert.Empty(counterparty.RegisteredTrueMarkProductGroups);

			// 6. Test AddRegisteredTrueMarkProductGroup / RemoveRegisteredTrueMarkProductGroup
			counterparty.AddRegisteredTrueMarkProductGroup(TrueMarkProductGroup.PetFood);
			Assert.Single(counterparty.RegisteredTrueMarkProductGroups);
			counterparty.AddRegisteredTrueMarkProductGroup(TrueMarkProductGroup.PetFood); // duplicate
			Assert.Single(counterparty.RegisteredTrueMarkProductGroups);
			counterparty.RemoveRegisteredTrueMarkProductGroup(TrueMarkProductGroup.PetFood);
			Assert.Empty(counterparty.RegisteredTrueMarkProductGroups);
		}

		[Theory]
		[InlineData(RegistrationInChestnyZnakStatus.Unknown, "Water", TrueMarkProductGroup.Water, false)]
		[InlineData(RegistrationInChestnyZnakStatus.InProcess, "Water", TrueMarkProductGroup.Water, false)]
		[InlineData(RegistrationInChestnyZnakStatus.Blocked, "Water", TrueMarkProductGroup.Water, false)]
		[InlineData(RegistrationInChestnyZnakStatus.Registered, "", TrueMarkProductGroup.Water, false)]
		[InlineData(RegistrationInChestnyZnakStatus.Registered, "Water", TrueMarkProductGroup.Water, true)]
		[InlineData(RegistrationInChestnyZnakStatus.Registered, "Water", TrueMarkProductGroup.PetFood, false)]
		[InlineData(RegistrationInChestnyZnakStatus.Registered, "PetFood", TrueMarkProductGroup.PetFood, true)]
		[InlineData(RegistrationInChestnyZnakStatus.Registered, "Water,PetFood", TrueMarkProductGroup.Water, true)]
		[InlineData(RegistrationInChestnyZnakStatus.Registered, "Water,PetFood", TrueMarkProductGroup.PetFood, true)]
		public void IsRegisteredInTrueMarkForProductGroup_ChecksCorrectly(
			RegistrationInChestnyZnakStatus status,
			string registeredGroupsCsv,
			TrueMarkProductGroup targetGroup,
			bool expected)
		{
			var counterparty = new CounterpartyEntity
			{
				RegistrationInChestnyZnakStatus = status
			};

			var groups = registeredGroupsCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => Enum.Parse<TrueMarkProductGroup>(x.Trim(), true));
			counterparty.UpdateRegisteredTrueMarkProductGroups(groups);

			var actual = counterparty.IsRegisteredInTrueMarkForProductGroup(targetGroup);

			Assert.Equal(expected, actual);
		}
	}
}
