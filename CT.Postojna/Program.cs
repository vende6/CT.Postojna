using CT.Postojna;
using CT.Postojna.Components;
using CT.Postojna.Models;
using CT.Postojna.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction() && builder.Configuration["RENDER"] == "true")
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
        options.ForwardLimit = 1;
    });
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(5, 5, 62))));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<DbConnection>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddHttpClient<StripeCheckoutService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

var demoEmail = builder.Configuration["DemoAccount:Email"];
var demoPassword = builder.Configuration["DemoAccount:Password"];
if ((demoEmail is null) != (demoPassword is null))
{
    throw new InvalidOperationException("Both DemoAccount:Email and DemoAccount:Password must be configured together.");
}

if (demoEmail is not null && demoPassword is not null)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (await userManager.FindByEmailAsync(demoEmail) is null)
    {
        var user = new ApplicationUser
        {
            UserName = demoEmail,
            Email = demoEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, demoPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Unable to create the configured demo account: {errors}");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (app.Environment.IsProduction() && app.Configuration["RENDER"] == "true")
{
    app.UseForwardedHeaders();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/activities", async (ActivityService service) => Results.Ok(await service.GetAllAsync()));
app.MapGet("/api/activities/{id:int}", async (int id, ActivityService service) =>
{
    var activity = await service.GetByIdAsync(id);
    return activity is null ? Results.NotFound() : Results.Ok(activity);
});
app.MapPost("/api/activities", async (Activity activity, ActivityService service) =>
{
    activity.Id = await service.CreateAsync(activity);
    return Results.Created($"/api/activities/{activity.Id}", activity);
});
app.MapPut("/api/activities/{id:int}", async (int id, Activity activity, ActivityService service) =>
    await service.UpdateAsync(id, activity) ? Results.NoContent() : Results.NotFound());
app.MapDelete("/api/activities/{id:int}", async (int id, ActivityService service) =>
    await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

app.MapGet("/api/persons", async (PersonService service) => Results.Ok(await service.GetAllAsync()));
app.MapGet("/api/persons/{id:int}", async (int id, PersonService service) =>
{
    var person = await service.GetByIdAsync(id);
    return person is null ? Results.NotFound() : Results.Ok(person);
});
app.MapPost("/api/persons", async (Person person, PersonService service) =>
{
    person.Id = await service.CreateAsync(person);
    return Results.Created($"/api/persons/{person.Id}", person);
});
app.MapPut("/api/persons/{id:int}", async (int id, Person person, PersonService service) =>
    await service.UpdateAsync(id, person) ? Results.NoContent() : Results.NotFound());
app.MapDelete("/api/persons/{id:int}", async (int id, PersonService service) =>
    await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

app.MapGet("/api/products", async (ProductService service) => Results.Ok(await service.GetAllAsync()));
app.MapGet("/api/products/{id:int}", async (int id, ProductService service) =>
{
    var product = await service.GetByIdAsync(id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});
app.MapPost("/api/products", async (Product product, ProductService service) =>
{
    product.Id = await service.CreateAsync(product);
    return Results.Created($"/api/products/{product.Id}", product);
});
app.MapPut("/api/products/{id:int}", async (int id, Product product, ProductService service) =>
    await service.UpdateAsync(id, product) ? Results.NoContent() : Results.NotFound());
app.MapDelete("/api/products/{id:int}", async (int id, ProductService service) =>
    await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
