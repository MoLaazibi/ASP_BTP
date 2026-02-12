using AP.BTP.Application.Interfaces;
using AP.BTP.Infrastructure.Contexts;
using AP.BTP.Infrastructure.Email;
using AP.BTP.Infrastructure.Repositories;
using AP.BTP.Infrastructure.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AP.BTP.Infrastructure.Extensions
{
    public static class Registrator
    {
        public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterDbContext(configuration);
            services.RegisterRepositories();

            services.Configure<EmailConfiguration>(configuration.GetSection("EmailSettings"));

            return services;
        }

        public static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BTPContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("BTPdb"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    )
                )
            );

            return services;
        }

        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<INurserySiteRepository, NurserySiteRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IEmployeeTaskRepository, EmployeeTaskRepository>();
            services.AddScoped<ITaskListRepository, TaskListRepository>();
            services.AddScoped<IGroundPlanRepository, GroundPlanRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IZoneRepository, ZoneRepository>();
            services.AddScoped<ITreeTypeRepository, TreeTypeRepository>();
            services.AddScoped<ITreeImageRepository, TreeImageRepository>();
            services.AddScoped<IInstructionRepository, InstructionRepository>();
            services.AddScoped<IUnitofWork, UnitofWork>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            return services;
        }
    }
}
