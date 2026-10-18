using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;

public interface ISalesInfoClient
{
    List<SaleInfo>? GetSales(Guid workerId, DateTime fromDate, DateTime toDate);
}