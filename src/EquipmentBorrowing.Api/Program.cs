using System.Text.Json;
using System.Text.Json.Serialization;
using EquipmentBorrowing.Application.Errors;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Models;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
builder.Services.AddPersistence();
builder.Services.AddTransient<BorrowEquipmentService>();
builder.Services.AddTransient<ReturnEquipmentService>();
builder.Services.AddTransient<StudentManagementService>();
builder.Services.AddTransient<EquipmentManagementService>();

var app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        app.Logger.LogError("API request failed with {ExceptionType}.", ex.GetType().Name);
        if (context.Response.HasStarted) throw;
        var (status, code, message) = MapError(ex);
        context.Response.Clear();
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ErrorDto(code, message));
    }
});
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    if (response.StatusCode == 404)
        await response.WriteAsJsonAsync(new ErrorDto("not_found", "The requested route or record was not found."));
    else if (response.StatusCode == 405)
        await response.WriteAsJsonAsync(new ErrorDto("method_not_allowed", "This HTTP method is not supported for the route."));
});
app.MapGet("/openapi/v1.json", () => Results.File(Path.Combine(AppContext.BaseDirectory, "openapi.v1.json"), "application/json"));

var api = app.MapGroup("/api/v1");

api.MapGet("/equipment", async (HttpContext http, CancellationToken ct) =>
{
    bool? available = null;
    if (http.Request.Query.TryGetValue("available", out var value))
    {
        if (!bool.TryParse(value, out var parsed)) throw new ApiInputException("available must be true or false.");
        available = parsed;
    }
    var equipment = http.RequestServices.GetRequiredService<IEquipmentRepository>();
    var items = available == true ? await equipment.GetAvailableAsync(ct) : await equipment.GetAllAsync(ct);
    return Results.Ok(items.Where(e => !available.HasValue || e.IsAvailable).OrderBy(e => e.Id).Select(ToEquipmentDto));
}).WithName("GetEquipment").WithSummary("List equipment, optionally filtering to available items.").Produces<EquipmentDto[]>(200).Produces<ErrorDto>(400);

api.MapGet("/equipment/{id:int}", async (int id, IEquipmentRepository repo, CancellationToken ct) =>
    Results.Ok(ToEquipmentDto(await repo.GetByIdAsync(id, ct) ?? throw new EquipmentNotFoundError($"Equipment {id} was not found."))))
    .WithName("GetEquipmentById").Produces<EquipmentDto>(200).Produces<ErrorDto>(404);

api.MapPost("/equipment", async (HttpContext context, CancellationToken ct) =>
{
    var body = await ReadBody<CreateEquipmentRequest>(context.Request, ct);
    var service = context.RequestServices.GetRequiredService<EquipmentManagementService>();
    var item = await service.RegisterAsync(body.EquipmentCode ?? "", body.Name ?? "", body.Type ?? "General", body.Description, ct);
    return Results.Created($"/api/v1/equipment/{item.Id}", ToEquipmentDto(item));
}).WithName("CreateEquipment").Accepts<CreateEquipmentRequest>("application/json").Produces<EquipmentDto>(201).Produces<ErrorDto>(400).Produces<ErrorDto>(409);

api.MapPut("/equipment/{id:int}", async (int id, HttpContext context, CancellationToken ct) =>
{
    var body = await ReadBody<UpdateEquipmentRequest>(context.Request, ct);
    var repo = context.RequestServices.GetRequiredService<IEquipmentRepository>();
    var service = context.RequestServices.GetRequiredService<EquipmentManagementService>();
    var existing = await repo.GetByIdAsync(id, ct) ?? throw new EquipmentNotFoundError($"Equipment {id} was not found.");
    return Results.Ok(ToEquipmentDto(await service.UpdateAsync(id, body.EquipmentCode ?? "", body.Name ?? "", body.Type ?? existing.Type, body.Description, ct)));
}).WithName("UpdateEquipment").Accepts<UpdateEquipmentRequest>("application/json").Produces<EquipmentDto>(200).Produces<ErrorDto>(400).Produces<ErrorDto>(404).Produces<ErrorDto>(409);

api.MapGet("/students", async (HttpContext http, CancellationToken ct) =>
{
    var search = http.Request.Query["search"].ToString().Trim();
    if (search.Length > 100) throw new ApiInputException("Search must be 100 characters or fewer.");
    var repo = http.RequestServices.GetRequiredService<IStudentRepository>();
    var rows = await repo.GetAllAsync(ct);
    if (!string.IsNullOrEmpty(search)) rows = rows.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || s.StudentNumber.Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray();
    return Results.Ok(rows.OrderBy(s => s.Id).Select(ToStudentDto));
}).WithName("GetStudents").Produces<StudentDto[]>(200).Produces<ErrorDto>(400);

