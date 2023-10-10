using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mango.Core.Codes
{
	public class CodeDescriptionProvider : IResultCodeDescriptionProvider
	{
		private Dictionary<int, CodeClass> _classes { get; set; } = new Dictionary<int, CodeClass>();

		public CodeDescriptionProvider(string filePath)
		{
			LoadCodes(filePath);
		}

		private void LoadCodes(string path)
		{
			var lines = File.ReadLines(path);
			foreach(var line in lines)
			{
				if(string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				if(line.Trim().StartsWith("#"))
				{
					continue;
				}

				var lineParts = line.Split('|');
				if(lineParts.Count() > 2)
				{
					throw new InvalidOperationException("Некорректный формат справочника. Строка имеет более 2 колонок");
				}

				var codeValue = lineParts[0];
				var description = lineParts[1];

				AddCode(codeValue, description);
			}
		}

		private void AddCode(string codeValue, string description)
		{
			if(string.IsNullOrWhiteSpace(description))
			{
				throw new ArgumentException($"Для кода {codeValue} отсутствует описание");
			}

			var codeClasses = codeValue.Select(x => '0' - x).ToList();
			if(!codeClasses.All(x => x >= 0 && x < 10))
			{
				throw new ArgumentException($"Код ({codeValue}) должен состоять только из цифр");
			}

			var codeLevels = codeClasses.Count();
			if(codeLevels <= 1)
			{
				return;
			}
			var firstLevelClass = codeClasses[0];
			if(!_classes.TryGetValue(firstLevelClass, out CodeClass firstLevelCodeClass))
			{
				firstLevelCodeClass = new CodeClass(0, firstLevelClass);
				_classes.Add(firstLevelClass, firstLevelCodeClass);
			}

			firstLevelCodeClass.AddCode(codeClasses, description);
		}

		public string GetCodeDescription(string codeValue)
		{
			var codeClasses = codeValue.Select(x => '0' - x).ToList();
			var codeLevels = codeClasses.Count();
			if(codeLevels <= 1)
			{
				return "";
			}

			if(_classes.TryGetValue(codeClasses[0], out CodeClass nextLevelCodeClass))
			{
				return nextLevelCodeClass.GetDescriptionForCode(codeClasses);
			}

			return "";
		}

		private class CodeClass
		{
			/// <summary>
			/// Zero based level
			/// </summary>
			public int Level { get; }
			public int Class { get; }
			public string Description { get; set; }

			public Dictionary<int, CodeClass> Classes { get; set; } = new Dictionary<int, CodeClass>();

			public CodeClass(int level, int codeClass)
			{
				Class = codeClass;
			}

			public void AddCode(IReadOnlyList<int> codeClasses, string description)
			{
				var codeLevels = codeClasses.Count();
				if(codeLevels == 0)
				{
					return;
				}

				if(codeLevels - 1 < Level)
				{
					return;
				}

				var isCurrentClass = !codeClasses.Skip(Level + 1).Any(x => x > 0);
				if(isCurrentClass)
				{
					Description = description;
					return;
				}

				var hasNextLevels = codeLevels >= Level + 1;
				if(!hasNextLevels)
				{
					return;
				}

				var nextLevelClass = codeClasses[Level + 1];

				if(!Classes.TryGetValue(nextLevelClass, out CodeClass nextLevelCodeClass))
				{
					nextLevelCodeClass = new CodeClass(Level + 1, nextLevelClass);
					Classes.Add(nextLevelClass, nextLevelCodeClass);
				}

				nextLevelCodeClass.AddCode(codeClasses, description);
			}

			public string GetDescriptionForCode(IReadOnlyList<int> codeClasses)
			{
				var codeLevels = codeClasses.Count();
				if(codeLevels <= 1)
				{
					return "";
				}

				var hasNextLevels = codeLevels >= Level + 1;
				if(!hasNextLevels)
				{
					return "";
				}

				var nextLevelClass = codeClasses[Level + 1];
				if(Classes.TryGetValue(nextLevelClass, out CodeClass nextLevelCodeClass))
				{
					var nextDescription = nextLevelCodeClass.GetDescriptionForCode(codeClasses);
					if(string.IsNullOrWhiteSpace(nextDescription))
					{
						return Description;
					}

					return $"{Description}. {nextDescription}";
				}

				return Description;
			}
		}
	}
}
