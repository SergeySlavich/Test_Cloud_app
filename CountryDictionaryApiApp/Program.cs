using CountryDictionaryApiApp.Model;
using DockerComposeExample_App;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();

var app = builder.Build();

app.MapGet("/", () => new {Message="Server is Running"});
app.MapGet("/ping", () => new {Message="pong"});
app.MapGet("/country", async (ApplicationDbContext db) => await db.Countries.ToListAsync());
app.MapPost("/country", async (Country country, ApplicationDbContext db) =>
{
    await db.Countries.AddAsync(country);
    await db.SaveChangesAsync();
    return country;
});

//получать страну по коду (поиск среди всех кодов)
app.MapGet("/country/code/{str}", async (string str, ApplicationDbContext db) =>
{
    Country? finded = await db.Countries.FirstOrDefaultAsync(c => c.ISO31661Alpha2Code == str || c.ISO31661Alpha3Code == str || c.ISO31661NumericCode == str);
    return finded;
});

//получать страну по id
app.MapGet("/country/{id}", async (int id, ApplicationDbContext db) => await db.Countries.FirstOrDefaultAsync(c => c.Id == id));

//редактирование данных о стране
app.MapPatch("/country/{id}", async (Country country, ApplicationDbContext db, int id) => await UpdateByIdAsync(country, db, id));

//Запуск приложения
app.Run();

//9. Дописать обработчики в приложении, позволяющие:
//получать страну по коду (поиск среди всех кодов)+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//получать страну по id++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//редактирование данных о стране
//добавить проверку недопустимости повторяемых кодов, валидацию длины кодов при добавлении и редактировании
//Пересобрать контейнеры и протестировать приложение в докере.


async Task<Country> UpdateByIdAsync(Country country, ApplicationDbContext db, int id)
{
    Country? updated = await db.Countries.FirstOrDefaultAsync(c => c.Id == id);
    if (updated != null)
    {
        if (country.Name != string.Empty)
        {
            updated.Name = country.Name;
        }
        if (country.ISO31661Alpha2Code != string.Empty)
        {
            updated.ISO31661Alpha2Code = country.ISO31661Alpha2Code;
        }
        if (country.ISO31661Alpha3Code != string.Empty)
        {
            updated.ISO31661Alpha3Code = country.ISO31661Alpha3Code;
        }
        if (country.ISO31661NumericCode != string.Empty)
        {
            updated.ISO31661NumericCode = country.ISO31661NumericCode;
        }
        await db.SaveChangesAsync();
        return updated;
    }
    else
    {
        await db.Countries.AddAsync(country);
        await db.SaveChangesAsync();
        return country;
    }
}
