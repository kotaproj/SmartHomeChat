using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHomeChat.Plugins;

public class FansPlugin
{
    private readonly string NatureToken = "DUMMY_TOKEN";

    private readonly List<FanModel> fans = new()
    {
        new FanModel { Id = 1, Name = "リビング 扇風機", IsOn = null, PowerId="DUMMY_POWERID"},
        new FanModel { Id = 2, Name = "寝室 扇風機", IsOn = null, PowerId="DUMMY_POWERID" +
            ""},
    };
    private static readonly HttpClient client = new HttpClient();


    [KernelFunction("get_fans")]
    [Description("Gets a list of fans and their current state")]
    [return: Description("An array of fans")]
    public async Task<List<FanModel>> GetFansAsync()
    {
        return fans;
    }

    [KernelFunction("get_state")]
    [Description("Gets the state of a particular fan")]
    [return: Description("The state of the fan")]
    public async Task<FanModel?> GetStateAsync([Description("The ID of the fan")] int id)
    {
        // Get the state of the fan with the specified ID
        return fans.FirstOrDefault(fan => fan.Id == id);
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the fan")]
    [return: Description("The updated state of the fan; will return null if the fan does not exist")]
    public async Task<FanModel?> ChangeStateAsync(int id, FanModel FanModel)
    {
        var fan = fans.FirstOrDefault(fan => fan.Id == id);
        if (fan == null)
        {
            return null;
        }

        bool isSuccess = await PostFanStatusAsync(id, FanModel.IsOn);
        if (!isSuccess)
        {
            fan.IsOn = null;
            return null;
        }

        // Update the fan with the new state
        fan.IsOn = FanModel.IsOn;

        return fan;
    }

    public async Task<bool> PostFanStatusAsync(int id, bool? turnOn)
    {
        var url = $"https://api.nature.global/1/signals/{fans.FirstOrDefault(fan => fan.Id == id)?.PowerId}/send";
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/x-www-form-urlencoded");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", NatureToken);

        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}

public class FanModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("power_id")]
    public string? PowerId { get; set; }
}