api.MapGet("/students/{id:int}", async (int id, IStudentRepository repo, CancellationToken ct) =>
    Results.Ok(ToStudentDto(await repo.GetByIdAsync(id, ct) ?? throw new StudentNotFoundError($"Student {id} was not found."))))
    .WithName("GetStudentById").Produces<StudentDto>(200).Produces<ErrorDto>(404);

api.MapPost("/students", async (HttpContext context, CancellationToken ct) =>
{
    var body = await ReadBody<CreateStudentRequest>(context.Request, ct);
    var service = context.RequestServices.GetRequiredService<StudentManagementService>();
    var student = await service.RegisterAsync(body.StudentNumber ?? "", body.FullName ?? "", body.Contact, ct);
    return Results.Created($"/api/v1/students/{student.Id}", ToStudentDto(student));
}).WithName("CreateStudent").Accepts<CreateStudentRequest>("application/json").Produces<StudentDto>(201).Produces<ErrorDto>(400).Produces<ErrorDto>(409);

api.MapPut("/students/{id:int}", async (int id, HttpContext context, CancellationToken ct) =>
{
    var body = await ReadBody<UpdateStudentRequest>(context.Request, ct);
    var service = context.RequestServices.GetRequiredService<StudentManagementService>();
    return Results.Ok(ToStudentDto(await service.UpdateAsync(id, body.StudentNumber ?? "", body.FullName ?? "", body.Contact, ct)));
}).WithName("UpdateStudent").Accepts<UpdateStudentRequest>("application/json").Produces<StudentDto>(200).Produces<ErrorDto>(400).Produces<ErrorDto>(404).Produces<ErrorDto>(409);

api.MapPatch("/students/{id:int}/eligibility", async (int id, HttpContext context, CancellationToken ct) =>
{
    var body = await ReadBody<EligibilityRequest>(context.Request, ct);
    if (body.IsEligible is null) throw new ApiInputException("isEligible must be a boolean.");
    var service = context.RequestServices.GetRequiredService<StudentManagementService>();
    return Results.Ok(ToStudentDto(await service.SetEligibilityAsync(id, body.IsEligible.Value, ct)));
}).WithName("SetStudentEligibility").Accepts<EligibilityRequest>("application/json").Produces<StudentDto>(200).Produces<ErrorDto>(400).Produces<ErrorDto>(404);

api.MapPost("/borrowings", async (HttpContext context, CancellationToken ct) =>
{
    var body = await ReadBody<CreateBorrowingRequest>(context.Request, ct);
    if (body.StudentId is null or <= 0 || body.EquipmentId is null or <= 0) throw new ApiInputException("studentId and equipmentId must be positive integers.");
    if (body.ExpectedReturnDate is null) throw new ApiInputException("expectedReturnDate is required.");
    ValidateDueDate(body.ExpectedReturnDate.Value);
    var service = context.RequestServices.GetRequiredService<BorrowEquipmentService>();
    var borrowing = await service.BorrowAsync(body.StudentId.Value, body.EquipmentId.Value, body.ExpectedReturnDate.Value.ToDateTime(TimeOnly.MinValue), ct);
    return Results.Created($"/api/v1/borrowings/{borrowing.Id}", await ToDtoAsync(borrowing, app.Services, ct));
}).WithName("CreateBorrowing").Accepts<CreateBorrowingRequest>("application/json").Produces<BorrowingDto>(201).Produces<ErrorDto>(400).Produces<ErrorDto>(404).Produces<ErrorDto>(409);

