using CT.Postojna;
using CT.Postojna.Components;
using CT.Postojna.Models;
using CT.Postojna.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<DbConnection>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
app.UseHttpsRedirection();

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
