// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "NU1510:PackageReference will not be pruned",
    Justification = "Microsoft.AspNetCore.RateLimiting is used in Program.cs for rate limiting configuration")]
