using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using FacturacionHN.Data;
using FacturacionHN.Services;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Facturación Honduras API", Version = "v1" });
});
builder.Services.AddDbContext<FacturacionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IFacturacionService, FacturacionService>();
builder.Services.AddSingleton<FacturaPdfService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Facturación HN v1"));

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
