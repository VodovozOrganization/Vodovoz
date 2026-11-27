using System.Linq;
using System.Text.Json;
using TrueMark.Contracts;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.Tests
{
	/// <summary>
	/// Проверки JSON-контракта товарной группы: в ЧЗ и в наших API значения идут строками
	/// нижнего регистра ("water"/"petfood"), а неизвестные группы из ответа ЧЗ не должны ронять разбор.
	/// </summary>
	public class TrueMarkProductGroupJsonTests
	{
		private static readonly JsonSerializerOptions GroupJsonOptions = new()
		{
			Converters = { new TrueMarkProductGroupJsonConverter() }
		};

		[Fact]
		public void Serialize_Water_WritesLowercaseString()
		{
			Assert.Equal("\"water\"", JsonSerializer.Serialize<TrueMarkProductGroup?>(TrueMarkProductGroup.Water, GroupJsonOptions));
			Assert.Equal("\"petfood\"", JsonSerializer.Serialize<TrueMarkProductGroup?>(TrueMarkProductGroup.PetFood, GroupJsonOptions));
		}

		[Fact]
		public void Deserialize_AcceptsBothCases()
		{
			Assert.Equal(TrueMarkProductGroup.Water, JsonSerializer.Deserialize<TrueMarkProductGroup?>("\"water\"", GroupJsonOptions));
			Assert.Equal(TrueMarkProductGroup.Water, JsonSerializer.Deserialize<TrueMarkProductGroup?>("\"Water\"", GroupJsonOptions));
			Assert.Equal(TrueMarkProductGroup.PetFood, JsonSerializer.Deserialize<TrueMarkProductGroup?>("\"PetFood\"", GroupJsonOptions));
		}

		[Fact]
		public void Deserialize_Nullable_HandlesNull()
		{
			Assert.Null(JsonSerializer.Deserialize<TrueMarkProductGroup?>("null", GroupJsonOptions));
			Assert.Equal(TrueMarkProductGroup.PetFood, JsonSerializer.Deserialize<TrueMarkProductGroup?>("\"petfood\"", GroupJsonOptions));
		}

		[Fact]
		public void ProductGroups_SkipsUnknownGroups()
		{
			const string json = "{\"productGroups\":[\"water\",\"tobacco\",\"petfood\",\"water\"]}";

			var dto = JsonSerializer.Deserialize<ParticipantRegistrationDto>(json);

			Assert.NotNull(dto);
			Assert.Equal(
				new[] { TrueMarkProductGroup.Water, TrueMarkProductGroup.PetFood },
				dto.ProductGroups.ToArray());
		}

		[Fact]
		public void ProductInstanceStatus_UnknownGroup_DoesNotThrow_ReturnsNull()
		{
			// ЧЗ может вернуть чужую группу (табак, обувь) — ответ не должен падать.
			var status = JsonSerializer.Deserialize<ProductInstanceStatus>("{\"productGroup\":\"tobacco\"}");

			Assert.NotNull(status);
			Assert.Null(status.ProductGroup);
		}
	}
}
