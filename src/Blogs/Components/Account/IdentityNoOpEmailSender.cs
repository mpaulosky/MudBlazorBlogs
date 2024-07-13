// ============================================
// Copyright (c) 2024. All rights reserved.
// File Name :     IdentityNoOpEmailSender.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : MudBlazorBlogs
// Project Name :  Blogs
// =============================================

using Blogs.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Blogs.Components.Account;

// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
	private readonly IEmailSender _EmailSender = new NoOpEmailSender();

	public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
	{
		return _EmailSender.SendEmailAsync(email, "Confirm your email",
			$"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
	}

	public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
	{
		return _EmailSender.SendEmailAsync(email, "Reset your password",
			$"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
	}

	public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
	{
		return _EmailSender.SendEmailAsync(email, "Reset your password",
			$"Please reset your password using the following code: {resetCode}");
	}
}
