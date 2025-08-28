using Microsoft.EntityFrameworkCore;

using LBV_GTM_Database_API.Data;
using LBV_GTM_Database_API.Services;
using LBV_GTM_Database_API.Services.Interfaces;
using LBV_GTM_Database_API.EfcContext;
using LBV_Basics.Configs;
using LBV_Basics;



Scripts.CreateDirectories();
SqlConnectionConfig.LoadJson();
SqlConnectionConfig? sqlConCfg = SqlConnectionConfig.GetActiveConnection();
if (sqlConCfg is null) { sqlConCfg = SqlConnectionConfig.List[0]; sqlConCfg.IsActive = true; }

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options => options.UseLazyLoadingProxies().UseSqlServer(sqlConCfg.ConnectionString));

builder.Services.AddScoped(typeof(BaseService<>));
builder.Services.AddScoped(typeof(IBaseContext<>), typeof(BaseContext<>));
builder.Services.AddScoped(typeof(FullService<>));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<ITicketContext, TicketContext>();

/*

builder.Services.AddScoped<Service>();
builder.Services.AddScoped<IContext, Context>();
*/

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
