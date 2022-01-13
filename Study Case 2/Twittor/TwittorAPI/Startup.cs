using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwittorAPI.GraphQL;
using TwittorAPI.Models;
using static TwittorAPI.Data.PrepDB;

namespace TwittorAPI
{

    public class Startup
    {
        public IConfiguration Configuration { get; }

        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {


            var conString = Configuration.GetConnectionString("MyDatabase");
            if (_env.IsProduction())
            {
                Console.WriteLine("--> Using Sql Server Db");
                services.AddDbContext<TwittorDBContext>(opt => opt.UseSqlServer(
                    Configuration.GetConnectionString(conString)
                ));
            }
            else
            {
                Console.WriteLine("--> Using Local Db");


                services.AddDbContext<TwittorDBContext>(options =>
                     options.UseSqlServer(conString));

            }


            services
              .AddGraphQLServer()
              .AddQueryType<Query>()
              .AddMutationType<Mutation>()
              .AddAuthorization();

            services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));
            services.Configure<KafkaSettings>(Configuration.GetSection("KafkaSettings"));


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidIssuer = Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
                       ValidateIssuer = true,
                       ValidAudience = Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
                       ValidateAudience = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),
                       ValidateIssuerSigningKey = true
                   };

               });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            PrepDb.PrepPopulation(app, env.IsProduction());
        }
    }
}
