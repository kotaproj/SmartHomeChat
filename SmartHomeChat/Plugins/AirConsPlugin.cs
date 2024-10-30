using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SmartHomeChat.Plugins;


public enum AirConMode
{
    [Display(Name = "冷房")]
    Cool,
    [Display(Name = "暖房")]
    Heat,
    [Display(Name = "ドライ")]
    Dry,
    [Display(Name = "自動")]
    Auto
}


public class AirConsPlugin
{
    // Mock data for the aircons
    private readonly List<AirConModel> aircons = new()
   {
      new AirConModel { Id = 1, Name = "Living", IsOn = false, Tempature = 26, Mode = AirConMode.Auto },
      new AirConModel { Id = 2, Name = "Bedroom", IsOn = false, Tempature = 26, Mode = AirConMode.Auto },
   };

    [KernelFunction("get_aircons")]
    [Description("Gets a list of air controllers and their current state")]
    [return: Description("An array of air controllers")]
    public async Task<List<AirConModel>> GetAirConsAsync()
    {
        Console.WriteLine("run:GetAirConsAsync"); //koko
        return aircons;
    }

    [KernelFunction("get_state")]
    [Description("Gets the state of a particular air controller")]
    [return: Description("The state of the air controller")]
    public async Task<AirConModel?> GetStateAsync([Description("The ID of the air controller")] int id)
    {
        Console.WriteLine("run:GetStateAsync");
        // Get the state of the aircon with the specified ID
        return aircons.FirstOrDefault(aircon => aircon.Id == id);
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the aircon")]
    [return: Description("The updated state of the aircon; will return null if the aircon does not exist")]
    public async Task<AirConModel?> ChangeStateAsync(int id, AirConModel AirConModel)
    {
        Console.WriteLine("run:ChangeStateAsync"); //koko

        var aircon = aircons.FirstOrDefault(aircon => aircon.Id == id);

        if (aircon == null)
        {
            return null;
        }

        Console.WriteLine($"AirConModel : {AirConModel.Id} ,{AirConModel.Name} ,{AirConModel.IsOn} ,{AirConModel.Tempature} ,{AirConModel.Mode} ,");
        Console.WriteLine($"aircon : {aircon.Id} ,{aircon.Name} ,{aircon.IsOn} ,{aircon.Tempature} ,{aircon.Mode} ,");

        // Update the aircon with the new state
        aircon.IsOn = AirConModel.IsOn;
        aircon.Tempature = AirConModel.Tempature;
        aircon.Mode = AirConModel.Mode;

        return aircon;
    }
}

public class AirConModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("tempature")]
    public int? Tempature { get; set; }

    [JsonPropertyName("aircon_mode")]
    public AirConMode? Mode { get; set; }
}
