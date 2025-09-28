using Avro.Mcp.Git.Models;
using Avro.Mcp.Git.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Avro.Mcp.Git.Endpoints;

/// <summary>
/// Git repository operations endpoints
/// </summary>
public static class GitRepositoryEndpoints
{
    /// <summary>
    /// Map Git repository endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapGitRepository(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/git/repository")
            .WithTags("Git Repository")
            .WithOpenApi();

        group.MapPost("/clone", CloneRepository)
            .WithSummary("Clone a Git repository")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        group.MapPost("/init", InitRepository)
            .WithSummary("Initialize a new Git repository")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        group.MapGet("/info", GetRepositoryInfo)
            .WithSummary("Get repository information")
            .Produces<GitRepository>(StatusCodes.Status200OK);

        group.MapGet("/status", GetStatus)
            .WithSummary("Get repository status")
            .Produces<GitStatus>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>
    /// Clone a Git repository from remote URL
    /// </summary>
    private static async Task<IResult> CloneRepository(
        [FromBody] CloneRepositoryRequest request,
        IGitService gitService,
        IValidator<CloneRepositoryRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.CloneRepositoryAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Initialize a new Git repository
    /// </summary>
    private static async Task<IResult> InitRepository(
        [FromBody] InitRepositoryRequest request,
        IGitService gitService,
        IValidator<InitRepositoryRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.InitRepositoryAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Get repository information
    /// </summary>
    private static async Task<IResult> GetRepositoryInfo(
        [AsParameters] GetRepositoryRequest request,
        IGitService gitService,
        IValidator<GetRepositoryRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var repository = await gitService.GetRepositoryInfoAsync(request.RepositoryPath);
        return repository != null ? Results.Ok(repository) : Results.NotFound("Repository not found");
    }

    /// <summary>
    /// Get repository status
    /// </summary>
    private static async Task<IResult> GetStatus(
        [AsParameters] GetRepositoryRequest request,
        IGitService gitService,
        IValidator<GetRepositoryRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var status = await gitService.GetStatusAsync(request.RepositoryPath);
        return Results.Ok(status);
    }
}

/// <summary>
/// Git commit operations endpoints
/// </summary>
public static class GitCommitEndpoints
{
    /// <summary>
    /// Map Git commit endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapGitCommits(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/git/commits")
            .WithTags("Git Commits")
            .WithOpenApi();

        group.MapPost("/", CreateCommit)
            .WithSummary("Create a new commit")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        group.MapGet("/", GetCommits)
            .WithSummary("Get repository commits")
            .Produces<GitPagedResponse<GitCommit>>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>
    /// Create a new commit with staged changes
    /// </summary>
    private static async Task<IResult> CreateCommit(
        [FromBody] CommitRequest request,
        IGitService gitService,
        IValidator<CommitRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.CommitChangesAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Get repository commits with pagination and filtering
    /// </summary>
    private static async Task<IResult> GetCommits(
        [AsParameters] GetCommitsRequest request,
        IGitService gitService,
        IValidator<GetCommitsRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var commits = await gitService.GetCommitsAsync(request);
        return Results.Ok(commits);
    }
}

/// <summary>
/// Git branch operations endpoints
/// </summary>
public static class GitBranchEndpoints
{
    /// <summary>
    /// Map Git branch endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapGitBranches(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/git/branches")
            .WithTags("Git Branches")
            .WithOpenApi();

        group.MapPost("/", CreateBranch)
            .WithSummary("Create a new branch")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        group.MapGet("/", GetBranches)
            .WithSummary("Get repository branches")
            .Produces<List<GitBranch>>(StatusCodes.Status200OK);

        group.MapPost("/checkout", CheckoutBranch)
            .WithSummary("Checkout a branch")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        group.MapDelete("/{branchName}", DeleteBranch)
            .WithSummary("Delete a branch")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        group.MapPost("/merge", MergeBranch)
            .WithSummary("Merge branches")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>
    /// Create a new Git branch
    /// </summary>
    private static async Task<IResult> CreateBranch(
        [FromBody] CreateBranchRequest request,
        IGitService gitService,
        IValidator<CreateBranchRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.CreateBranchAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Get all repository branches
    /// </summary>
    private static async Task<IResult> GetBranches(
        [AsParameters] GetBranchesRequest request,
        IGitService gitService,
        IValidator<GetBranchesRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var branches = await gitService.GetBranchesAsync(request);
        return Results.Ok(branches);
    }

    /// <summary>
    /// Checkout a Git branch
    /// </summary>
    private static async Task<IResult> CheckoutBranch(
        [FromBody] CheckoutBranchRequest request,
        IGitService gitService,
        IValidator<CheckoutBranchRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.CheckoutBranchAsync(request.RepositoryPath, request.BranchName);
        return Results.Ok(response);
    }

    /// <summary>
    /// Delete a Git branch
    /// </summary>
    private static async Task<IResult> DeleteBranch(
        string branchName,
        [AsParameters] DeleteBranchRequest request,
        IGitService gitService,
        IValidator<DeleteBranchRequest> validator)
    {
        request.BranchName = branchName;
        
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.DeleteBranchAsync(request.RepositoryPath, request.BranchName, request.Force);
        return Results.Ok(response);
    }

    /// <summary>
    /// Merge Git branches
    /// </summary>
    private static async Task<IResult> MergeBranch(
        [FromBody] MergeBranchRequest request,
        IGitService gitService,
        IValidator<MergeBranchRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.MergeBranchAsync(request);
        return Results.Ok(response);
    }
}

/// <summary>
/// Git remote operations endpoints
/// </summary>
public static class GitRemoteEndpoints
{
    /// <summary>
    /// Map Git remote endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapGitRemote(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/git/remote")
            .WithTags("Git Remote")
            .WithOpenApi();

        group.MapPost("/push", PushChanges)
            .WithSummary("Push changes to remote")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        group.MapPost("/pull", PullChanges)
            .WithSummary("Pull changes from remote")
            .Produces<GitOperationResponse>(StatusCodes.Status200OK);

        return app;
    }

    /// <summary>
    /// Push changes to remote repository
    /// </summary>
    private static async Task<IResult> PushChanges(
        [FromBody] PushRequest request,
        IGitService gitService,
        IValidator<PushRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.PushChangesAsync(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Pull changes from remote repository
    /// </summary>
    private static async Task<IResult> PullChanges(
        [FromBody] PullRequest request,
        IGitService gitService,
        IValidator<PullRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var response = await gitService.PullChangesAsync(request);
        return Results.Ok(response);
    }
}

// Additional request models for endpoints

/// <summary>
/// Checkout branch request
/// </summary>
public class CheckoutBranchRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    [Required]
    public string BranchName { get; set; } = string.Empty;
}

/// <summary>
/// Delete branch request
/// </summary>
public class DeleteBranchRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    [Required]
    public string BranchName { get; set; } = string.Empty;
    
    public bool Force { get; set; } = false;
}