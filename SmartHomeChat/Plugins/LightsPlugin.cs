using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHomeChat.Plugins;

public class LightsPlugin
{
    private readonly List<LightModel> lights = new()
    {
        new LightModel { Id = 1, Name = "Kitchen Light", IsOn = null, IpAdress = "192.168.11.6" },
    };
    private static readonly HttpClient client = new HttpClient();


    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    [return: Description("An array of lights")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nameof(LightsPlugin)}.{nameof(GetLightsAsync)} - run");

        foreach (var light in lights)
        {
            light.IsOn = await GetLightStatusAsync(light.Id);
        }

        return lights;
    }

    [KernelFunction("get_state")]
    [Description("Gets the state of a particular light")]
    [return: Description("The state of the light")]
    public async Task<LightModel?> GetStateAsync([Description("The ID of the light")] int id)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nameof(LightsPlugin)}.{nameof(GetStateAsync)} - run");

        // Get the state of the light with the specified ID
        return lights.FirstOrDefault(light => light.Id == id);
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    [return: Description("The updated state of the light; will return null if the light does not exist")]
    public async Task<LightModel?> ChangeStateAsync(int id, LightModel LightModel)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nameof(LightsPlugin)}.{nameof(ChangeStateAsync)} - run");

        var light = lights.FirstOrDefault(light => light.Id == id);
        if (light == null)
        {
            return null;
        }

        bool isSuccess = await PostLightStatusAsync(id, LightModel.IsOn);
        if (!isSuccess)
        {
            light.IsOn = null;
            return null;
        }

        // Update the light with the new state
        light.IsOn = LightModel.IsOn;

        return light;
    }

    public async Task<bool?> GetLightStatusAsync(int id)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nameof(LightsPlugin)}.{nameof(GetLightStatusAsync)} - run");

        var url = $"http://{lights.FirstOrDefault(light => light.Id == id)?.IpAdress}/status";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // タイムアウトを1秒に設定

        try
        {
            var response = await client.GetAsync(url, cts.Token);
            response.EnsureSuccessStatusCode();

            // レスポンス内容を文字列として読み取る
            var responseContent = await response.Content.ReadAsStringAsync();

            // JSONデータを解析してライトの状態を取得
            var jsonDocument = JsonDocument.Parse(responseContent);
            var lightStatus = jsonDocument.RootElement.GetProperty("light").GetString();

            // ライトの状態に応じて true または false を返す
            return lightStatus == "on" ? true : lightStatus == "off" ? false : null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            // 失敗の場合は null を返す
            return null;
        }
    }

    public async Task<bool> PostLightStatusAsync(int id, bool? turnOn)
    {
        if (!turnOn.HasValue)
        {
            return false;
        }

        var url = $"http://{lights.FirstOrDefault(light => light.Id == id)?.IpAdress}/light";
        var lightStatus = turnOn.Value ? "on" : "off";
        var content = new StringContent($"light={lightStatus}", Encoding.UTF8, "application/x-www-form-urlencoded");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)); // タイムアウトを3秒に設定

        try
        {
            var response = await client.PostAsync(url, content, cts.Token);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nameof(LightsPlugin)}.{nameof(PostLightStatusAsync)} - ok!");
            return true;
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nameof(LightsPlugin)}.{nameof(PostLightStatusAsync)} - Request timed out.");
            return false;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {nameof(LightsPlugin)}.{nameof(PostLightStatusAsync)} - Request failed: {ex.Message}");
            return false;
        }
    }
}

public class LightModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("ipaddress")]
    public string IpAdress { get; set; } = string.Empty;
}
