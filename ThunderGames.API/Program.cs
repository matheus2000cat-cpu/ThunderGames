using Microsoft.AspNetCore.Identity;
using ThunderGames.Data;
using ThunderGames.Domain;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços de Controllers e Swagger ao contêiner de DI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://thundergames-frontend.onrender.com"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("Supabase");
builder.Services.AddSingleton(new ConexaoBanco(connectionString));
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<PessoaRepository>();
builder.Services.AddScoped<IPasswordHasher<Pessoa>, PasswordHasher<Pessoa>>();

var app = builder.Build();

// Habilita a interface gráfica do Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ThunderGames API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();