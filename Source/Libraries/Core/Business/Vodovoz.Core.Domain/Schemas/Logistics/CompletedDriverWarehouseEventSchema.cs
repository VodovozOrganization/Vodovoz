namespace Vodovoz.Core.Domain.Schemas.Logistics
{
	public static class CompletedDriverWarehouseEventSchema
	{
		public static string TableName => "completed_drivers_warehouses_events";
		public static string IdColumn => "id";
		public static string LatitudeColumn => "latitude";
		public static string LongitudeColumn => "longitude";
		public static string CompletedColumn => "completed_date";
		public static string DistanceMetersFromScanningLocationColumn => "distance_meters_from_scanning_location";
		public static string DocumentIdColumn => "document_id";
		public static string DriverWarehouseEventColumn => "driver_warehouse_event_id";
		public static string CarColumn => "car_id";
		public static string EmployeeColumn => "employee_id";
	}
}
