using MagicLinkRestGate;
using MagicLinkRestGate.Classes;
using MagicLinkRestGate.InterFaces;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MlkSettings>(builder.Configuration.GetSection("MagicLinkParams"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMagoWSGateHelper, MagoWSGateHelper>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MagicLinkRestGate", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MagicLinkRestGate v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
