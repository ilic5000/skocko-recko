using System.Text;

namespace Skocko.Api.Middlewares
{
    public class SwaggerAuthorizedMiddleware
    {
        private readonly RequestDelegate _next;
        public IConfiguration Configuration { get; }
        private string _letMeInPass;

        public SwaggerAuthorizedMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            Configuration = configuration;

            _letMeInPass = Configuration.GetValue<string>("PasswordDevOptions");

            if (_letMeInPass == null)
            {
                // Default pass
                _letMeInPass = "mellon";
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                if (!context.Request.Cookies.ContainsKey("pass")
                    || context.Request.Cookies["pass"] != _letMeInPass)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync("{\"text\":\"NEЋЕ МОЋИ\"}", Encoding.UTF8);

                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}
