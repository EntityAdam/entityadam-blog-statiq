---
Title: "Razor Components Authentication with Azure AD"
Published: 2019-03-15 12:00:00 -0500
Image: "/posts/img/adam-avatar-final-trans-400.png"
Tags: [C#, ASP.NET Core, .NET Core, Blazor]
IsPost: False
---

`[Obsolete]` Getting started with Razor Components, Adding OpenID Connect Authentication.

# Overview
1. Azure Account (Not Covered)
2. Azure Active Directory (Not Covered)
3. Azure AD App Registration
4. Scaffold Razor Components App
5. Code Configuration
6. Sign In / Sign Out Controller
7. Razor Component

------

## 3 Azure AD App Registration
Steps
 - Sign into your Azure Portal and `navigate` to App Registrations or App Registrations (Preview)
 - Create a new App Registration
   - Enter any name
   - Choose supported account type (for this, I chose *Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)*)   
   - Enter your redirect uri, as `https://localhost:<your ssl port>/auth/signin-callback`
   
> **Note:** you can find your SSL port in the `MyApp.Server` project properties, under the debug tab, or in your `launchsettings.json` file.

> **Note:** You can name the Redirect Uri as anything you'd like, but it needs to match the call back uri provided to authentication provider. Also, you cannot have a controller handle this route or you will get a `Correlation` exception. Example: If you have a `SignIn()` method on `AuthController`, and specify a redirect uri of `/auth/signin/`, you will get an exception.
 
--- 
## 4 Scaffold Razor Components App

Using Visual Studio 2019 Preview you can create a new ASP.NET Core Web Application and simply choose the Razor Components template.

--- 

## 5 Code Configuration

### 5.1 Startup Class - Constructor
In the startup class, one of the first things I'm doing is creating a constructor to grab an instance of `IConfiguration` so we can retrieve our Azure AD configuration from the `appsetting.json` file



```cs
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
}
```

---

### 5.2 Startup Class -> ConfigureServices

Moving to the `ConfigureServices` method, we'll need to register the authentication provider with the `IServiceCollection`

### 5.2.1 Add MVC and Authentication

```cs
services.AddMvc();

services.AddAuthentication();
```

### 5.2.1 Add OpenID Connect
In this example, I'm going to use OpenID Connect so we'll tack that on to the builder.

```cs
services.AddAuthentication()
    .AddOpenIdConnect(options =>
    {
       //configure here
    });
```
> **What is OpenID Connect?**  
More info here -> [https://openid.net/connect/](https://openid.net/connect/)

> **NuGet Package:** Microsoft.AspNetCore.Authentication.OpenIdConnect   
Make sure you have the correct version installed, at the time of writing, I had to use the second to most recent which was `3.0.0-preview-19075-0444`

> **MissingMethod Exception:**
If you get any exception complaining about a Missing Method, you've got a dependency version mismatch


### 5.2.2 Configuring OpenID Connect

This is where we tell OpenIdConnect about our Azure and since there are a lot of options I'm going to go with a very basic setup. If you need something different for your application, check out the Pluralsight course in the notes section below.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication()
        .AddOpenIdConnect(options =>
        {
            options.Authority = Configuration.GetValue<string>("AzureOpenIdConnect:Authority");
            options.ClientId = Configuration.GetValue<string>("AzureOpenIdConnect:ClientId");
            //etc...
        });
}
```

Wait wait, that looks terrible.  Let's try binding our configuration instead. It's less cluttered and more idiomatic of .NET Core.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication()
            .AddOpenIdConnect(options => Configuration.Bind("AzureOpenIdConnect", options))
}
```

And put all our options into `appsetting.json` like this
```json
{
  "AzureOpenIdConnect": {
    "Authority": "https://login.microsoftonline.com/{yourdomain}.onmicrosoft.com",
    "ClientId": "{{ your-client-id }}",
    "ResponseType": "id_token",
    "CallbackPath": "/auth/signin-callback"
  }
}
```

### 5.2.3 Configuring Authentication

This is where we tell `Authentication` which process to use to find out if a user is eligible to access the application.

```cs
services.AddAuthentication(options => {
            //configure
        })
        .AddOpenIdConnect(options => Configuration.Bind("AzureOpenIdConnect", options));
```

I'm going to use simple **Cookie Authentication** in this example, and that looks like this:

```cs
services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddOpenIdConnect(options => Configuration.Bind("AzureOpenIdConnect", options))
        .AddCookie();
```
> **Note:**  Don't forget the `.AddCookie()`. But if you do, don't worry. You'll get a very friendly exception asking if you forgot it.