api.MapPost("/checkout-requests", async (HttpContext context, CancellationToken ct) =>
{
    var body = await ReadBody<CheckoutRequest>(context.Request, ct);
    if (body.StudentId is null or <= 0) throw new ApiInputException("studentId must be a positive integer.");
    if (body.EquipmentIds is null || body.EquipmentIds.Count < 2) throw new ApiInputException("Select at least two equipment items.");
    if (body.EquipmentIds.Distinct().Count() != body.EquipmentIds.Count) throw new ApiInputException("Duplicate equipment IDs are not allowed.");
    if (body.EquipmentIds.Any(id => id <= 0)) throw new ApiInputException("Equipment IDs must be positive integers.");
    if (body.ExpectedReturnDate is null) throw new ApiInputException("expectedReturnDate is required.");
    ValidateDueDate(body.ExpectedReturnDate.Value);
    var service = context.RequestServices.GetRequiredService<BorrowEquipmentService>();
    var rows = await service.BorrowMultipleAsync(body.StudentId.Value, body.EquipmentIds, body.ExpectedReturnDate.Value.ToDateTime(TimeOnly.MinValue), ct);
    var result = new List<BorrowingDto>();
    foreach (var row in rows) result.Add(await ToDtoAsync(row, app.Services, ct));
    return Results.Created("/api/v1/borrowings", new CheckoutResponse(result));
}).WithName("CreateCheckoutRequest").Accepts<CheckoutRequest>("application/json").Produces<CheckoutResponse>(201).Produces<ErrorDto>(400).Produces<ErrorDto>(404).Produces<ErrorDto>(409);

api.MapPost("/borrowings/{id:int}/return", async (int id, HttpContext context, CancellationToken ct) =>
{
    _ = await ReadBody<ReturnRequest>(context.Request, ct);
    var service = context.RequestServices.GetRequiredService<ReturnEquipmentService>();
    var borrowing = await service.ReturnAsync(id, ct);
    return Results.Ok(await ToDtoAsync(borrowing, app.Services, ct));
}).WithName("ReturnBorrowing").Accepts<ReturnRequest>("application/json").Produces<BorrowingDto>(200).Produces<ErrorDto>(400).Produces<ErrorDto>(404).Produces<ErrorDto>(409);

api.MapGet("/borrowings/{id:int}", async (int id, IBorrowingRepository repo, IServiceProvider sp, CancellationToken ct) =>
{
    var borrowing = await repo.GetByIdAsync(id, ct) ?? throw new BorrowingNotFoundError($"Borrowing {id} was not found.");
    return Results.Ok(await ToDtoAsync(borrowing, sp, ct));
}).WithName("GetBorrowingById").Produces<BorrowingDto>(200).Produces<ErrorDto>(404);

api.MapGet("/borrowings", async (HttpContext http, CancellationToken ct) =>
{
    var statusText = http.Request.Query["status"].FirstOrDefault() ?? "All";
    var sortText = http.Request.Query["sort"].FirstOrDefault() ?? "borrowedDate";
    var directionText = http.Request.Query["direction"].FirstOrDefault() ?? "asc";
    var status = statusText.ToUpperInvariant() switch
    {
        "ALL" => BorrowingStatusFilter.All,
        "ACTIVE" => BorrowingStatusFilter.Active,
        "RETURNED" => BorrowingStatusFilter.Returned,
        _ => throw new ApiInputException("status must be All, Active or Returned.")
    };
    var sort = sortText switch
    {
        "borrowedDate" => BorrowingSortField.CheckoutDate,
        "expectedReturnDate" => BorrowingSortField.ExpectedReturnDate,
        _ => throw new ApiInputException("sort must be borrowedDate or expectedReturnDate.")
    };
    if (!string.Equals(directionText, "asc", StringComparison.OrdinalIgnoreCase) && !string.Equals(directionText, "desc", StringComparison.OrdinalIgnoreCase)) throw new ApiInputException("direction must be asc or desc.");
    var text = http.Request.Query["search"].ToString();
    if (text.Length > 100) throw new ApiInputException("Search must be 100 characters or fewer.");
    var repo = http.RequestServices.GetRequiredService<IBorrowingRepository>();
    var records = await repo.SearchAsync(text, status, sort, directionText.Equals("desc", StringComparison.OrdinalIgnoreCase), ct);
    return Results.Ok(records.Select(ToBorrowingDto));
}).WithName("SearchBorrowings").Produces<BorrowingDto[]>(200).Produces<ErrorDto>(400);

api.MapGet("/students/{id:int}/borrowings", async (int id, IStudentRepository students, IBorrowingRepository borrowings, CancellationToken ct) =>
{
    _ = await students.GetByIdAsync(id, ct) ?? throw new StudentNotFoundError($"Student {id} was not found.");
    return Results.Ok((await borrowings.GetHistoryAsync(studentId: id, cancellationToken: ct)).Select(ToBorrowingDto));
}).WithName("GetStudentBorrowings").Produces<BorrowingDto[]>(200).Produces<ErrorDto>(404);

