using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new() { ValidateIssuer=true, ValidateAudience=true, ValidateLifetime=true, ValidateIssuerSigningKey=true, ValidIssuer=jwt["Issuer"], ValidAudience=jwt["Audience"], IssuerSigningKey=key });
builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);
var app = builder.Build();
app.Use(async (context,next) => { var started=DateTime.UtcNow; try { await next(); } catch(Exception ex) { app.Logger.LogError(ex,"Error no controlado en gateway"); context.Response.StatusCode=500; await context.Response.WriteAsJsonAsync(new { mensaje="Error interno del Gateway", codigo="GATEWAY_ERROR" }); } finally { app.Logger.LogInformation("AUDIT {Method} {Path} -> {Status} en {Ms} ms",context.Request.Method,context.Request.Path,context.Response.StatusCode,(DateTime.UtcNow-started).TotalMilliseconds); } });
app.UseWhen(c => c.Request.Path == "/auth/token", auth => auth.Run(async context => {
    if (!HttpMethods.IsPost(context.Request.Method)) { context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed; return; }
    var login = await context.Request.ReadFromJsonAsync<Login>();
    if (login is null || login.Usuario != "admin" || login.Clave != "desafio2") { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return; }
    var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], [new Claim(ClaimTypes.Name,"admin"), new Claim(ClaimTypes.Role,"api-user")], expires:DateTime.UtcNow.AddHours(4), signingCredentials:new SigningCredentials(key,SecurityAlgorithms.HmacSha256));
    await context.Response.WriteAsJsonAsync(new { access_token=new JwtSecurityTokenHandler().WriteToken(token), token_type="Bearer", expires_in=14400 });
}));
await app.UseOcelot();
app.Run();
record Login(string Usuario,string Clave);
