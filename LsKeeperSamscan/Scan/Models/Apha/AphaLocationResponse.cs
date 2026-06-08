using System.Text.Json.Serialization;

namespace LsKeeperSamscan.Scan.Models.Apha;

public class AphaLocationResponse
{
    [JsonPropertyName("data")]
    public AphaLocationData? Data { get; set; }
}

public class AphaLocationData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public AphaAddress? Address { get; set; }

    [JsonPropertyName("osMapReference")]
    public string? OsMapReference { get; set; }

    [JsonPropertyName("livestockUnits")]
    public List<AphaLivestockUnit>? LivestockUnits { get; set; }

    [JsonPropertyName("facilities")]
    public List<AphaFacility>? Facilities { get; set; }
}

public class AphaAddress
{
    [JsonPropertyName("pafOrganisationName")]
    public string? PafOrganisationName { get; set; }

    [JsonPropertyName("buildingName")]
    public string? BuildingName { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("locality")]
    public string? Locality { get; set; }

    [JsonPropertyName("town")]
    public string? Town { get; set; }

    [JsonPropertyName("administrativeAreaCounty")]
    public string? AdministrativeAreaCounty { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}

public class AphaLivestockUnit
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("animalQuantities")]
    public int AnimalQuantities { get; set; }

    [JsonPropertyName("species")]
    public string? Species { get; set; }
}

public class AphaFacility
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("facilityType")]
    public string? FacilityType { get; set; }

    [JsonPropertyName("businessActivity")]
    public string? BusinessActivity { get; set; }
}
