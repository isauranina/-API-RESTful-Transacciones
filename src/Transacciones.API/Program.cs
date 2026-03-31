using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Transacciones.API;
using Transacciones.API.Ioc;
using Transacciones.API.Middlewares;
using Transacciones.Core.Mappings;
using Transacciones.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApiVersioning(config => {
	config.AssumeDefaultVersionWhenUnspecified = true;
	config.DefaultApiVersion = new ApiVersion(1, 0);
	config.ReportApiVersions = true;
	config.ApiVersionReader = ApiVersionReader.Combine(
		new QueryStringApiVersionReader("api-version"),
		new HeaderApiVersionReader("X-Version"),
		new MediaTypeApiVersionReader("ver"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
	c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
		Title = "Transacciones API",
		Version = "v1",
		Description = "API para gestión de transacciones bancarias"
	});

	// JWT en Swagger
	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
		Description = "JWT Authorization header usando el esquema Bearer. Ingresa solo el token (sin 'Bearer'). Ejemplo: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
		Name = "Authorization",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT"
	});

	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});


builder.Services.AddAutoMapper(typeof(TransaccionesMappingProfile));

builder.Services.AddTransaccionesDependencies(builder.Configuration);

builder.Services.AddDbContext<TransaccionesDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("TransaccionesConnection"),
		// para las migraciones Transacciones.Infrastructure
		sqlOptions => sqlOptions.MigrationsAssembly("Transacciones.Infrastructure")
	));

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
		builder
			.WithOrigins("http://localhost:4200", "*")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials()));

// Obtener la clave secreta de entorno (JWT_SECRET_KEY) o, en desarrollo, de appsettings / user-secrets
var jwtSecretKey =
	Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
	?? builder.Configuration["Bearer:SecretKey"]
	?? throw new InvalidOperationException("La clave secreta JWT no está configurada. Define JWT_SECRET_KEY o Bearer:SecretKey.");

builder.Services.AddAuthentication("Bearer")
				.AddJwtBearer("Bearer", options => {
					options.TokenValidationParameters = new TokenValidationParameters {
						ValidateIssuer = false,
						ValidateAudience = false,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
						ClockSkew = TimeSpan.Zero
					};

					if (builder.Environment.IsDevelopment()) {
						options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents {
							OnAuthenticationFailed = context => {
								Console.WriteLine($"Error de autenticación: {context.Exception.Message}");
								return Task.CompletedTask;
							},
							OnTokenValidated = context => {
								Console.WriteLine($"Token validado correctamente para: {context.Principal?.Identity?.Name}");
								return Task.CompletedTask;
							},
							OnChallenge = context => {
								Console.WriteLine($"Challenge: {context.Error} - {context.ErrorDescription}");
								return Task.CompletedTask;
							}
						};
					}
				});

// Configurando la autorización con esquema Bearer por defecto
builder.Services.AddAuthorization(options => {
	options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Bearer")
		.RequireAuthenticatedUser()
		.Build();
});

 builder.WebHost.UseUrls("http://0.0.0.0:5032");

var app = builder.Build();

app.UseCors("CorsPolicy");

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()) {
app.UseSwagger();
//app.UseSwaggerUI();
app.UseSwaggerUI(options => {
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
	options.RoutePrefix = string.Empty; // Esto permite ver Swagger en http://tu-ip/
});
//}

pp.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsProduction()) {
	AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
	app.Urls.Add("http://0.0.0.0:5035");

}

app.Run();