> **Note:** If you need more info on configuring authentication, check out this [Pluralsight course by Chris Klug](https://app.pluralsight.com/library/courses/aspnet-core-identity-management-playbook/table-of-contents)


### 5.2.4 Making it all work in Razor Components

The last thing we'll need is something a little bit funky that I'll try to summarize as best as I can. Blazor (Client Side) comes packed with Http stuff, but Razor Components out of the box doesn't have any Http stuff on the client app.  So, to be able access critical info from the client side, we'll need to expose the `HttpClient` and the `HttpContextAccessor` from the server side to get any information about the currently logged in user.

So, still in the `ConfigureServices` method, we need to add

```cs
services.AddHttpContextAccessor();
services.AddScoped<HttpContextAccessor>();
services.AddHttpClient();
services.AddScoped<HttpClient>();
```

### 5.3 Startup -> Configure()

Now everything is registered with the `ServiceCollection` and configured, but don't forget to `.Use()` them!

In the `Configure()` method of the `Startup` class, we'll need to add these things to our `AppBuilder`, in a specific order.

`UseAuthentication()` should be after `UseStaticFiles()` and before `UseMvcWithDefaultRoute()`

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseMvcWithDefaultRoute();
    app.UseRazorComponents<App.Startup>();
}
```

---

### 6 Sign In / Sign Out Controller

Next, we need a controller in our `ProjectName.Server` project to handle our sign in and sign out.  This step is actually not required if using the **AzureAD.UI NuGet Package** (Not Covered) because it actually creates a virtual controller which handles this stuff for you.  But since I'm not a big fan of using automagical things without knowing what's going on under the hood, I'm sharing the OpenIdConnect method first.

```cs
[Route("auth")]
public class AuthController : Controller
{
    [Route("signin")]
    public IActionResult SignIn(string returnUrl = null)
    {
        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" });
    }

    [HttpPost]
    [Route("signout")]
    public async Task SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
}
```

Nothing too fancy here.  There are a few takeaways though.  If you don't pass in the `RedirectUri` into the `Challenge`, you'll catch your self in an infinite loop. Don't do it. Next, it's important in the sign out feature to not only log your users out of the app, but also out of the authentication provider, or else people will lose their minds when they find out they were not actually signed out.  Also, in ensuring your users are logged out of their authentication provider, you're not responsible for returning a redirect `ActionResult`, the service will log you out and return the user to your site (and you can even tell it where, by specifying a *Post Logout* uri in the configuration)

---

## 7 Razor Component

Next, we'll need an actual Razor Component! Under pages, I created a new Component named `LoginControl`, and here's the code. (Apologies about the syntax highlighting, not supported yet.)

```text
@using System.Security.Claims
@using Microsoft.AspNetCore.Http
@page "/login"
@inject HttpClient Http
@inject HttpContextAccessor _httpContextAccessor

@if (User.Identity.Name != null)
{
    <b>You are logged in as: @GivenName</b>
    @*<a class="ml-md-auto btn btn-primary"
        href="/auth/signout?post_logout_redirect_uri=@RedirectUri"
        target="_top">Logout</a>*@
    <form class="ml-md-auto" method="post" action="/auth/signout">
        <input class="btn btn-primary" type="submit" value="Sign Out" />
    </form>
}
else
{
    <a class="ml-md-auto btn btn-primary"
       href="/auth/signin?redirectUri=@RedirectUri"
       target="_top">Login</a>
}
@functions {
    private ClaimsPrincipal User;
    private string RedirectUri;
    private string GivenName;
    protected override void OnInit()
    {
        base.OnInit();
        try
        {
            // Set the user to determine if they are logged in
            User = _httpContextAccessor.HttpContext.User;
            // Try to get the GivenName
            var givenName =
                _httpContextAccessor.HttpContext.User
                .FindFirst(ClaimTypes.GivenName);
            if (givenName != null)
            {
                GivenName = givenName.Value;
            }
            else
            {
                GivenName = User.Identity.Name;
            }
            // Need to determine where we are to set the RedirectUri
            RedirectUri =
                _httpContextAccessor.HttpContext.Request.Host.Value;
        }
        catch { }
    }
}
```

So at the top you can see we're injecting `HttpClient` and `HttpContextAccessor` which I stated during the last portion of `ConfigureServices` that is what gives us access to retrieve the currently logged in user (`ClaimsPrincipal`) from the HttpContext of the Server.

Lastly, we'll just consume the `LoginControl` from the main layout component.

So under `/Shared/MainLayout.cshtml` you should have

```text
@inherits LayoutComponentBase

<div class="sidebar">
    <NavMenu />
</div>

<div class="main">
    <div class="top-row px-4">
        <LoginControl />
    </div>

    <div class="content px-4">
        @Body
    </div>
</div>
```


# AzureAD.UI

> *Microsoft.AspNetCore.Authentication.AzureAD.UI*

*Coming Soon..*