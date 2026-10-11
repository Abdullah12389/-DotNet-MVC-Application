var builder = WebApplication.CreateBuilder(args);
string? connectionString = builder.Configuration.GetConnectionString("SqlDatabaseString");

builder.Services.AddScoped(provider=>new StudentRepository(connectionString!));
builder.Services.AddControllersWithViews();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Student}/{action=Public}/")
    .WithStaticAssets();


app.Run();
