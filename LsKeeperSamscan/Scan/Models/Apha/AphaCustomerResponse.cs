using System.Text.Json.Serialization;

namespace LsKeeperSamscan.Scan.Models.Apha;

public class AphaCustomerFindResponse
{
    [JsonPropertyName("data")]
    public List<AphaCustomerData>? Data { get; set; }
}

public class AphaCustomerData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("contactDetails")]
    public List<AphaContactDetail>? ContactDetails { get; set; }

    [JsonPropertyName("addresses")]
    public List<AphaCustomerAddress>? Addresses { get; set; }
}

public class AphaContactDetail
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("isPreferred")]
    public bool IsPreferred { get; set; }
}

public class AphaCustomerAddress
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("town")]
    public string? Town { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("isPreferred")]
    public bool IsPreferred { get; set; }
}
