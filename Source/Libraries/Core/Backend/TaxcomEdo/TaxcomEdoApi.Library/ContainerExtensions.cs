using System;
using TaxcomEdo.Contracts.Xml.Container;
using TaxcomEdo.Contracts.Xml.Container.Entities.Card;
using TaxcomEdoApi.Library.Models;
using TaxcomEdoApi.Library.Models.Containers;

namespace TaxcomEdoApi.Library
{
	public static class ContainerExtensions
	{
		public static DefinitionTypeName ToDefinitionTypeName(this DocumentType source)
		{
			return source switch
			{
				DocumentType.Account => DefinitionTypeName.Account,
				DocumentType.Contract => DefinitionTypeName.Contract,
				DocumentType.Statement => DefinitionTypeName.Statement,
				DocumentType.ExpInvoiceAndPrimaryAccountingDocumentVendor => DefinitionTypeName.ExpInvoiceAndPrimaryAccountingDocumentVendor,
				DocumentType.ExpInvoiceAndPrimaryAccountingDocumentCustomer => DefinitionTypeName.ExpInvoiceAndPrimaryAccountingDocumentCustomer,
				DocumentType.PrimaryAccountingDocumentCustomer => DefinitionTypeName.PrimaryAccountingDocumentCustomer,
				DocumentType.CancellationOffer => DefinitionTypeName.CancellationOffer,
				DocumentType.CancellationOfferResign => DefinitionTypeName.ReceiveNotification,
				_ => throw new ArgumentOutOfRangeException(nameof(source), $"Документ {source} не поддерживается")
			};
		}
		
		public static ContainerDocFlowStatus ToContainerDocFlowStatus(this DocflowStatus source)
		{
			return source switch
			{
				DocflowStatus.Unknown => ContainerDocFlowStatus.Unknown,
				DocflowStatus.InProgress => ContainerDocFlowStatus.InProgress,
				DocflowStatus.Succeed => ContainerDocFlowStatus.Succeed,
				DocflowStatus.Warning => ContainerDocFlowStatus.Warning,
				DocflowStatus.Error => ContainerDocFlowStatus.Error,
				DocflowStatus.NotStarted => ContainerDocFlowStatus.NotStarted,
				DocflowStatus.CompletedWithDivergences => ContainerDocFlowStatus.CompletedWithDivergences,
				DocflowStatus.NotAccepted => ContainerDocFlowStatus.NotAccepted,
				DocflowStatus.WaitingForCancellation => ContainerDocFlowStatus.WaitingForCancellation,
				DocflowStatus.Cancelled => ContainerDocFlowStatus.Cancelled,
				_ => throw new ArgumentOutOfRangeException(nameof(source), $"Статус ДО {source} не поддерживается")
			};
		}
		
		public static DocflowStatus ToDocflowStatus(this ContainerDocFlowStatus source)
		{
			return source switch
			{
				ContainerDocFlowStatus.Unknown => DocflowStatus.Unknown,
				ContainerDocFlowStatus.InProgress => DocflowStatus.InProgress,
				ContainerDocFlowStatus.Succeed => DocflowStatus.Succeed,
				ContainerDocFlowStatus.Warning => DocflowStatus.Warning,
				ContainerDocFlowStatus.Error => DocflowStatus.Error,
				ContainerDocFlowStatus.NotStarted => DocflowStatus.NotStarted,
				ContainerDocFlowStatus.CompletedWithDivergences => DocflowStatus.CompletedWithDivergences,
				ContainerDocFlowStatus.NotAccepted => DocflowStatus.NotAccepted,
				ContainerDocFlowStatus.WaitingForCancellation => DocflowStatus.WaitingForCancellation,
				ContainerDocFlowStatus.Cancelled => DocflowStatus.Cancelled,
				_ => throw new ArgumentOutOfRangeException(nameof(source), $"Статус ДО {source} не поддерживается")
			};
		}
		
		public static ContainerDocFlowInternalStatus ToContainerDocFlowInternalStatus(this DocflowInternalStatus source)
		{
			return source switch
			{
				DocflowInternalStatus.None => ContainerDocFlowInternalStatus.None,
				DocflowInternalStatus.OnNegotiation => ContainerDocFlowInternalStatus.OnNegotiation,
				DocflowInternalStatus.Negotiated => ContainerDocFlowInternalStatus.Negotiated,
				DocflowInternalStatus.FailNegotiation => ContainerDocFlowInternalStatus.FailNegotiation,
				DocflowInternalStatus.OnSign => ContainerDocFlowInternalStatus.OnSign,
				DocflowInternalStatus.SignedAndSent => ContainerDocFlowInternalStatus.SignedAndSent,
				DocflowInternalStatus.FailSign => ContainerDocFlowInternalStatus.FailSign,
				DocflowInternalStatus.Unknown => ContainerDocFlowInternalStatus.Unknown,
				_ => throw new ArgumentOutOfRangeException(nameof(source), $"Внутренний статус ДО {source} не поддерживается")
			};
		}
		
		public static DocflowInternalStatus ToDocflowInternalStatus(this ContainerDocFlowInternalStatus source)
		{
			return source switch
			{
				ContainerDocFlowInternalStatus.None => DocflowInternalStatus.None,
				ContainerDocFlowInternalStatus.OnNegotiation => DocflowInternalStatus.OnNegotiation,
				ContainerDocFlowInternalStatus.Negotiated => DocflowInternalStatus.Negotiated,
				ContainerDocFlowInternalStatus.FailNegotiation => DocflowInternalStatus.FailNegotiation,
				ContainerDocFlowInternalStatus.OnSign => DocflowInternalStatus.OnSign,
				ContainerDocFlowInternalStatus.SignedAndSent => DocflowInternalStatus.SignedAndSent,
				ContainerDocFlowInternalStatus.FailSign => DocflowInternalStatus.FailSign,
				ContainerDocFlowInternalStatus.Unknown => DocflowInternalStatus.Unknown,
				_ => throw new ArgumentOutOfRangeException(nameof(source), $"Внутренний статус ДО {source} не поддерживается")
			};
		}
		
		public static DocFlowErrorType ToDocFlowErrorType(this ErrorType source)
		{
			return source switch
			{
				ErrorType.ImportFailed => DocFlowErrorType.ImportFailed,
				ErrorType.VerificationError => DocFlowErrorType.VerificationError,
				ErrorType.SignaturesCheckFailed => DocFlowErrorType.SignaturesCheckFailed,
				ErrorType.SendingError => DocFlowErrorType.SendingError,
				ErrorType.DocflowError => DocFlowErrorType.DocflowError,
				ErrorType.Unknown => DocFlowErrorType.Unknown,
				_ => throw new ArgumentOutOfRangeException(nameof(source), $"Ошибка ДО {source} не поддерживается")
			};
		}
		
		public static ErrorType ToErrorType(this DocFlowErrorType source)
		{
			return source switch
			{
				DocFlowErrorType.ImportFailed => ErrorType.ImportFailed,
				DocFlowErrorType.VerificationError => ErrorType.VerificationError,
				DocFlowErrorType.SignaturesCheckFailed => ErrorType.SignaturesCheckFailed,
				DocFlowErrorType.SendingError => ErrorType.SendingError,
				DocFlowErrorType.DocflowError => ErrorType.DocflowError,
				DocFlowErrorType.Unknown => ErrorType.Unknown,
				_ => throw new ArgumentOutOfRangeException(nameof(source), $"Ошибка ДО {source} не поддерживается")
			};
		}
	}
}
