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
using Microsoft.AspNetCore.SignalR;
using VoiceMap_API.AppClasses;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

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
            var key = Encoding.ASCII.GetBytes("mLvcPoWKSTQi1fCiqqCrpBvd3mTTvvzB"); 
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/notificationHub"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

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
            services.AddScoped<INotifications, NotificationRepo>();
            services.AddScoped<IGroups, GroupsRepo>();
            services.AddScoped<IGroupMembers, GroupMembersRepo>();
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            services.AddSignalR();

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

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notificationHub");
            });
        }
    }
}
