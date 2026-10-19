using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UniversityAdministrationSystem;
using UniversityAdministrationSystem.IntegrationTests.Fixtures;
using Xunit;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace UniversityAdministrationSystem.IntegrationTests.Api;

[Collection("Integration")]
public class AuthIntegrationTests : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    private HttpClient _client = null!;

    public AuthIntegrationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;


    private async Task<string> GetAntiforgeryTokenAsync()
    {
        using var response = await _client.GetAsync("/login");

        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(html, @"<input\b(?=[^>]*\bname=""__RequestVerificationToken"")(?=[^>]*\bvalue=""([^""]+)"")[^>]*>",
            RegexOptions.IgnoreCase);

        match.Success.Should().BeTrue(
            "the login page should render an antiforgery token");

        return match.Groups[1].Value;
    }

    [Fact]
    public async Task Login_ValidCredentials_RedirectsAndIssuesAuthCookie()
    {
        var token = await GetAntiforgeryTokenAsync();

        var formData = new Dictionary<string, string>
        {
            ["Email"] = "admin@school.com",
            ["Password"] = "Admin123!",
            ["RememberMe"] = "false",
            ["__RequestVerificationToken"] = token
        };

        using var response = await _client.PostAsync("/account/login",
            new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.OriginalString.Should().Be("/");

        response.Headers.TryGetValues("Set-Cookie", out var cookies)
            .Should().BeTrue();

        cookies!.Should().Contain(cookie =>
            cookie.StartsWith(
                ".AspNetCore.Identity.Application=",
                StringComparison.Ordinal));
    }



    [Fact]
    public async Task Login_InvalidPassword_RedirectsWithoutAuthCookie()
    {
        var token = await GetAntiforgeryTokenAsync();
        var formData = new Dictionary<string, string>
        {
            ["Email"] = "admin@school.com",
            ["Password"] = "WrongPassword123!",
            ["RememberMe"] = "false",
            ["__RequestVerificationToken"] = token
        };

        using var response = await _client.PostAsync(
            "/account/login",
            new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.OriginalString
            .Should().Be("/login?error=true");

        var cookies = response.Headers.TryGetValues(
            "Set-Cookie", out var values)
            ? values
            : Enumerable.Empty<string>();

        cookies.Should().NotContain(cookie =>
            cookie.StartsWith(
                ".AspNetCore.Identity.Application=",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task Login_NonExistentUser_RedirectsWithoutAuthCookie()
    {
        var token = await GetAntiforgeryTokenAsync();

        var formData = new Dictionary<string, string>
        {
            ["Email"] = "nobody@example.com",
            ["Password"] = "WrongPassword123!",
            ["RememberMe"] = "false",
            ["__RequestVerificationToken"] = token
        };

        using var response = await _client.PostAsync(
            "/account/login",
            new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        response.Headers.Location?.OriginalString
            .Should().Be("/login?error=true");

        var cookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : Enumerable.Empty<string>();

        cookies.Should().NotContain(cookie =>
            cookie.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    }
    [Fact]
    public async Task Login_EmptyEmail_RedirectsWithoutAuthCookie()
    {
        var token = await GetAntiforgeryTokenAsync();

        var formData = new Dictionary<string, string>
        {
            ["Email"] = "",
            ["Password"] = "WrongPassword123!",
            ["RememberMe"] = "false",
            ["__RequestVerificationToken"] = token
        };

        using var response = await _client.PostAsync(
            "/account/login",
            new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        response.Headers.Location?.OriginalString
            .Should().Be("/login?error=true");

        var cookies = response.Headers.TryGetValues(
            "Set-Cookie", out var values)
            ? values
            : Enumerable.Empty<string>();

        cookies.Should().NotContain(cookie =>
            cookie.StartsWith(
                ".AspNetCore.Identity.Application=",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logout_Authenticated_InvalidatesSession()
    {
        var token = await GetAntiforgeryTokenAsync();

        var formData = new Dictionary<string, string>
        {
            ["Email"] = "admin@school.com",
            ["Password"] = "Admin123!",
            ["RememberMe"] = "false",
            ["__RequestVerificationToken"] = token
        };

        using var loginResponse = await _client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        loginResponse.Headers.Location?.OriginalString.Should().Be("/");

        // Confirm the session can access a protected endpoint.
        using var beforeLogout = await _client.GetAsync("/api/students");

        beforeLogout.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);

        // Obtain a fresh antiforgery token before signing out.
        var logoutToken = await GetAntiforgeryTokenAsync();

        // Sign out using the fresh token.
        using var logoutResponse = await _client.PostAsync(
            "/account/logout",
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = logoutToken
                }));

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        logoutResponse.Headers.Location?.OriginalString.Should().Be("/login");

        // Confirm the session no longer grants access.
        using var afterLogout = await _client.GetAsync("/api/students");

        afterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutAuth_ReturnsUnauthorized()
    {
        using var response = await _client.GetAsync("/api/students");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Login_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        var formData = new Dictionary<string, string>
        {
            ["Email"] = "admin@school.com",
            ["Password"] = "Admin123!",
            ["RememberMe"] = "false"
        };

        using var response = await _client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidAntiforgeryToken_ReturnsBadRequest()
    {
        await GetAntiforgeryTokenAsync();

        var formData = new Dictionary<string, string>
        {
            ["Email"] = "admin@school.com",
            ["Password"] = "Admin123!",
            ["RememberMe"] = "false",
            ["__RequestVerificationToken"] = "invalid-token"
        };

        using var response = await _client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        using var response = await _client.PostAsync("/account/logout", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithInvalidAntiforgeryToken_ReturnsBadRequest()
    {
        await GetAntiforgeryTokenAsync();

        using var response = await _client.PostAsync("/account/logout", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = "invalid-token"
                }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


}