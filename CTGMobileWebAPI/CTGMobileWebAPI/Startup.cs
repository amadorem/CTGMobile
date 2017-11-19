using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CTGMobileWebAPI.Filters;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Caching.Distributed;
using CTGMobileWebAPI.Services;

namespace CTGMobileWebAPI
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

            services.AddMvc(opts =>
            {
                opts.Filters.Add(typeof(AdalTokenAcquisitionExceptionFilter));
            });

            services.AddAuthorization(o =>
            {
            });

            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("Authentication"));

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(opts =>
            {
                Configuration.GetSection("Authentication").Bind(opts);

                opts.Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = async ctx =>
                    {
                        HttpRequest request = ctx.HttpContext.Request;
                        string currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
                        var credential = new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

                        IDistributedCache distributedCache = ctx.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();

                        string userId = ctx.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                        var cache = new AdalDistributedTokenCache(distributedCache, userId);

                        var authContext = new AuthenticationContext(ctx.Options.Authority, cache);

                        AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                            ctx.ProtocolMessage.Code, new Uri(currentUri), credential, ctx.Options.Resource);

                        ctx.HandleCodeRedemption(result.AccessToken, result.IdToken);
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
