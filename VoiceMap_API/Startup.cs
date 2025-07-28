using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoiceMap_API.Repositories;
using VoiceMap_API.Repositories.Interface;
using AutoMapper;
using VoiceMap_API.Models;

namespace VoiceMap_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDevClient",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                                .AllowCredentials();
            });
            });

            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            services.AddSwaggerGen();

            services.AddDbContext<AppDbContext.AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddHttpContextAccessor();
            services.AddScoped<ISignUp, SignUpRepo>();
            services.AddScoped<IUserVerification, UserVerificationRepo>();
            services.AddScoped<IUserLoginLogs, UserLoginLogsRepo>();
            services.AddScoped<IExpertiseType, ExpertiseTypeRepo>();
            services.AddScoped<IProfileType, ProfileTypeRepo>();
            services.AddScoped<IUserProfiles, UserProfilesRepo>();
            services.AddScoped<IUserSecuritySettings, UserSecuritySettingsRepo>();
            services.AddScoped<IReactionTypes, ReactionTypesRepo>();
            services.AddScoped<IPosts, PostRepo>();
            services.AddScoped<IPostCategories, PostCategoriesRepo>();
            services.AddScoped<IPostReactions, PostReactionsRepo>();
            services.AddScoped<IPostComments, PostCommentsRepo>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAngularDevClient");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty; // So swagger opens at https://localhost:<port>/
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
