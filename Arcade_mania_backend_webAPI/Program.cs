using Arcade_mania_backend_webAPI.Models;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace Arcade_mania_backend_webAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext regisztrálása DI-be
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<ArcadeManiaDatasContext>(options =>
                options.UseMySQL(connectionString!));   // ⬅️ NINCS MySqlServerVersion, csak UseMySQL

            // CORS beállítás React frontendhez
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactCorsPolicy", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://localhost:3000"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("ReactCorsPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
