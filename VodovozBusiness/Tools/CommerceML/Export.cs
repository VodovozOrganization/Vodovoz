using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using QSOrmProject;
using Vodovoz.Repository;
using Vodovoz.Tools.CommerceML.Nodes;

namespace Vodovoz.Tools.CommerceML
{
	public class Export
	{
		static public XmlWriterSettings WriterSettings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = System.Text.Encoding.UTF8,
				NewLineChars = "\r\n"
			};			

		public IUnitOfWork UOW { get; private set; }

		public List<string> Errors = new List<string>();

		public Owner DefaultOwner { get; private set; }

		private Root root;

#region Progress

		public Action ProgressUpdated;

		public string CurrentTaskText { get; set; }

		public int CurrentTask = 0;

		public int TotalTasks = 3;

		public void OnProgressPlusOneTask(string text)
		{
			CurrentTaskText = text;
			CurrentTask++;
			ProgressUpdated?.Invoke();
		}

#endregion

		public Export(IUnitOfWork uow )
		{
			UOW = uow;
		}

		public void RunCatalog()
		{
			Errors.Clear();
			CurrentTaskText = "Получение общих объектов";
			ProgressUpdated?.Invoke();

			var org = OrganizationRepository.GetOrganizationByInn(UOW, "7816453294");
			DefaultOwner = new Owner(this, org);

			root = new Root(this);
		}

		public XElement GetXml()
		{
			OnProgressPlusOneTask("Формируем XML");
			return root.ToXml();
		}
	}
}
