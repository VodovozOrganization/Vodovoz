using TrueMark.Contracts;
using System.Linq;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Models.TrueMark;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.Tests
{
	public class TrueMarkCodeParsersTests
	{
		private const string Gs = "\u001d";
		private const string GsText = "\\u001d";

		private const string WaterGtin = "04650117240163";
		private const string WaterSerial = "ABCDEFGHIJKLM";
		private const string WaterCheck = "Ujpf";

		private const string PetGtin = "04650117240163";
		private const string PetSerial = "21ABCD";
		private const string PetKey = "0024";
		private const string PetValue = "UjpfAmE6elcVxvtPJHHGl/8ULzQB8YPU/XnOERdyuQ==";

		private readonly TrueMarkWaterCodeParser _water = new TrueMarkWaterCodeParser();
		private readonly TrueMarkPetFoodCodeParser _pet = new TrueMarkPetFoodCodeParser();
		private readonly TrueMarkCodeParser _facade;

		public TrueMarkCodeParsersTests()
		{
			_facade = new TrueMarkCodeParser(new ITrueMarkProductCodeParser[] { _water, _pet });
		}

		private static string WaterCode(string lead = "", string sep = Gs) =>
			$"{lead}01{WaterGtin}21{WaterSerial}{sep}93{WaterCheck}";

		private static string PetShort(string sep = Gs) =>
			$"01{PetGtin}21{PetSerial}{sep}93Ujpf";

		private static string PetLong(string sep = Gs) =>
			$"01{PetGtin}21{PetSerial}{sep}91{PetKey}{sep}92{PetValue}";

		// ---------- Вода: стандартный формат ----------

		[Theory]
		[InlineData("")]
		[InlineData("\u00e8")]
		[InlineData(Gs)]
		[InlineData(GsText)]
		[InlineData("\\u00e8")]
		public void Water_Standard_AcceptsOptionalLeadingFnc1(string lead)
		{
			Assert.True(_water.TryParse(WaterCode(lead), out var code));
			Assert.Equal(WaterGtin, code.Gtin);
			Assert.Equal(WaterSerial, code.SerialNumber);
			Assert.Equal(WaterCheck, code.CheckCode);
		}

		[Theory]
		[InlineData(Gs)]
		[InlineData(GsText)]
		[InlineData("")]
		public void Water_Standard_SeparatorIsOptional(string sep)
		{
			Assert.True(_water.TryParse(WaterCode(sep: sep), out var code));
			Assert.Equal($"{GsText}01{WaterGtin}21{WaterSerial}{GsText}93{WaterCheck}", code.SourceCode);
		}

		[Fact]
		public void Water_Standard_PreservesFnc1SymbolInSourceCode()
		{
			Assert.True(_water.TryParse(WaterCode("\u00e8"), out var code));
			Assert.StartsWith("\\u00e8" + "01", code.SourceCode);
		}

		[Theory]
		[InlineData("\n")]
		[InlineData("\r\n")]
		[InlineData("  ")]
		public void Water_Standard_IgnoresTrailingWhitespace_AndDoesNotLeakItIntoCheckCode(string tail)
		{
			// раньше при "…93Uj\r\n" код проверки получался "Uj\r\n"
			Assert.True(_water.TryParse(WaterCode() + tail, out var code));
			Assert.Equal(WaterCheck, code.CheckCode);

			Assert.True(_water.TryParse($"01{WaterGtin}21{WaterSerial}93Uj{tail}", out var shortCheck) == false
				|| shortCheck.CheckCode.All(c => !char.IsControl(c)));
		}

		[Fact]
		public void Water_Standard_RejectsCommaAsLeadingFnc1()
		{
			Assert.False(_water.TryParse("," + WaterCode(), out _));
		}

		[Fact]
		public void Water_Standard_RejectsNonDigitGtin()
		{
			Assert.False(_water.TryParse($"01ABCDEFGHIJKLMN21{WaterSerial}{Gs}93{WaterCheck}", out _));
		}

		[Fact]
		public void Water_Standard_RejectsControlCharactersInsideFields()
		{
			Assert.False(_water.TryParse($"01{WaterGtin}21ABCDEF\nGHIJKL{Gs}93{WaterCheck}", out _));
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData("garbage")]
		public void Water_Standard_RejectsEmptyAndGarbage(string raw)
		{
			Assert.False(_water.TryParse(raw, out var code));
			Assert.Null(code);
		}

		[Fact]
		public void Water_Bracket_Format()
		{
			Assert.True(_water.TryParse($"(01){WaterGtin}(21){WaterSerial}(93){WaterCheck}", out var code));
			Assert.Equal(WaterSerial, code.SerialNumber);
			Assert.Equal($"{GsText}01{WaterGtin}21{WaterSerial}{GsText}93{WaterCheck}", code.SourceCode);
		}

		// ---------- Вода: 1С ----------

		[Theory]
		[InlineData("|ГС|")]
		[InlineData("_x001d_")]
		[InlineData("_x001D_")]
		[InlineData(GsText)]
		[InlineData(Gs)]
		[InlineData(" ")]
		[InlineData("")]
		public void Water_From1c_SeparatorVariants(string sep)
		{
			Assert.True(_water.TryParseCodeFrom1c(WaterCode(sep: sep), out var code));
			Assert.Equal(WaterSerial, code.SerialNumber);
			Assert.Equal(WaterCheck, code.CheckCode);
		}

		[Fact]
		public void Water_From1c_StripsOuterQuotes()
		{
			Assert.True(_water.TryParseCodeFrom1c("\"" + WaterCode(sep: "|ГС|") + "\"", out var code));
			Assert.Equal(WaterGtin, code.Gtin);
		}

		[Fact]
		public void Water_From1c_RejectsSurroundingText()
		{
			Assert.False(_water.TryParseCodeFrom1c("xx" + WaterCode() + "yy", out _));
		}

		// ---------- Вода: ручной ввод ----------

		[Fact]
		public void Water_Manual_NoSeparators_AllowsSurroundingText()
		{
			var code = _water.ParseCodeFromManual($"код: 01{WaterGtin}21{WaterSerial}93{WaterCheck} (из письма)");
			Assert.NotNull(code);
			Assert.Equal(WaterSerial, code.SerialNumber);
		}

		[Fact]
		public void Water_Manual_ReturnsNullOnFailure()
		{
			Assert.Null(_water.ParseCodeFromManual("garbage"));
			Assert.Null(_water.ParseCodeFromManual(null));
		}

		// ---------- Вода: нечёткий ----------

		[Fact]
		public void Water_Fuzzy_FullCode()
		{
			Assert.True(_water.FuzzyParse(WaterCode(), out var code));
			Assert.Equal(WaterSerial, code.SerialNumber);
			Assert.Equal(WaterGtin, code.Gtin);
			Assert.Equal(WaterCheck, code.CheckCode);
		}

		[Fact]
		public void Water_Fuzzy_SerialOnly()
		{
			Assert.True(_water.FuzzyParse(WaterSerial, out var code));
			Assert.Equal(WaterSerial, code.SerialNumber);
			Assert.Null(code.Gtin);
		}

		[Fact]
		public void Water_Fuzzy_DoesNotTreatCommaAsFnc1()
		{
			Assert.False(_water.FuzzyParse("," + WaterCode(), out _));
		}

		[Fact]
		public void Water_Fuzzy_Empty()
		{
			Assert.False(_water.FuzzyParse(null, out _));
			Assert.False(_water.FuzzyParse(" ", out _));
		}

		// ---------- Корма ----------

		[Theory]
		[InlineData(Gs)]
		[InlineData(GsText)]
		[InlineData("")]
		public void Pet_Short(string sep)
		{
			Assert.True(_pet.TryParse(PetShort(sep), out var code));
			Assert.Equal(PetGtin, code.Gtin);
			Assert.Equal(PetSerial, code.SerialNumber);
			Assert.Equal("93Ujpf", code.CheckCode);
			Assert.Equal($"{GsText}01{PetGtin}21{PetSerial}{GsText}93Ujpf", code.SourceCode);
		}

		[Theory]
		[InlineData(Gs)]
		[InlineData(GsText)]
		[InlineData("")]
		public void Pet_Long(string sep)
		{
			Assert.True(_pet.TryParse(PetLong(sep), out var code));
			Assert.Equal($"91{PetKey}{Gs}92{PetValue}", code.CheckCode);
			Assert.Equal($"{GsText}01{PetGtin}21{PetSerial}{GsText}91{PetKey}{GsText}92{PetValue}", code.SourceCode);
		}

		[Fact]
		public void Pet_Bracket_Formats()
		{
			Assert.True(_pet.TryParse($"(01){PetGtin}(21){PetSerial}(93)Ujpf", out var shortCode));
			Assert.Equal("93Ujpf", shortCode.CheckCode);

			Assert.True(_pet.TryParse($"(01){PetGtin}(21){PetSerial}(91){PetKey}(92){PetValue}", out var longCode));
			Assert.Equal(PetKey, longCode.CheckCode.Substring(2, 4));
		}

		[Fact]
		public void Pet_IgnoresTrailingNewline()
		{
			Assert.True(_pet.TryParse(PetShort() + "\r\n", out var code));
			Assert.Equal("93Ujpf", code.CheckCode);
		}

		[Fact]
		public void Pet_RejectsWrongSerialLength()
		{
			Assert.False(_pet.TryParse($"01{PetGtin}21ABCDE{Gs}93Ujpf", out _));
			Assert.False(_pet.TryParse($"01{PetGtin}21ABCDEFG{Gs}93Ujpf", out _));
		}

		[Theory]
		[InlineData("|ГС|")]
		[InlineData("_x001d_")]
		[InlineData("_x001D_")]
		[InlineData(GsText)]
		[InlineData(Gs)]
		[InlineData("")]
		public void Pet_From1c_SeparatorVariants_Short(string sep)
		{
			// регрессия: раньше |ГС| и _x001d_ заменялись на пробел и код не разбирался
			Assert.True(_pet.TryParseCodeFrom1c(PetShort(sep), out var code));
			Assert.Equal(PetSerial, code.SerialNumber);
			Assert.Equal("93Ujpf", code.CheckCode);
		}

		[Theory]
		[InlineData("|ГС|")]
		[InlineData("_x001d_")]
		[InlineData("")]
		public void Pet_From1c_SeparatorVariants_Long(string sep)
		{
			Assert.True(_pet.TryParseCodeFrom1c(PetLong(sep), out var code));
			Assert.Equal($"91{PetKey}{Gs}92{PetValue}", code.CheckCode);
		}

		[Fact]
		public void Pet_From1c_StripsOuterQuotes()
		{
			Assert.True(_pet.TryParseCodeFrom1c("\"" + PetShort("|ГС|") + "\"", out _));
		}

		[Fact]
		public void Pet_Manual_IsNotSupported()
		{
			Assert.False(((ITrueMarkProductCodeParser)_pet).TryParseManual(PetShort(""), out _));
		}

		[Fact]
		public void Pet_Fuzzy_Variants()
		{
			Assert.True(_pet.FuzzyParse(PetLong(), out var full));
			Assert.Equal(PetSerial, full.SerialNumber);

			Assert.True(_pet.FuzzyParse($"01{PetGtin}21{PetSerial}", out var noCrypto));
			Assert.Equal(PetGtin, noCrypto.Gtin);
			Assert.Null(noCrypto.CheckCode);

			Assert.True(_pet.FuzzyParse(PetSerial, out var serialOnly));
			Assert.Equal(PetSerial, serialOnly.SerialNumber);
			Assert.Null(serialOnly.Gtin);

			Assert.False(_pet.FuzzyParse("abc", out _));
		}

		[Fact]
		public void Pet_Fuzzy_BracketPartial()
		{
			// (01)…(21)… без криптохвоста: TryParse такой код не берёт, поэтому проверяется
			// именно ветка _fuzzyAltIdentificationPattern.
			Assert.True(_pet.FuzzyParse($"(01){PetGtin}(21){PetSerial}", out var partial));
			Assert.Equal(PetGtin, partial.Gtin);
			Assert.Equal(PetSerial, partial.SerialNumber);
			Assert.Null(partial.CheckCode);
		}

		[Fact]
		public void Pet_Fuzzy_RejectsNonDigitFirstChar()
		{
			// Серийник корма начинается с цифры кода страны ЕАЭС, произвольные 6 символов не должны приниматься.
			Assert.False(_pet.FuzzyParse("AB12CD", out _));
		}

		// ---------- TrueMarkCodeHelper.ExtractSerialNumber ----------

		[Fact]
		public void ExtractSerialNumber_StandardGs1()
		{
			Assert.Equal(PetSerial, TrueMarkCodeHelper.ExtractSerialNumber($"01{PetGtin}21{PetSerial}", PetGtin));
			Assert.Equal(WaterSerial, TrueMarkCodeHelper.ExtractSerialNumber($"01{WaterGtin}21{WaterSerial}", WaterGtin));
		}

		[Fact]
		public void ExtractSerialNumber_EmptyOrNull_ReturnsEmpty()
		{
			Assert.Equal(string.Empty, TrueMarkCodeHelper.ExtractSerialNumber(null, PetGtin));
			Assert.Equal(string.Empty, TrueMarkCodeHelper.ExtractSerialNumber("   ", PetGtin));
			Assert.Equal(string.Empty, TrueMarkCodeHelper.ExtractSerialNumber(string.Empty, PetGtin));
		}

		[Fact]
		public void ExtractSerialNumber_GtinWithoutPrefix()
		{
			Assert.Equal(PetSerial, TrueMarkCodeHelper.ExtractSerialNumber($"{PetGtin}21{PetSerial}", PetGtin));
		}

		[Fact]
		public void ExtractSerialNumber_NonGs1WithoutGtin_ReturnsEmpty()
		{
			// Регресс: раньше здесь вызывался Replace("", ""), который бросал ArgumentException.
			Assert.Equal(string.Empty, TrueMarkCodeHelper.ExtractSerialNumber("abc", null));
			Assert.Equal(string.Empty, TrueMarkCodeHelper.ExtractSerialNumber("abc", string.Empty));
			Assert.Equal(string.Empty, TrueMarkCodeHelper.ExtractSerialNumber("0123", null));
		}

		// ---------- Фасад ----------

		[Fact]
		public void Facade_ParsesWaterAndPetFood()
		{
			Assert.True(_facade.TryParse(WaterCode(), out var water));
			Assert.Equal(TrueMarkProductGroup.Water, water.ProductGroup);

			Assert.True(_facade.TryParse(PetShort(), out var petShort));
			Assert.Equal(TrueMarkProductGroup.PetFood, petShort.ProductGroup);

			Assert.True(_facade.TryParse(PetLong(), out var petLong));
			Assert.Equal(TrueMarkProductGroup.PetFood, petLong.ProductGroup);
		}

		[Fact]
		public void Facade_FallsBackToManualInput()
		{
			Assert.True(_facade.TryParse($"01{WaterGtin}21{WaterSerial}93{WaterCheck} лишний текст", out var code));
			Assert.Equal(TrueMarkProductGroup.Water, code.ProductGroup);
		}

		[Fact]
		public void Facade_From1c_RoutesToCorrectGroup()
		{
			Assert.True(_facade.TryParseCodeFrom1c(WaterCode(sep: "|ГС|"), out var water));
			Assert.Equal(TrueMarkProductGroup.Water, water.ProductGroup);

			Assert.True(_facade.TryParseCodeFrom1c(PetShort("|ГС|"), out var pet));
			Assert.Equal(TrueMarkProductGroup.PetFood, pet.ProductGroup);
		}

		[Fact]
		public void Facade_Fuzzy()
		{
			Assert.True(_facade.FuzzyParse(WaterSerial, out var water));
			Assert.Equal(TrueMarkProductGroup.Water, water.ProductGroup);

			Assert.False(_facade.FuzzyParse("", out _));
		}

		[Fact]
		public void Facade_Empty_ReturnsFalseAndNull()
		{
			Assert.False(_facade.TryParse(null, out var a));
			Assert.Null(a);
			Assert.False(_facade.TryParseCodeFrom1c(" ", out var b));
			Assert.Null(b);
			Assert.False(_facade.TryParse("garbage", out var c));
			Assert.Null(c);
		}

		[Fact]
		public void Facade_CanBeBuiltFromParserList_AndRespectsOrder()
		{
			var petOnly = new TrueMarkCodeParser(new ITrueMarkProductCodeParser[] { _pet });
			Assert.False(petOnly.TryParse(WaterCode(), out _));
			Assert.True(petOnly.TryParse(PetShort(), out _));
		}

		[Fact]
		public void Facade_ValidatesArguments()
		{
			Assert.Throws<System.ArgumentNullException>(() => new TrueMarkCodeParser((System.Collections.Generic.IEnumerable<ITrueMarkProductCodeParser>)null));
			Assert.Throws<System.ArgumentException>(() => new TrueMarkCodeParser(new ITrueMarkProductCodeParser[] { _water, null }));
		}

		[Theory]
		[InlineData("123456789012345678", false)]
		[InlineData("12345678901234567890123456", true)]
		[InlineData("0012345678901234567890123456", true)]
		[InlineData("", false)]
		[InlineData(null, false)]
		public void Facade_IsTransportCode_CurrentBehavior(string raw, bool expected)
		{
			Assert.Equal(expected, _facade.IsTransportCode(raw));
		}
	}
}
