using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using static Vodovoz.EntityRepositories.Orders.OrderRepository;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public class ReconciliationOfMutualSettlements
		{
			private ReconciliationOfMutualSettlements(
				string clientInn,
				IList<Domain.Client.Counterparty> counterparties,
				IDictionary<int, OrderDiscrepanciesNode> orderNodes,
				IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> paymentNodes)
			{
				if(string.IsNullOrEmpty(clientInn))
				{
					throw new ArgumentException($"'{nameof(clientInn)}' cannot be null or empty.", nameof(clientInn));
				}

				ClientInn = clientInn;
				Counterparties = counterparties ?? throw new ArgumentNullException(nameof(counterparties));
				OrderNodes = orderNodes ?? throw new ArgumentNullException(nameof(orderNodes));
				PaymentNodes = paymentNodes ?? throw new ArgumentNullException(nameof(paymentNodes));
			}

			public string ClientInn { get; }
			public IList<Domain.Client.Counterparty> Counterparties { get; }
			public IDictionary<int, OrderDiscrepanciesNode> OrderNodes { get; }
			public IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> PaymentNodes { get; }

			public static ReconciliationOfMutualSettlements CreateReconciliationOfMutualSettlementsFromXml(
				string fileName,
				IUnitOfWork unitOfWork,
				IOrderRepository orderRepository,
				IPaymentsRepository paymentsRepository,
				ICounterpartyRepository counterpartyRepository)
			{
				var rowsFromXls = XlsParseHelper.GetRowsFromXls(fileName);

				var reconciliationOfMutualSettlements = 
					CreateReconciliationOfMutualSettlements(rowsFromXls, unitOfWork, orderRepository, paymentsRepository, counterpartyRepository);

				return reconciliationOfMutualSettlements;
			}

			private static ReconciliationOfMutualSettlements CreateReconciliationOfMutualSettlements(
				IList<IList<string>> rows,
				IUnitOfWork unitOfWork,
				IOrderRepository orderRepository,
				IPaymentsRepository paymentsRepository,
				ICounterpartyRepository counterpartyRepository)
			{
				var clientInn = GetClientInnFromFile(rows);

				var counterparties = GetCounterpartiesByInn(clientInn, unitOfWork, counterpartyRepository);

				var orderNodes = GetOrderNodes(unitOfWork, orderRepository, rows, counterparties.Select(c => c.Id).FirstOrDefault());

				var paymentNodes = GetPaymentNodes(unitOfWork, paymentsRepository, clientInn, rows);

				return new ReconciliationOfMutualSettlements(
					clientInn,
					counterparties,
					orderNodes,
					paymentNodes);
			}

			private static string GetClientInnFromFile(IList<IList<string>> rows)
			{
				var clientInn = string.Empty;

				foreach(var rowData in rows)
				{
					if(rowData.Count < 1)
					{
						continue;
					}

					if(rowData.Count > 1 && rowData[0].StartsWith("(Сосновцев"))
					{
						clientInn = XlsParseHelper.ParseClientInnFromString(rowData[1]);
						continue;
					}
				}

				return clientInn;
			}

			private static IList<Domain.Client.Counterparty> GetCounterpartiesByInn(
				string inn, 
				IUnitOfWork unitOfWork, 
				ICounterpartyRepository counterpartyRepository)
			{
				return counterpartyRepository.GetCounterpartiesByINN(unitOfWork, inn);
			}

			#region GetOrderNodes
			private static IDictionary<int, OrderDiscrepanciesNode> GetOrderNodes(
				IUnitOfWork unitOfWork,
				IOrderRepository orderRepository,
				IList<IList<string>> rows,
				int clientId)
			{
				var orderDiscrepanciesNode = GetOrderNodesFromFile(rows);

				var allocations = GetAllocationsFromDatabase(unitOfWork, orderRepository, clientId, orderDiscrepanciesNode.Keys.ToList());

				MatchOrdersNodesFromFileWithDatabase(allocations, ref orderDiscrepanciesNode);

				return orderDiscrepanciesNode;
			}

			private static IDictionary<int, OrderDiscrepanciesNode> GetOrderNodesFromFile(IList<IList<string>> rows)
			{
				var orderNodes = new Dictionary<int, OrderDiscrepanciesNode>();

				foreach (var rowData in rows)
				{
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[1].StartsWith("Продажа"))
					{
						var orderNode = CreateOrderNode(rowData);

						orderNodes.Add(orderNode.OrderId, orderNode);
					}
				}

				return orderNodes;
			}

			private static IList<OrderWithAllocation> GetAllocationsFromDatabase(IUnitOfWork unitOfWork, IOrderRepository orderRepository, int clientId, IList<int> orderIds)
			{
				var allocations = orderRepository.GetOrdersWithAllocationsOnDay(unitOfWork, orderIds);
				var ordersMissingFromDocument = orderRepository.GetOrdersWithAllocationsOnDay2(unitOfWork, clientId, orderIds);

				return allocations.Concat(ordersMissingFromDocument).ToList();
			}

			private static void MatchOrdersNodesFromFileWithDatabase(
				IList<OrderWithAllocation> allocations,
				ref IDictionary<int, OrderDiscrepanciesNode> orderDiscrepanciesNode)
			{
				foreach(var allocation in allocations)
				{
					if(orderDiscrepanciesNode.TryGetValue(allocation.OrderId, out var node))
					{
						node.ProgramOrderSum = allocation.OrderSum;
						node.AllocatedSum = allocation.OrderAllocation;
						node.OrderDeliveryDate = allocation.OrderDeliveryDate;
						node.OrderStatus = allocation.OrderStatus;
						node.OrderPaymentStatus = allocation.OrderPaymentStatus;
						node.IsMissingFromDocument = allocation.IsMissingFromDocument;
					}
					else
					{
						orderDiscrepanciesNode.Add(
							allocation.OrderId,
							new OrderDiscrepanciesNode
							{
								OrderId = allocation.OrderId,
								AllocatedSum = allocation.OrderAllocation,
								ProgramOrderSum = allocation.OrderSum,
								OrderDeliveryDate = allocation.OrderDeliveryDate,
								OrderStatus = allocation.OrderStatus,
								OrderPaymentStatus = allocation.OrderPaymentStatus,
								IsMissingFromDocument = allocation.IsMissingFromDocument
							});
					}
				}
			}
			#endregion GetOrderNodes

			#region GetGetPaymentNodes
			private static IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> GetPaymentNodes(
				IUnitOfWork unitOfWork,
				IPaymentsRepository paymentsRepository,
				string counterpartyInn,
				IList<IList<string>> rows)
			{
				var paymentDiscrepanciesNodes = GetPaymentDiscrepanciesNodesFromFile(rows);
				var paymentNums = paymentDiscrepanciesNodes.Keys.Select(k => k.PaymentNum).ToList();

				var paymentFromDatabase = GetPaymentsByNumbersFromDatabase(unitOfWork, paymentsRepository, paymentNums, counterpartyInn);

				MatchPaymentsFromFileWithDatabase(paymentFromDatabase, ref paymentDiscrepanciesNodes);

				return paymentDiscrepanciesNodes;
			}

			private static IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> GetPaymentDiscrepanciesNodesFromFile(IList<IList<string>> rows)
			{
				var paymentNodes = new Dictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode>();

				foreach(var rowData in rows)
				{
					if(!IsOrderOrPaymentDataRow(rowData))
					{
						continue;
					}

					if(rowData[1].StartsWith("Оплата"))
					{
						var paymentNode = CreatePaymentNode(rowData);

						paymentNodes.Add((paymentNode.PaymentNum, paymentNode.PaymentDate), paymentNode);
					}
				}

				return paymentNodes;
			}

			private static IList<PaymentNode> GetPaymentsByNumbersFromDatabase(
				IUnitOfWork unitOfWork,
				IPaymentsRepository paymentsRepository,
				IList<int> paymentNums,
				string counterpartyInn)
			{
				var payments = paymentsRepository.GetPaymentsByNumbers(unitOfWork, paymentNums, counterpartyInn);

				return payments;
			}

			private static void MatchPaymentsFromFileWithDatabase(
				IList<PaymentNode> paymentNodesFromDatabase,
				ref IDictionary<(int PaymentNum, DateTime Date), PaymentDiscrepanciesNode> paymentDiscrepanciesNodes)
			{
				foreach(var paymentNode in paymentNodesFromDatabase)
				{
					if(paymentDiscrepanciesNodes.TryGetValue((paymentNode.PaymentNum, paymentNode.PaymentDate), out var node))
					{
						node.ProgramPaymentSum = paymentNode.PaymentSum;
						node.IsManuallyCreated = paymentNode.IsManuallyCreated;
						node.CounterpartyId = paymentNode.CounterpartyId;
						node.CounterpartyName = paymentNode.CounterpartyName;
						node.CounterpartyInn = paymentNode.CounterpartyInn;
						node.PaymentPurpose = paymentNode.PaymentPurpose;
					}
					else
					{
						paymentDiscrepanciesNodes.Add(
							(paymentNode.PaymentNum, paymentNode.PaymentDate),
							new PaymentDiscrepanciesNode
							{
								PaymentNum = paymentNode.PaymentNum,
								PaymentDate = paymentNode.PaymentDate,
								ProgramPaymentSum = paymentNode.PaymentSum,
								IsManuallyCreated = paymentNode.IsManuallyCreated,
								CounterpartyId = paymentNode.CounterpartyId,
								CounterpartyName = paymentNode.CounterpartyName,
								CounterpartyInn = paymentNode.CounterpartyInn,
								PaymentPurpose = paymentNode.PaymentPurpose
							});
					}
				}
			}

			#endregion GetGetPaymentNodes

			private static bool IsOrderOrPaymentDataRow(IList<string> rowData)
			{
				if(rowData.Count < 3)
				{
					return false;
				}

				DateTime.TryParse(rowData[0], out var date);

				if(date == default)
				{
					return false;
				}

				return true;
			}

			private static OrderDiscrepanciesNode CreateOrderNode(IEnumerable<string> rowData)
			{
				var data = rowData.ToArray();

				var node = new OrderDiscrepanciesNode
				{
					OrderId = XlsParseHelper.ParseNumberFromString(data[1]),
					DocumentOrderSum = decimal.Parse(data[2])
				};

				return node;
			}

			private static PaymentDiscrepanciesNode CreatePaymentNode(IEnumerable<string> rowData)
			{
				var data = rowData.ToArray();

				var node = new PaymentDiscrepanciesNode
				{
					PaymentNum = XlsParseHelper.ParseNumberFromString(data[1]),
					PaymentDate = XlsParseHelper.ParseDateFromString(data[1]),
					DocumentPaymentSum = decimal.Parse(data[2])
				};

				return node;
			}

			public class OrderDiscrepanciesNode
			{
				public int OrderId { get; set; }
				public DateTime? OrderDeliveryDate { get; set; }
				public OrderStatus? OrderStatus { get; set; }
				public OrderPaymentStatus? OrderPaymentStatus { get; set; }
				public decimal DocumentOrderSum { get; set; }
				public decimal ProgramOrderSum { get; set; }
				public decimal AllocatedSum { get; set; }
				public bool IsMissingFromDocument { get; set; }
				public bool OrderSumDiscrepancy => ProgramOrderSum != DocumentOrderSum;
			}

			public class PaymentDiscrepanciesNode
			{
				public int PaymentNum { get; set; }
				public DateTime PaymentDate { get; set; }
				public decimal DocumentPaymentSum { get; set; }
				public decimal ProgramPaymentSum { get; set; }
				public int CounterpartyId { get; set; }
				public string CounterpartyName { get; set; }
				public string CounterpartyInn { get; set; }
				public bool IsManuallyCreated { get; set; }
				public string PaymentPurpose { get; set; }
			}
		}
	}
}
