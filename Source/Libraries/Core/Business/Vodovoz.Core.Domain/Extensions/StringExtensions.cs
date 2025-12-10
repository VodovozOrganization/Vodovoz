using System;
using System.Linq;
using Vodovoz.Core.Domain.Users;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class StringExtensions
	{
		public static string FromPascalCaseToSnakeCase(this string sentence)
		{
			var result = string.Empty;

			for(var i = 0; i < sentence.Length; i++)
			{
				if(sentence[i] == char.ToUpper(sentence[i]) && i > 0)
				{
					result += $"_";
				}

				result += char.ToLower(sentence[i]);
			}

			return result;
		}

		public static int[] FromStringToIntArray(this string sentence)
		{
			var splittedValues = sentence.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

			var values = splittedValues
				.Select(x => int.Parse(x.Trim()))
				.ToArray();

			return values;
		}

		public static string GetSubstringAfterSeparator(this string source, char separator)
		{
			for(var i = 0; i < source.Length; i++)
			{
				if(source[i] == separator)
				{
					return source.Substring(++i);
				}
			}

			return null;
		}

		public static PrivilegeType ToPrivilegeType(this string source)
		{
			if(string.IsNullOrWhiteSpace(source) || !source.Contains('.'))
			{
				throw new ArgumentException($"'{source}' is not a valid schema and table for role privilege");
			}
			
			var schemaAndTable = source.Split('.');

			if(schemaAndTable[0] == "*")
			{
				return PrivilegeType.GlobalPrivilege;
			}

			if(schemaAndTable[0] == "mysql")
			{
				return PrivilegeType.SpecialPrivilege;
			}

			if(schemaAndTable[1] != "*")
			{
				return PrivilegeType.TablePrivilege;
			}

			if(schemaAndTable[0] != "*")
			{
				return PrivilegeType.DatabasePrivilege;
			}
			
			throw new ArgumentOutOfRangeException(nameof(source), @"Нельзя подобрать тип привилегии для роли");
		}
	}
}
