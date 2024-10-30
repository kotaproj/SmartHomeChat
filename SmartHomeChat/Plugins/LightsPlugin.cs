using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace SmartHomeChat.Plugins;

public class LightsPlugin
{
    // Mock data for the lights
    private readonly List<LightModel> lights = new()
    {
        new LightModel { Id = 1, Name = "Table Lamp", IsOn = false, Brightness = 100, Hex = "FF0000" },
        new LightModel { Id = 2, Name = "Porch light", IsOn = false, Brightness = 50, Hex = "00FF00" },
        new LightModel { Id = 3, Name = "Chandelier", IsOn = true, Brightness = 75, Hex = "0000FF" }
    };
    private static readonly HttpClient client = new HttpClient();


    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    [return: Description("An array of lights")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        Console.WriteLine("run:GetLightsAsync"); //koko
        return lights;
    }

    [KernelFunction("get_state")]
    [Description("Gets the state of a particular light")]
    [return: Description("The state of the light")]
    public async Task<LightModel?> GetStateAsync([Description("The ID of the light")] int id)
    {
        Console.WriteLine("run:GetStateAsync");
        // Get the state of the light with the specified ID
        return lights.FirstOrDefault(light => light.Id == id);
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    [return: Description("The updated state of the light; will return null if the light does not exist")]
    public async Task<LightModel?> ChangeStateAsync(int id, LightModel LightModel)
    {
        Console.WriteLine("run:ChangeStateAsync"); //koko

        var light = lights.FirstOrDefault(light => light.Id == id);

        if (light == null)
        {
            return null;
        }

        //ここに
        bool isSuccess = await SetLightStatusAsync(LightModel.IsOn);
        if (!isSuccess)
        {
            return null;
        }


        //using (var httpClient = new HttpClient())
        //{
        //    try
        //    {
        //        Console.WriteLine("httpClient.GetAsync"); //koko
        //        var response = await httpClient.GetAsync("https://localhost:7244/");

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine("!response.IsSuccessStatusCode"); //koko
        //            return null;
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        Console.WriteLine($"Request failed: {ex.Message}");
        //        return null;
        //    }
        //}

        Console.WriteLine($"LightModel : {LightModel.IsOn}, {LightModel.Brightness}, {LightModel.Hex}");
        Console.WriteLine($"light : {light.IsOn}, {light.Brightness}, {light.Hex}");

        // Update the light with the new state
        light.IsOn = LightModel.IsOn;
        light.Brightness = LightModel.Brightness;
        light.Hex = LightModel.Hex;

        return light;
    }

    public async Task<bool> SetLightStatusAsync(bool? turnOn)
    {
        if ( !turnOn.HasValue )
        {
            return false;
        }

        var url = "http://192.168.11.19/light";
        var lightStatus = turnOn.Value ? "on" : "off";
        var content = new StringContent($"light={lightStatus}", Encoding.UTF8, "application/x-www-form-urlencoded");

        try
        {
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            // 成功の場合は true を返す
            return true;
        }
        catch (HttpRequestException)
        {
            // 失敗の場合は false を返す
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

    [JsonPropertyName("brightness")]
    public byte? Brightness { get; set; }

    [JsonPropertyName("hex")]
    public string? Hex { get; set; }
}

