using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Vodovoz.Application.Users
{
	/// <summary>
	/// Парсер прав, выданных роли
	/// </summary>
	public class GrantsRoleParser
	{
		private const string _pattern = @"GRANT (\s?(\w+\s?\w+),?)+ ON [`|']?(\*|\w+\W?[^`|'])[`|']?\.[`|']?(\*|\w+)[`|']? TO [`|']?(\w+)[`|']?";
		
		/// <summary>
		/// Словарь прав роли, ключ которого название роли
		/// </summary>
		/// <param name="grants">Строки с привилегиями роли</param>
		/// <returns></returns>
		public IDictionary<string, IDictionary<string, IList<string>>> Parse(IEnumerable<string> grants)
		{
			var result =  new Dictionary<string, IDictionary<string, IList<string>>>();
			
			foreach(var grant in grants)
			{
				Match match = null;
				
				try
				{
					match = Regex.Match(grant, _pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
				}
				catch(RegexMatchTimeoutException ex)
				{
					continue;
				}

				if(!match.Success)
				{
					continue;
				}

				if(!result.ContainsKey(match.Groups[5].Value))
				{
					result.Add(match.Groups[5].Value, new Dictionary<string, IList<string>>());
				}
				
				var privilegesDictionary = result[match.Groups[5].Value];
				var schemaAndTable = $"{match.Groups[3].Value}.{match.Groups[4].Value}";

				if(!privilegesDictionary.ContainsKey(schemaAndTable))
				{
					privilegesDictionary.Add(schemaAndTable, new List<string>());
				}
				
				var privileges = privilegesDictionary[schemaAndTable];

				foreach(var privilege in match.Groups[2].Captures)
				{
					if(privilege is Capture capture)
					{
						privileges.Add(capture.Value);
					}
				}
				
				privilegesDictionary[schemaAndTable] = privileges.OrderBy(x => x).ToList();
			}

			return result;
		}
	}
}
