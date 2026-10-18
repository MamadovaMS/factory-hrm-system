namespace HRM_SlowGasMotorFactory_DataModels.DataModels;

public class SaleInfo
{
    public Guid CashierId { get; set; }
    public DateTime SaleDate { get; set; }
    public double Sum { get; set; }
}