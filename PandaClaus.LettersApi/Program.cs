using PandaClaus.LettersApi;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<GoogleSheetsClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/letters", async (GoogleSheetsClient client) => await client.FetchLetters()).WithOpenApi();

app.MapPost("/letters/assign", async (LetterAssignment assignment, GoogleSheetsClient client) =>
{
    var letter = await client.FetchLetter(assignment.RowNumber);

    if (letter.IsHidden || letter.IsAssigned)
    {
        return Results.BadRequest("Ten list nie może zostać przypisany");
    }

    await client.AssignLetter(assignment);
    return Results.Ok();
});

app.Run();
