using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using QS.HistoryLog;
using System;
using System.Text;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;

namespace Vodovoz.Commons
{
	internal class ThemmedDiffFormater : IDiffFormatter
	{
		private readonly string _pangoInsertFormat = $"<span background=\"{GdkColors.SuccessBase.ToHtmlColor()}\">";
		private readonly string _pangoDeleteFormat = $"<span background=\"{GdkColors.DangerBase.ToHtmlColor()}\">";
		private readonly string _pangoChangeFormat = $"<span background=\"{GdkColors.DangerBase.ToHtmlColor()}\">";
		public const string PangoEnd = "</span>";

		public void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff)
		{
			var d = new Differ();
			var differ = new SideBySideFullDiffBuilder(d);
			var diffRes = differ.BuildDiffModel(oldValue, newValue);
			oldDiff = RenderDiffLines(diffRes.OldText);
			newDiff = RenderDiffLines(diffRes.NewText);
		}

		public string RenderDiffLines(DiffPaneModel diffModel)
		{
			StringBuilder result = new StringBuilder();
			foreach(var line in diffModel.Lines)
			{
				if(line.Type == ChangeType.Deleted)
				{
					result.AppendLine(_pangoDeleteFormat + line.Text + PangoEnd);
				}
				else if(line.Type == ChangeType.Inserted)
				{
					result.AppendLine(_pangoInsertFormat + line.Text + PangoEnd);
				}
				else if(line.Type == ChangeType.Unchanged)
				{
					result.AppendLine(line.Text);
				}
				else if(line.Type == ChangeType.Modified)
				{
					result.AppendLine(RenderDiffWords(line));
				}
				else if(line.Type == ChangeType.Imaginary)
				{
					result.AppendLine();
				}
			}
			return result.ToString().TrimEnd(Environment.NewLine.ToCharArray());
		}

		private string RenderDiffWords(DiffPiece line)
		{
			StringBuilder result = new StringBuilder();
			ChangeType lastAction = ChangeType.Unchanged;
			foreach(var word in line.SubPieces)
			{
				if(word.Type == ChangeType.Imaginary)
				{
					continue;
				}

				if(word.Type == ChangeType.Modified)
				{
					result.Append(RenderDiffCharacter(word, ref lastAction));
				}
				else
				{
					if(lastAction != ChangeType.Unchanged && lastAction != word.Type)
					{
						result.Append(PangoEnd);
					}

					result.Append(StartSpan(word.Type, lastAction)).Append(word.Text);
					lastAction = word.Type;
				}
			}
			if(lastAction != ChangeType.Unchanged)
			{
				result.Append(PangoEnd);
			}

			return result.ToString();
		}

		private string RenderDiffCharacter(DiffPiece word, ref ChangeType lastAction)
		{
			StringBuilder result = new StringBuilder();
			foreach(var characters in word.SubPieces)
			{
				if(characters.Type == ChangeType.Imaginary)
				{
					continue;
				}

				if(lastAction != ChangeType.Unchanged && lastAction != characters.Type)
				{
					result.Append(PangoEnd);
				}

				result.Append(StartSpan(characters.Type, lastAction)).Append(characters.Text);
				lastAction = characters.Type;
			}
			return result.ToString();
		}

		private string StartSpan(ChangeType current, ChangeType last)
		{
			if(current != last)
			{
				if(current == ChangeType.Deleted)
				{
					return _pangoDeleteFormat;
				}
				else if(current == ChangeType.Inserted)
				{
					return _pangoInsertFormat;
				}
			}
			return string.Empty;
		}
	}
}
