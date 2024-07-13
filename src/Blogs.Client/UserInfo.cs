// ============================================
// Copyright (c) 2024. All rights reserved.
// File Name :     UserInfo.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : MudBlazorBlogs
// Project Name :  Blogs.Client
// =============================================

namespace Blogs.Client;

// Add properties to this class and update the server and client AuthenticationStateProviders
// to expose more information about the authenticated user to the client.
public class UserInfo
{
	public required string UserId { get; set; }
	public required string Email { get; set; }
}
