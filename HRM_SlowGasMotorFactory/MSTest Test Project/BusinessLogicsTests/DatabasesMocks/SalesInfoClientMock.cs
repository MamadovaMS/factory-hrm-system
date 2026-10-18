using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;

namespace HRM_SlowGasMotorFactory_TestsMSTest.BusinessLogicsTests.DatabasesMocks;

internal class SalesInfoClientMock : ISalesInfoClient
{
    public readonly List<SaleInfo> Values = [];

    public List<SaleInfo>? GetSales(Guid workerId, DateTime fromDate, DateTime toDate)
    {
        return Values.Where(x => x.CashierId == workerId && x.SaleDate >= fromDate && x.SaleDate <= toDate).ToList();
    }
}