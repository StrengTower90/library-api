using LibraryAPI;
using LibraryAPI.Data;
using LibraryAPI.Entities;
using LibraryAPI.Services;
using LibraryAPI.Swagger;
using LibraryAPI.Utilities;
using LibraryAPI.Utilities.V1;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilderABC(args);

/* Dictionary on memory */
//var dictionaryConfigurations = new Dictionary<string, string>
//{
//    {"who_ami", "a dictionary in memory" }
//};

//builder.Configuration.AddInMemoryCollection(dictionaryConfigurations!);

/* End Dictionary on memory */

/* Services Area */

//configuring OutputCache to manage Cache
/* the code below doesn't work when redis is integrated
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
}); */

//OutputCache with redis
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

// Adding the Data  protection, this services add the minimal config
builder.Services.AddDataProtection();

// Configure  CORS
var permittedOrigins = builder.Configuration.GetSection("permittedOrigins").Get<string[]>()!;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(optionsCORS =>
    {
        // To allow any origin
        //optionsCORS.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        optionsCORS.WithOrigins(permittedOrigins).AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("total-numbers-records");
    });
});

    /* -> Pattern on options */
    // the configuration below mapp the section values config into the PersonOptions Class 
    //builder.Services.AddOptions<PersonOptions>()
    //    .Bind(builder.Configuration.GetSection(PersonOptions.Section))
    //    .ValidateDataAnnotations() // enable the DataAnnotations validations
    //    .ValidateOnStart(); // start validations

    //builder.Services.AddOptions<RatesOptions>()
    //    .Bind(builder.Configuration.GetSection(RatesOptions.Section))
    //    .ValidateDataAnnotations()
    //    .ValidateOnStart();

    //builder.Services.AddSingleton<ProccesingPayments>();
     

/* Use AddJsonOptions before to start using DTOs to avoid cicle entity navigation prop
builder.Services.AddControllers().AddJsonOptions(options => 
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles); // enable the controllers system
*/
builder.Services.AddAutoMapper(typeof(Program)); // Configure AutoMapper to the current Project to work with DTOs mapping

// to start using JSON Patch serialization for input / output on Patch Method
builder.Services.AddControllers(options =>
{
    options.Filters.Add<FilterTimeExecution>();
    options.Conventions.Add(new ConventionGroupByVersion());
}).AddNewtonsoftJson();

//builder.Services.AddApiVersioning()

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("name=DefaultConnection"));

builder.Services.AddIdentityCore<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // this services allows identity services connect with the tables in the Data base
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<User>>(); // Allow manage the user registration
builder.Services.AddScoped<SignInManager<User>>(); // Allow user authentication
builder.Services.AddTransient<IServicesUser, ServicesUser>(); // We don't need to share state
builder.Services.AddTransient<IHashService, HashService>(); // We don't share state
builder.Services.AddTransient<IFileStorage, StorageLocalFiles>();
builder.Services.AddScoped<MyActionFilter>();// 
builder.Services.AddScoped<FilterBookValidation>();
builder.Services.AddScoped<LibraryAPI.Services.v1.IServiceAuthors,
        LibraryAPI.Services.v1.ServiceAuthors>();

// HATEOAS Filters
builder.Services.AddScoped<LibraryAPI.Services.v1.IGeneradorEnlaces, 
    LibraryAPI.Services.v1.GeneradorEnlaces>();

builder.Services.AddScoped<HATEOASAuthorAttribute>();
builder.Services.AddScoped<HATEOASAuthorsAttribute>();
//builder.Services.AddScoped<AddHeadersAttributeFilter>();

builder.Services.AddHttpContextAccessor(); // allow indentity to access the Http context 

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // entity framework core usually change the claim name for other set it to false disable that

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = 
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["keyjwt"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Adding a new policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("esadmin", policy => policy.RequireClaim("esadmin"));
});

builder.Services.AddSwaggerGen(options =>
{
    // Add additional swagger configurations
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Library API",
        Description = "This is a web api to work with authors and books datas",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Email = "l.escalante.c90@gmail.com",
            Name = "Luis Escalante",
            Url = new Uri("https://strengtower90.github.io/mi_portafolios/")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v2",
        Title = "Library API",
        Description = "This is a web api to work with authors and books datas",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Email = "l.escalante.c90@gmail.com",
            Name = "Luis Escalante",
            Url = new Uri("https://strengtower90.github.io/mi_portafolios/")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    // we gonna use the configure below

    options.OperationFilter<FilterAuthorization>();

    // instead of using the configure below

    //options.AddSecurityRequirement(new OpenApiSecurityRequirement
    //{
    //    {
    //        new OpenApiSecurityScheme
    //        {
    //            Reference = new OpenApiReference
    //            {
    //                Type = ReferenceType.SecurityScheme,
    //                Id = "Bearer"
    //            }
    //        },
    //        new string[]{}
    //    }
    //});
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

/* End Service Area */

/* Middlewares Area */

app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error!;

    var error = new Error()
    {
        MessageError = exception.Message,
        StrackTrace = exception.StackTrace,
        Date = DateTime.UtcNow
    };

    var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
    dbContext.Add(error);
    await dbContext.SaveChangesAsync();
    await Results.InternalServerError(new
    {
        type = "error",
        message = "Has ocurred an inesperate error",
        status = 500
    }).ExecuteAsync(context);
}));

// The next middleware will execute for any request to the web API
app.Use(async (context, next) =>
{
    /* by default this header won't be accessed from a browser specially those who use JavaScript
     to expose that header we need to additionally add some configure into the cors configure at the top */
    context.Response.Headers.Append("my-header", "value");

    await next();
});

app.UseSwagger(); // Specially use to serve the swagger document the web api routes
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
    opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "Library API V2");
}); 
// Provide a User interfaces to interact with the swagger doc. web apis

// serve static files from wwwroot
app.UseStaticFiles();

// Enable Cors
app.UseCors();

// Enable OutputCache
app.UseOutputCache();

app.MapControllers(); // Redirect any request to the controllers app

app.Run();

public partial class Program { }
