// ============================================
// Copyright (c) 2024. All rights reserved.
// File Name :     IdentityUserAccessor.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : MudBlazorBlogs
// Project Name :  Blogs
// =============================================

using Blogs.Data;
using Microsoft.AspNetCore.Identity;

namespace Blogs.Components.Account;

internal sealed class IdentityUserAccessor(
	UserManager<ApplicationUser> userManager,
	IdentityRedirectManager redirectManager)
{
	public async Task<ApplicationUser> GetRequiredUserAsync(HttpContext context)
	{
		var user = await userManager.GetUserAsync(context.User);

		if (user is null)
		{
			redirectManager.RedirectToWithStatus("Account/InvalidUser",
				$"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
		}

		return user;
	}
}
