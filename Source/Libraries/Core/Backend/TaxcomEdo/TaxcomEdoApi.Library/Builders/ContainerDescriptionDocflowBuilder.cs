using System;
using DateTimeHelpers;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdoApi.Library.Models.Containers;

namespace TaxcomEdoApi.Library.Builders
{
	public class ContainerDescriptionDocflowBuilder
	{
		private ContainerDescriptionDocFlow _docflow = new ContainerDescriptionDocFlow();
		
		/// <summary>
		/// Установка идентификатора ДО(документооборота)
		/// </summary>
		/// <param name="id">Идентификатор ДО</param>
		/// <returns></returns>
		public ContainerDescriptionDocflowBuilder Id(Guid? id)
		{
			if(id.HasValue)
			{
				_docflow.Id = id.Value.ToString();
			}
				
			return this;
		}

		public ContainerDescriptionDocflowBuilder Status(DocflowStatus status)
		{
			if(status != DocflowStatus.Unknown)
			{
				_docflow.Status = status.ToContainerDocFlowStatus();
			}

			return this;
		}
		
		public ContainerDescriptionDocflowBuilder InternalStatus(DocflowInternalStatus? internalStatus)
		{
			if(internalStatus.HasValue && internalStatus.Value != DocflowInternalStatus.Unknown)
			{
				_docflow.InternalStatus = internalStatus.Value.ToContainerDocFlowInternalStatus();
			}
			
			return this;
		}
		
		public ContainerDescriptionDocflowBuilder ErrorType(ErrorType? errorType)
		{
			if(errorType.HasValue)
			{
				_docflow.ErrorType = errorType.Value.ToDocFlowErrorType();
			}
			
			return this;
		}
		
		public ContainerDescriptionDocflowBuilder ErrorDescription(string errorDescription)
		{
			if(!string.IsNullOrWhiteSpace(errorDescription))
			{
				_docflow.ErrorDescription = errorDescription;
			}
			
			return this;
		}
		
		public ContainerDescriptionDocflowBuilder StatusChangeDateTime(DateTime? statusChangeDateTime)
		{
			if(statusChangeDateTime.HasValue)
			{
				_docflow.StatusChangeDateTime = statusChangeDateTime.Value.ToEdoMetaFileDateTimeString();
			}
			
			return this;
		}
		
		public ContainerDescriptionDocflowBuilder CreatedAt(DateTime? createdAt)
		{
			if(createdAt.HasValue)
			{
				InitializeDescriptionIfNull();
				_docflow.Description.Date = createdAt.Value.ToEdoMetaFileDateTimeString();
			}
			
			return this;
		}
		
		public ContainerDescriptionDocflowBuilder Title(string title)
		{
			if(string.IsNullOrWhiteSpace(title))
			{
				InitializeDescriptionIfNull();
				_docflow.Description.Title = title;
			}
			
			return this;
		}
		
		public ContainerDescriptionDocflowBuilder Comment(string comment)
		{
			if(string.IsNullOrWhiteSpace(comment))
			{
				InitializeDescriptionIfNull();
				_docflow.Description.Comment = comment;
			}
			
			return this;
		}
		
		public ContainerDescriptionDocflowBuilder Documents(ContainerDescriptionDocFlowDocument[] documents)
		{
			if(documents is { Length: > 0 })
			{
				_docflow.Documents = documents;
			}
			
			return this;
		}

		private void InitializeDescriptionIfNull()
		{
			_docflow.Description ??= new Description();
		}

		public ContainerDescriptionDocFlow Build()
		{
			var createdDocflow = _docflow;
			_docflow = new ContainerDescriptionDocFlow();
			
			return createdDocflow;
		}

		public static ContainerDescriptionDocflowBuilder Create() => new ContainerDescriptionDocflowBuilder();
	}
}
