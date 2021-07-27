using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using UMS.Core;
using UMS.Dto;
using UMS.Helper.JWT;

namespace UMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<DbCredential>(Configuration.GetSection("DbCredential"));
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var dependency = new Dependency(services);
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "UMS",
                    Description = "UMS API",
                    Contact = new OpenApiContact
                    {
                        Name = "Duo Infotech",
                        Email = "developer@di.com",
                    },
                });
                c.CustomSchemaIds(type => type.FullName);
                //c.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");
                //c.DocumentFilter<BasePathFilter>("/v1");
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "UMS API",
                    Description = "UMS ** API **",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, new string[] { }}
                });
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(env.IsDevelopment() ? "/error-local-development" : "/error");

            app.UseSwagger();
            app.UseSession();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSwagger(c => { c.SerializeAsV2 = true; });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("./swagger/v1/swagger.json", "UMS API");
                c.RoutePrefix = string.Empty;
                c.InjectStylesheet("./swagger-ui/custom.css");
                c.InjectJavascript("./swagger-ui/custom.js", "text/javascript");
                c.DocumentTitle = "UMS API";
            });

            app.UseCors("AllowAll");
            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
            });

            app.UseEndpoints(x => { x.MapControllers(); });
        }
    }
}
