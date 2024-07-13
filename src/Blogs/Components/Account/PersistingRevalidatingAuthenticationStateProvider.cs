// ============================================
// Copyright (c) 2024. All rights reserved.
// File Name :     PersistingRevalidatingAuthenticationStateProvider.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : MudBlazorBlogs
// Project Name :  Blogs
// =============================================

using System.Diagnostics;
using System.Security.Claims;
using Blogs.Client;
using Blogs.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Blogs.Components.Account;

// This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user
// every 30 minutes an interactive circuit is connected. It also uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
internal sealed class PersistingRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
	private readonly IdentityOptions _Options;
	private readonly IServiceScopeFactory _ScopeFactory;
	private readonly PersistentComponentState _State;

	private readonly PersistingComponentStateSubscription _Subscription;

	private Task<AuthenticationState>? _AuthenticationStateTask;

	public PersistingRevalidatingAuthenticationStateProvider(
		ILoggerFactory loggerFactory,
		IServiceScopeFactory serviceScopeFactory,
		PersistentComponentState persistentComponentState,
		IOptions<IdentityOptions> optionsAccessor)
		: base(loggerFactory)
	{
		_ScopeFactory = serviceScopeFactory;
		_State = persistentComponentState;
		_Options = optionsAccessor.Value;

		AuthenticationStateChanged += OnAuthenticationStateChanged;
		_Subscription = _State.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
	}

	protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

	protected override async Task<bool> ValidateAuthenticationStateAsync(
		AuthenticationState authenticationState, CancellationToken cancellationToken)
	{
		// Get the user manager from a new scope to ensure it fetches fresh data
		await using var scope = _ScopeFactory.CreateAsyncScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		return await ValidateSecurityStampAsync(userManager, authenticationState.User);
	}

	private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager,
		ClaimsPrincipal principal)
	{
		var user = await userManager.GetUserAsync(principal);
		if (user is null)
		{
			return false;
		}

		if (!userManager.SupportsUserSecurityStamp)
		{
			return true;
		}

		var principalStamp = principal.FindFirstValue(_Options.ClaimsIdentity.SecurityStampClaimType);
		var userStamp = await userManager.GetSecurityStampAsync(user);
		return principalStamp == userStamp;
	}

	private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
	{
		_AuthenticationStateTask = task;
	}

	private async Task OnPersistingAsync()
	{
		if (_AuthenticationStateTask is null)
		{
			throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
		}

		var authenticationState = await _AuthenticationStateTask;
		var principal = authenticationState.User;

		if (principal.Identity?.IsAuthenticated == true)
		{
			var userId = principal.FindFirst(_Options.ClaimsIdentity.UserIdClaimType)?.Value;
			var email = principal.FindFirst(_Options.ClaimsIdentity.EmailClaimType)?.Value;

			if (userId != null && email != null)
			{
				_State.PersistAsJson(nameof(UserInfo), new UserInfo
				{
					UserId = userId,
					Email = email
				});
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		_Subscription.Dispose();
		AuthenticationStateChanged -= OnAuthenticationStateChanged;
		base.Dispose(disposing);
	}
}
