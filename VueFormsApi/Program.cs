using VueFormsApi.DataStructures;
using VueFormsApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.UseMallDataProvider();
//CORS
builder.Services.AddCors(p => p.AddPolicy("LooseCors", builder =>
{
    builder.WithOrigins("*").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("LooseCors");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
