using Google.Api;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var pathToCredentials = "aire_firebase_private_key.json";
var credentials = GoogleCredential.FromFile(pathToCredentials);

// Initialize Firestore with the project ID and credentials
string projectId = "a-level-pro";
FirestoreDb firestoreDb = FirestoreDb.Create(projectId, new FirestoreClientBuilder
{
    CredentialsPath = pathToCredentials
}.Build());

// Add the FirestoreDb instance to the services
builder.Services.AddSingleton(firestoreDb);
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
var httpClient = app.Services.GetRequiredService<HttpClient>();

app.Run();