api.MapGet("/equipment/{id:int}/borrowings", async (int id, IEquipmentRepository equipment, IBorrowingRepository borrowings, CancellationToken ct) =>
{
    _ = await equipment.GetByIdAsync(id, ct) ?? throw new EquipmentNotFoundError($"Equipment {id} was not found.");
    return Results.Ok((await borrowings.GetHistoryAsync(equipmentId: id, cancellationToken: ct)).Select(ToBorrowingDto));
}).WithName("GetEquipmentBorrowings").Produces<BorrowingDto[]>(200).Produces<ErrorDto>(404);

await app.RunAsync();

static async Task<T> ReadBody<T>(HttpRequest request, CancellationToken cancellationToken)
{
    if (!request.HasJsonContentType()) throw new ApiInputException("Content-Type must be application/json.");
    try
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
        using var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
        return document.RootElement.Deserialize<T>(options) ?? throw new ApiInputException("A JSON request body is required.");
    }
    catch (JsonException) { throw new ApiInputException("The JSON request body is invalid."); }
    catch (InvalidOperationException) { throw new ApiInputException("The JSON request body is invalid."); }
}

static void ValidateDueDate(DateOnly dueDate)
{
    if (dueDate == default || dueDate <= DateOnly.FromDateTime(DateTime.Today)) throw new ApiInputException("Expected return date must be later than today.");
}

static StudentDto ToStudentDto(Student s) => new(s.Id, s.StudentNumber, s.Name, s.Contact, s.IsAllowedToBorrow);
static EquipmentDto ToEquipmentDto(Equipment e) => new(e.Id, e.EquipmentNumber, e.Name, e.Description, e.IsAvailable, e.Type);
static BorrowingDto ToBorrowingDto(BorrowingHistoryRecord b) => new(b.BorrowingId, b.StudentId, b.EquipmentId,
    DateOnly.FromDateTime(b.BorrowedOn), DateOnly.FromDateTime(b.ExpectedReturnOn), b.ReturnedOn is null ? null : DateOnly.FromDateTime(b.ReturnedOn.Value), b.Status.ToString(), b.StudentName, b.EquipmentName);

static async Task<BorrowingDto> ToDtoAsync(Borrowing b, IServiceProvider sp, CancellationToken ct)
{
    var students = sp.GetRequiredService<IStudentRepository>();
    var equipment = sp.GetRequiredService<IEquipmentRepository>();
    var student = await students.GetByIdAsync(b.StudentId, ct) ?? throw new StudentNotFoundError("Related student was not found.");
    var item = await equipment.GetByIdAsync(b.EquipmentId, ct) ?? throw new EquipmentNotFoundError("Related equipment was not found.");
    return new BorrowingDto(b.Id, b.StudentId, b.EquipmentId, DateOnly.FromDateTime(b.BorrowedOn),
        DateOnly.FromDateTime(b.ExpectedReturnOn), b.ReturnedOn is null ? null : DateOnly.FromDateTime(b.ReturnedOn.Value),
        b.Status.ToString(), student.Name, item.Name);
}

static (int Status, string Code, string Message) MapError(Exception ex) => ex switch
{
    ApiInputException => (400, "invalid_request", ex.Message),
    BadHttpRequestException or JsonException => (400, "invalid_request", "The JSON request body is invalid."),
    ArgumentException => (400, "invalid_request", ex.Message),
    StudentNotFoundError or EquipmentNotFoundError or BorrowingNotFoundError => (404, "not_found", ex.Message),
    DuplicateIdentifierError => (409, "duplicate_identifier", ex.Message),
    StudentNotAllowedToBorrowError or EquipmentNotAvailableError or BorrowingLimitExceededError or BorrowingAlreadyReturnedError => (409, "conflict", ex.Message),
    CommitOutcomeUnknownError => (500, "outcome_unknown", ex.Message),
    DbUpdateException db when IsUniqueViolation(db) => (409, "duplicate_identifier", "An identifier or active borrowing conflicts with an existing record."),
    DbUpdateException => (500, "server_error", "The server could not complete the database change. Refresh before deciding whether to retry."),
    _ => (500, "server_error", "The server could not complete the request. Check server configuration and try again.")
};

static bool IsUniqueViolation(DbUpdateException exception)
{
    for (var current = exception.InnerException; current is not null; current = current.InnerException)
        if (current is PostgresException postgres && postgres.SqlState == "23505") return true;
    return false;
}

sealed class ApiInputException(string message) : Exception(message);
