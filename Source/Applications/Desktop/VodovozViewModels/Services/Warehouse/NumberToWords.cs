using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.Services.Warehouse
{
	/// <summary>
	/// Конвертер целых чисел в русский текст (прописью).
	/// Поддерживает числа до миллиардов. Род — мужской (один, два...).
	/// </summary>
	public static class NumberToWords
	{
		private static readonly string[] _units =
		{
			"ноль", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять",
			"десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать",
			"пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать"
		};

		private static readonly string[] _tens =
		{
			"", "", "двадцать", "тридцать", "сорок", "пятьдесят",
			"шестьдесят", "семьдесят", "восемьдесят", "девяносто"
		};

		private static readonly string[] _hundreds =
		{
			"", "сто", "двести", "триста", "четыреста",
			"пятьсот", "шестьсот", "семьсот", "восемьсот", "девятьсот"
		};

		public static string ToWords(int number)
		{
			if(number == 0)
			{
				return _units[0];
			}

			if(number < 0)
			{
				return "минус " + ToWords(Math.Abs(number));
			}

			var parts = new List<string>();

			var billions = number / 1_000_000_000;
			number %= 1_000_000_000;

			var millions = number / 1_000_000;
			number %= 1_000_000;

			var thousands = number / 1_000;
			number %= 1_000;

			if(billions > 0)
			{
				parts.Add(TripleToWords(billions, GrammaticalGender.Masculine));
				parts.Add(Pluralize(billions, "миллиард", "миллиарда", "миллиардов"));
			}

			if(millions > 0)
			{
				parts.Add(TripleToWords(millions, GrammaticalGender.Masculine));
				parts.Add(Pluralize(millions, "миллион", "миллиона", "миллионов"));
			}

			if(thousands > 0)
			{
				parts.Add(TripleToWords(thousands, GrammaticalGender.Feminine));
				parts.Add(Pluralize(thousands, "тысяча", "тысячи", "тысяч"));
			}

			if(number > 0)
			{
				parts.Add(TripleToWords(number, GrammaticalGender.Masculine));
			}

			return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
		}

		private enum GrammaticalGender
		{
			Masculine,
			Feminine
		}

		private static string TripleToWords(int number, GrammaticalGender gender)
		{
			var parts = new List<string>();

			var hundreds = number / 100;
			var remainder = number % 100;

			if(hundreds > 0)
			{
				parts.Add(_hundreds[hundreds]);
			}

			if(remainder >= 20)
			{
				parts.Add(_tens[remainder / 10]);
				remainder %= 10;
			}

			if(remainder > 0)
			{
				parts.Add(UnitToWords(remainder, gender));
			}

			return string.Join(" ", parts);
		}

		private static string UnitToWords(int unit, GrammaticalGender gender)
		{
			if(gender == GrammaticalGender.Feminine)
			{
				switch(unit)
				{
					case 1: return "одна";
					case 2: return "две";
					default: return _units[unit];
				}
			}

			return _units[unit];
		}

		private static string Pluralize(int number, string one, string few, string many)
		{
			var mod10 = number % 10;
			var mod100 = number % 100;

			if(mod10 == 1 && mod100 != 11)
			{
				return one;
			}

			if(mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20))
			{
				return few;
			}

			return many;
		}
	}
}
