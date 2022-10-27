using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Vodovoz.RDL.Elements;

namespace Vodovoz.Reports.Editing
{
    public class ReportController : /*IValidatableObject,*/ IDisposable
    {
		private Stream _inputStream;
		private XDocument _report;
		private List<IReportModifier> _reportModifiers = new List<IReportModifier>();

		public ReportController(string reportPath)
		{
			_inputStream = new FileStream(reportPath, FileMode.Open, FileAccess.Read);
			_report = XDocument.Load(reportPath);
			XmlWriterSettings = new XmlWriterSettings
			{
				NewLineChars = "\n",
				Indent = true,
				IndentChars = "  "
			};
		}

		public bool RemovingEmptyNamespaces { get; set; } = true;
		public XmlWriterSettings XmlWriterSettings { get; set; }

		public void AddModifier(IReportModifier reportModifier)
		{
			if(reportModifier == null || _reportModifiers.Contains(reportModifier))
			{
				return;
			}
			_reportModifiers.Add(reportModifier);
		}

		public void Modify()
		{
			foreach (var reportModifier in _reportModifiers)
			{
				reportModifier.ApplyChanges(_report);
			}
		}

		public void Save(Stream stream)
		{
			var resultRdl = new StringBuilder();
			using(var writer = XmlWriter.Create(resultRdl, XmlWriterSettings))
			{
				_report.Save(writer);
			}

			if(RemovingEmptyNamespaces)
			{
				resultRdl.Replace("xmlns=\"\"", "");
			}

			var fileWriter = new StreamWriter(stream);
			var result = resultRdl.ToString();
			fileWriter.Write(result);
			fileWriter.Flush();
		}

		/*public IEnumerable<ValidationResult> Validate(ValidationContext validationContext = null)
		{
			var result = new List<ValidationResult>();
			foreach(var reportModifier in _reportModifiers)
			{
				result.AddRange(reportModifier.Validate(validationContext));
			}
			return result;
		}*/

		public void Dispose()
		{
			_inputStream?.Dispose();
		}
	}
}
