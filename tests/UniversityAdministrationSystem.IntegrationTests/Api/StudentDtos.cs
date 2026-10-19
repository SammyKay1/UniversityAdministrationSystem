using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

public class StudentListDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";
    
    [JsonPropertyName("department")]
    public string Department { get; set; } = "";
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
}

public class PagedResultDto<T>
{
    [JsonPropertyName("items")]
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }
    
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
    
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }
    
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }
}

public class StudentDetailsDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";
    
    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }
    
    [JsonPropertyName("department")]
    public string Department { get; set; } = "";
    
    [JsonPropertyName("address")]
    public string Address { get; set; } = "";
    
    [JsonPropertyName("courses")]
    public List<string> Courses { get; set; } = new();
}

public class StudentEditDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = "";
    
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = "";
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";
    
    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }
    
    [JsonPropertyName("departmentId")]
    public int DepartmentId { get; set; }
    
    [JsonPropertyName("address")]
    public AddressDto Address { get; set; } = new();
    
    [JsonPropertyName("concurrencyToken")]
    public Guid ConcurrencyToken { get; set; }
}

public class AddressDto
{
    [JsonPropertyName("addressLine1")]
    public string AddressLine1 { get; set; } = "";
    
    [JsonPropertyName("addressLine2")]
    public string AddressLine2 { get; set; } = "";
    
    [JsonPropertyName("city")]
    public string City { get; set; } = "";
    
    [JsonPropertyName("state")]
    public string State { get; set; } = "";
    
    [JsonPropertyName("country")]
    public string Country { get; set; } = "";
}