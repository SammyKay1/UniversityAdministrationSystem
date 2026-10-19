
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        app.MapPost("/account/login", async (HttpContext context, IAntiforgery antiforgery, SignInManager<ApplicationUser> signInManager) =>
            {
                if (!await antiforgery.IsRequestValidAsync(context))
                {
                    return Results.BadRequest();
                }

                var form = await context.Request.ReadFormAsync();

                var email = form["Email"].ToString();
                var password = form["Password"].ToString();
                var remember = form["RememberMe"].ToString() == "true";

                var result = await signInManager.PasswordSignInAsync(
                    email,
                    password,
                    remember,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return Results.Redirect("/");
                }

                return Results.Redirect("/login?error=true");
            });

        app.MapPost(
            "/account/logout",
            async ( HttpContext context, IAntiforgery antiforgery,   SignInManager<ApplicationUser> signInManager) =>
            {
                if (!await antiforgery.IsRequestValidAsync(context))
                {
                    return Results.BadRequest();
                }

                await signInManager.SignOutAsync();

                return Results.Redirect("/login");
            });
    }
}
