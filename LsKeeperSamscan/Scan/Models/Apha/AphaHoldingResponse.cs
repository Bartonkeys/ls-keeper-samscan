using System.Text.Json.Serialization;

namespace LsKeeperSamscan.Scan.Models.Apha;

public class AphaHoldingResponse
{
    [JsonPropertyName("data")]
    public AphaHoldingData? Data { get; set; }
}

public class AphaHoldingData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("cphType")]
    public string? CphType { get; set; }

    [JsonPropertyName("relationships")]
    public AphaHoldingRelationships? Relationships { get; set; }
}

public class AphaHoldingRelationships
{
    [JsonPropertyName("location")]
    public AphaRelationshipRef? Location { get; set; }

    [JsonPropertyName("cphHolder")]
    public AphaRelationshipRef? CphHolder { get; set; }
}

public class AphaRelationshipRef
{
    [JsonPropertyName("data")]
    public AphaRefData? Data { get; set; }
}

public class AphaRefData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
