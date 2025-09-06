using System.Text;
using JobPortalManagement.Data;
using JobPortalManagement.Models;
using JobPortalManagement.Services;
using JobPortalManagement.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// DB
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequireDigit = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ✅ Register HttpClientFactory (fix for TokenService error)
builder.Services.AddHttpClient();

// ✅ Authentication (JWT only for API, Cookie for MVC if needed)
builder.Services.AddAuthentication(options =>
{
    // default JWT for APIs
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
        ),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

// ✅ Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Job Portal API",
        Version = "v1",
        Description = "API documentation for Job Portal Management"
    });

    // JWT support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // ✅ Sirf [ApiController] wale show karo
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        return apiDesc.ActionDescriptor.EndpointMetadata
            .Any(em => em is Microsoft.AspNetCore.Mvc.ApiControllerAttribute);
    });
});

// ✅ Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Custom services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMenuService, MenuService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Swagger UI enable only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Job Portal API V1");
        c.RoutePrefix = "swagger";
    });
}

// Middleware pipeline
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers(); // APIs
app.MapDefaultControllerRoute(); // MVC

app.Run();
