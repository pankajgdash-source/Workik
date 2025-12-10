using FellowOakDicom;
using FellowOakDicom.Imaging.NativeCodec;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlayBook3DTSL.API.Middleware;
using PlayBook3DTSL.API.ServiceInstallers;
using PlayBook3DTSL.API.ServiceInstallers.Base;
using PlayBook3DTSL.Database.DataContext;
using PlayBook3DTSL.Services.Helper;
using PlayBook3DTSL.Utilities.Helpers;
using PlayBook3DTSL.Utilities.ServiceModel;
using Serilog;
using Serilog.Events;
using System.Text;
using WebApp.Services;


string[] parameters = {
    "https://opplaybookscmsdev.azurewebsites.net",
    "https://opplaybookuidev.azurewebsites.net",
    "https://opplaybookapidev.azurewebsites.net",
     "https://opplaybookscmsqa.azurewebsites.net",
    "https://opplaybookuiqa.azurewebsites.net",
    "https://opplaybookapiqa.azurewebsites.net",
    "http://localhost:4200",
    "https://localhost:7296",
    "https://opplaybookapidev.azurewebsites.net/hangfire",
    "https://opplaybookapiqa.azurewebsites.net/hangfire",
    "https://opplaybookapiuat.azurewebsites.net/hangfire",
    "https://3dtsluidev.azurewebsites.net",
    "https://3dtslapidev.azurewebsites.net",
    "https://apidev.3dtsl.com",
    "https://uidev.3dtsl.com",
    "https://3dtsluidev.azurewebsites.net"
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region Swagger

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Playbook 3DTSL API",
        Description = "Playbook 3DTSL API",
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); //This line

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

});

#endregion


string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHostedService(sp =>
    new DatabaseHealthCheckService(sp.GetRequiredService<ILogger<DatabaseHealthCheckService>>(), connectionString));

builder.Services.AddDbContext<PlayBook3DTSLDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    op => op.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<DicomService>();
// Update the logging configuration
Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.Console(LogEventLevel.Error) // Ensure the Serilog.Sinks.Console package is installed
        .WriteTo.RollingFile("Logs/MainLog-{Date}.txt", LogEventLevel.Error)
        .CreateLogger();
//
// Create IEnumerable KeyPair
//
var dictionary = new[]
{
    new KeyValuePair<string, string>("ConnectionString", builder.Configuration.GetConnectionString("DefaultConnection"))
    //new KeyValuePair<string, string>("ConnectionString", builder.Configuration.GetConnectionString("SDLDefaultConnection"))
};
//
// Add IEnumerable KeyPair
//
builder.Configuration.AddInMemoryCollection(dictionary);

//Register dependency for Dicom image capture library.


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(2);
    hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = long.MaxValue;
    o.MultipartBoundaryLengthLimit = int.MaxValue;
    o.MultipartHeadersCountLimit = int.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
    o.BufferBodyLengthLimit = long.MaxValue;
    o.BufferBody = true;
    o.ValueCountLimit = int.MaxValue;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Authorization"); // Expose the Authorization header
        options.AddPolicy("StaticFileCorsPolicy", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .WithHeaders("application/dicom")
                   .WithExposedHeaders()
                   .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    });
});

builder.Services.InstallServices(builder.Configuration);
builder.Services.LernenderServices(builder.Configuration);
builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection("FilePath"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["AppSettings:JwtIssuer"],
        ValidAudience = builder.Configuration["AppSettings:JwtAudience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(2);
    hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = long.MaxValue;
    o.MultipartBoundaryLengthLimit = int.MaxValue;
    o.MultipartHeadersCountLimit = int.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
    o.BufferBodyLengthLimit = long.MaxValue;
    o.BufferBody = true;
    o.ValueCountLimit = int.MaxValue;
});

var app = builder.Build();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
// NOTE: Add new mappings
provider.Mappings[".DCM"] = "application/dicom";
// NOTE: replace the line app.UseStaticFiles(); with this block of code
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// Configure the HTTP request pipeline.
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseDeveloperExceptionPage();
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("../swagger/v1/swagger.json", "Playbook3DTSL-API");
        c.RoutePrefix = string.Empty;
    });
}



app.UseCors(x => x
    .WithOrigins(parameters)
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
app.UseCors("AllowAll");
app.UseCors("StaticFileCorsPolicy");
app.UseHttpsRedirection();
app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<ApiVersionMiddleware>();
app.MapControllers();

app.Run();
