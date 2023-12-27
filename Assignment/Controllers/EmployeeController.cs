using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

public class EmployeeController : Controller
{
    private readonly HttpClient _client;
    private List<Dictionary<string, object>> _data;

    public EmployeeController()
    {
        _client = new HttpClient();
        _data = new List<Dictionary<string, object>>();
    }

    private async Task FetchData()
    {
        var apiKey = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        var apiUrl = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={apiKey}";

        var response = await _client.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content);
        }
    }

    public async Task<ActionResult> Index()
    {
        await FetchData();
        return View(_data);
    }

    public async Task<ActionResult> Hours()
    {
        if (!_data.Any())
        {
            await FetchData();
        }

        var employeeHours = new Dictionary<string, double>();

        foreach (var item in _data)
        {
            if (item.ContainsKey("EmployeeName") && item["EmployeeName"] != null)
            {
                var name = item["EmployeeName"].ToString();
                var startTime = DateTime.Parse(item["StarTimeUtc"].ToString());
                var endTime = DateTime.Parse(item["EndTimeUtc"].ToString());
                var difference = (endTime - startTime).TotalHours;

                if (employeeHours.ContainsKey(name))
                {
                    employeeHours[name] += difference;
                }
                else
                {
                    employeeHours[name] = difference;
                }
            }
        }

        var orderedEmployeeHours = employeeHours.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        return View(orderedEmployeeHours);
    }
}