using HRM_SlowGasMotorFactory_BusinessLogics.Interfaces;
using HRM_SlowGasMotorFactory_DataModels.DataModels;
using System.Text.Json;

namespace HRM_SlowGasMotorFactory_Controllers.Clients;

public class SalesInfoClient : ISalesInfoClient
{
    private readonly HttpClient _client;
    protected static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public SalesInfoClient(HttpClient client)
    {
        _client = client;
    }

    public virtual List<SaleInfo>? GetSales(Guid workerId, DateTime fromDate, DateTime toDate)
    {
        var response = _client.GetAsync($"/v1/sale?cashierId={workerId}&saleDateFrom={fromDate:MM/dd/yyyy HH:mm:ss}&saleDateTo={toDate:MM/dd/yyyy HH:mm:ss}")
            .GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode) return null;

        return JsonSerializer.Deserialize<List<SaleInfo>>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult(), JsonSerializerOptions);
    }
}