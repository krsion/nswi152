using System.Text;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

string connectionString = app.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")!;

try
{
    // Table would be created ahead of time in production
    using var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand(
        "CREATE TABLE Persons (ID int NOT NULL PRIMARY KEY IDENTITY, FirstName varchar(255), LastName varchar(255));",
        conn);
    using SqlDataReader reader = command.ExecuteReader();
}
catch (Exception e)
{
    // Table may already exist
    Console.WriteLine(e.Message);
}

app.MapGet("/Person", () => {
    var html = new StringBuilder("<html><body><table border=1>");

    using var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand("SELECT * FROM Persons", conn);
    using SqlDataReader reader = command.ExecuteReader();

    if (reader.HasRows)
    {
        while (reader.Read())
        {
            html.Append($"<tr><td>{reader.GetInt32(0)}</td><td>{reader.GetString(1)}</td><td>{reader.GetString(2)}</td></tr>");
        }
    }

    html.Append("</table></body></html>");

    return Results.Content(html.ToString(), "text/html");
});

app.MapPost("/Person", (Person person) => {
    using var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand(
        "INSERT INTO Persons (firstName, lastName) VALUES (@firstName, @lastName)",
        conn);

    command.Parameters.Clear();
    command.Parameters.AddWithValue("@firstName", person.FirstName);
    command.Parameters.AddWithValue("@lastName", person.LastName);

    using SqlDataReader reader = command.ExecuteReader();
});

app.Run();