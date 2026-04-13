using System;
using System.Collections.Generic;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Builders;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;

namespace TaxcomEdoApi.Library.Models.Containers
{
	public class ContainerDocflow : IContainerDocflow
	{
		private readonly IList<IContainerDocument> _documents = new List<IContainerDocument>();

		public IEnumerable<IContainerDocument> Documents => _documents;

		public Guid? DocflowId { get; set; }

		public DocflowStatus Status { get; set; }

		public ErrorType? ErrorType { get; set; }

		public string ErrorDescription { get; set; }

		public DateTime? StatusChangeDateTime { get; set; }

		public DateTime? CreationDate { get; set; }

		public string Title { get; set; }

		public string Comment { get; set; }

		public DocflowInternalStatus? InternalStatus { get; set; }

		public void AddDocuments(IEnumerable<IContainerDocument> documents)
		{
			foreach(var document in documents)
			{
				_documents.Add(document);
			}
		}
		
		public void AddDocument(IContainerDocument document)
		{
			_documents.Add(document);
		}

		public ContainerDescriptionDocFlow ToWrapperXml(bool exportCardAsExternalFile)
		{
			var builder =
				ContainerDescriptionDocflowBuilder
					.Create()
					.Id(DocflowId)
					.Status(Status)
					.InternalStatus(InternalStatus)
					.ErrorType(ErrorType)
					.ErrorDescription(ErrorDescription)
					.StatusChangeDateTime(StatusChangeDateTime)
					.CreatedAt(CreationDate)
					.Title(Title)
					.Comment(Comment);

			var docflowDocuments =
				Documents
					.Select(document => document.ToWrapperXml(exportCardAsExternalFile))
					.ToArray();

			builder.Documents(docflowDocuments);
			
			return builder.Build();
		}

		public IList<IContainerDocflow> Split()
		{
			var containerDocflowList = new List<IContainerDocflow>();
			
			foreach(var document in _documents)
			{
				ContainerDocflow containerDocflow = new ContainerDocflow
				{
					DocflowId = DocflowId,
					Status = Status,
					ErrorType = ErrorType,
					ErrorDescription = ErrorDescription,
					StatusChangeDateTime = StatusChangeDateTime,
					CreationDate = CreationDate,
					Title = Title,
					Comment = Comment
				};
				
				containerDocflow._documents.Add(document);
				containerDocflowList.Add(containerDocflow);
			}

			return containerDocflowList;
		}
	}
}
