using System.Net;
using System.Net.Http.Json;
using EquipmentBorrowing.Contracts;

namespace EquipmentBorrowing.Client;

public interface IEquipmentBorrowingApiClient
{
    Task<IReadOnlyList<StudentDto>> GetStudentsAsync(string? search = null, CancellationToken ct = default);
    Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default);
    Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentRequest request, CancellationToken ct = default);
    Task<StudentDto> SetEligibilityAsync(int id, bool isEligible, CancellationToken ct = default);
    Task<IReadOnlyList<EquipmentDto>> GetEquipmentAsync(bool? available = null, CancellationToken ct = default);
    Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequest request, CancellationToken ct = default);
    Task<EquipmentDto> UpdateEquipmentAsync(int id, UpdateEquipmentRequest request, CancellationToken ct = default);
    Task<BorrowingDto> BorrowAsync(CreateBorrowingRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BorrowingDto>> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default);
    Task<BorrowingDto> ReturnAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<BorrowingDto>> GetBorrowingsAsync(string? search = null, string status = "All", string sort = "borrowedDate", string direction = "asc", CancellationToken ct = default);
    Task<IReadOnlyList<BorrowingDto>> GetStudentHistoryAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<BorrowingDto>> GetEquipmentHistoryAsync(int id, CancellationToken ct = default);
}
public sealed class EquipmentBorrowingApiClient(HttpClient http) : IEquipmentBorrowingApiClient
{
    private const string Prefix = "api/v1";

    public Task<IReadOnlyList<StudentDto>> GetStudentsAsync(string? search = null, CancellationToken ct = default) =>
        GetList<StudentDto>($"{Prefix}/students" + (string.IsNullOrWhiteSpace(search) ? "" : $"?search={Uri.EscapeDataString(search)}"), ct);
    public Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default) => Send<StudentDto>(HttpMethod.Post, $"{Prefix}/students", request, ct);
    public Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentRequest request, CancellationToken ct = default) => Send<StudentDto>(HttpMethod.Put, $"{Prefix}/students/{id}", request, ct);
    public Task<StudentDto> SetEligibilityAsync(int id, bool isEligible, CancellationToken ct = default) => Send<StudentDto>(HttpMethod.Patch, $"{Prefix}/students/{id}/eligibility", new EligibilityRequest(isEligible), ct);
    public Task<IReadOnlyList<EquipmentDto>> GetEquipmentAsync(bool? available = null, CancellationToken ct = default) =>
        GetList<EquipmentDto>($"{Prefix}/equipment" + (available.HasValue ? $"?available={available.Value.ToString().ToLowerInvariant()}" : ""), ct);
    public Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentRequest request, CancellationToken ct = default) => Send<EquipmentDto>(HttpMethod.Post, $"{Prefix}/equipment", request, ct);
    public Task<EquipmentDto> UpdateEquipmentAsync(int id, UpdateEquipmentRequest request, CancellationToken ct = default) => Send<EquipmentDto>(HttpMethod.Put, $"{Prefix}/equipment/{id}", request, ct);
    public Task<BorrowingDto> BorrowAsync(CreateBorrowingRequest request, CancellationToken ct = default) => Send<BorrowingDto>(HttpMethod.Post, $"{Prefix}/borrowings", request, ct);
    public async Task<IReadOnlyList<BorrowingDto>> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default) =>
        (await Send<CheckoutResponse>(HttpMethod.Post, $"{Prefix}/checkout-requests", request, ct)).Borrowings;
    public Task<BorrowingDto> ReturnAsync(int id, CancellationToken ct = default) => Send<BorrowingDto>(HttpMethod.Post, $"{Prefix}/borrowings/{id}/return", new ReturnRequest(), ct);
    public Task<IReadOnlyList<BorrowingDto>> GetBorrowingsAsync(string? search = null, string status = "All", string sort = "borrowedDate", string direction = "asc", CancellationToken ct = default)
    {
        var query = $"status={Uri.EscapeDataString(status)}&sort={Uri.EscapeDataString(sort)}&direction={Uri.EscapeDataString(direction)}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        return GetList<BorrowingDto>($"{Prefix}/borrowings?{query}", ct);
    }
    public Task<IReadOnlyList<BorrowingDto>> GetStudentHistoryAsync(int id, CancellationToken ct = default) => GetList<BorrowingDto>($"{Prefix}/students/{id}/borrowings", ct);
    public Task<IReadOnlyList<BorrowingDto>> GetEquipmentHistoryAsync(int id, CancellationToken ct = default) => GetList<BorrowingDto>($"{Prefix}/equipment/{id}/borrowings", ct);

    private async Task<T> Send<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var message = new HttpRequestMessage(method, path);
        if (body is not null) message.Content = JsonContent.Create(body);
        HttpResponseMessage response;
        try { response = await http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct); }
        catch (HttpRequestException ex)
        {
            throw new EquipmentBorrowingApiException(null, "outcome_unknown",
                "No response was received for this change. Refresh the affected records before deciding whether to retry.", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new EquipmentBorrowingApiException(null, "outcome_unknown",
                "The request timed out before a response arrived. Refresh the affected records before deciding whether to retry.", ex);
        }
        using (response)
        {
        if (!response.IsSuccessStatusCode) throw await CreateException(response, ct);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
            ?? throw new EquipmentBorrowingApiException(response.StatusCode, "invalid_response", "The API returned an empty or invalid response.");
        }
    }

    private async Task<IReadOnlyList<T>> GetList<T>(string path, CancellationToken ct)
    {
        using var response = await http.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode) throw await CreateException(response, ct);
        return await response.Content.ReadFromJsonAsync<List<T>>(cancellationToken: ct) ?? [];
    }

    private static async Task<EquipmentBorrowingApiException> CreateException(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorDto>(cancellationToken: ct);
            if (error is not null) return new EquipmentBorrowingApiException(response.StatusCode, error.Code, error.Message);
        }
        catch (System.Text.Json.JsonException) { }
        return new EquipmentBorrowingApiException(response.StatusCode, "http_error", $"The API returned HTTP {(int)response.StatusCode}.");
    }
}

public sealed class EquipmentBorrowingApiException(HttpStatusCode? statusCode, string code, string message, Exception? innerException = null) : Exception(message, innerException)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
    public string Code { get; } = code;
    public bool IsServerError => StatusCode is { } status && (int)status >= 500;
}

public static class EquipmentBorrowingApiFeedback
{
    public static bool NeedsRefresh(Exception exception) => exception is EquipmentBorrowingApiException { IsServerError: true } or EquipmentBorrowingApiException { Code: "outcome_unknown" };

    public static string For(Exception exception) => exception switch
    {
        EquipmentBorrowingApiException { Code: "outcome_unknown" } => exception.Message,
        EquipmentBorrowingApiException api when api.IsServerError => "The API is reachable, but the server or database failed. Refresh before retrying. " + api.Message,
        EquipmentBorrowingApiException api => api.Message,
        HttpRequestException => "The API could not be reached. Check that it is running and that the API URL is correct.",
        TaskCanceledException => "The API request timed out. Check the API and try refreshing.",
        _ => exception.Message
    };
}

