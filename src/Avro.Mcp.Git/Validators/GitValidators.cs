using Avro.Mcp.Git.Endpoints;
using Avro.Mcp.Git.Models;
using FluentValidation;

namespace Avro.Mcp.Git.Validators;

/// <summary>
/// Validator for clone repository requests
/// </summary>
public class CloneRepositoryRequestValidator : AbstractValidator<CloneRepositoryRequest>
{
    public CloneRepositoryRequestValidator()
    {
        RuleFor(x => x.RemoteUrl)
            .NotEmpty()
            .WithMessage("Remote URL is required")
            .Must(BeValidUrl)
            .WithMessage("Remote URL must be a valid URL");

        RuleFor(x => x.LocalPath)
            .NotEmpty()
            .WithMessage("Local path is required")
            .Must(BeValidPath)
            .WithMessage("Local path contains invalid characters");

        RuleFor(x => x.Branch)
            .MaximumLength(250)
            .WithMessage("Branch name cannot exceed 250 characters")
            .Must(BeValidBranchName)
            .When(x => !string.IsNullOrEmpty(x.Branch))
            .WithMessage("Branch name contains invalid characters");

        RuleFor(x => x.Username)
            .MaximumLength(100)
            .WithMessage("Username cannot exceed 100 characters");

        RuleFor(x => x.Password)
            .MaximumLength(500)
            .WithMessage("Password cannot exceed 500 characters");
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || 
                uri.Scheme == "git" || uri.Scheme == "ssh");
    }

    private static bool BeValidPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var invalidChars = Path.GetInvalidPathChars();
        return !path.Any(c => invalidChars.Contains(c));
    }

    private static bool BeValidBranchName(string branchName)
    {
        if (string.IsNullOrEmpty(branchName))
            return true;

        // Basic Git branch name validation
        return !branchName.Contains("..") && 
               !branchName.StartsWith('/') && 
               !branchName.EndsWith('/') &&
               !branchName.Contains(' ');
    }
}

/// <summary>
/// Validator for init repository requests
/// </summary>
public class InitRepositoryRequestValidator : AbstractValidator<InitRepositoryRequest>
{
    public InitRepositoryRequestValidator()
    {
        RuleFor(x => x.LocalPath)
            .NotEmpty()
            .WithMessage("Local path is required")
            .Must(BeValidPath)
            .WithMessage("Local path contains invalid characters");
    }

    private static bool BeValidPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var invalidChars = Path.GetInvalidPathChars();
        return !path.Any(c => invalidChars.Contains(c));
    }
}

/// <summary>
/// Validator for commit requests
/// </summary>
public class CommitRequestValidator : AbstractValidator<CommitRequest>
{
    public CommitRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Commit message is required")
            .MaximumLength(1000)
            .WithMessage("Commit message cannot exceed 1000 characters");

        RuleFor(x => x.AuthorName)
            .NotEmpty()
            .WithMessage("Author name is required")
            .MaximumLength(100)
            .WithMessage("Author name cannot exceed 100 characters");

        RuleFor(x => x.AuthorEmail)
            .NotEmpty()
            .WithMessage("Author email is required")
            .EmailAddress()
            .WithMessage("Author email must be a valid email address");

        RuleFor(x => x.FilesToAdd)
            .NotNull()
            .WithMessage("Files to add list cannot be null");

        RuleForEach(x => x.FilesToAdd)
            .NotEmpty()
            .WithMessage("File path cannot be empty");
    }
}

/// <summary>
/// Validator for create branch requests
/// </summary>
public class CreateBranchRequestValidator : AbstractValidator<CreateBranchRequest>
{
    public CreateBranchRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(250)
            .WithMessage("Branch name cannot exceed 250 characters")
            .Must(BeValidBranchName)
            .WithMessage("Branch name contains invalid characters");

        RuleFor(x => x.FromCommit)
            .MaximumLength(40)
            .WithMessage("Commit SHA cannot exceed 40 characters")
            .Must(BeValidCommitSha)
            .When(x => !string.IsNullOrEmpty(x.FromCommit))
            .WithMessage("Commit SHA must be a valid hexadecimal string");
    }

    private static bool BeValidBranchName(string branchName)
    {
        if (string.IsNullOrEmpty(branchName))
            return false;

        return !branchName.Contains("..") && 
               !branchName.StartsWith('/') && 
               !branchName.EndsWith('/') &&
               !branchName.Contains(' ') &&
               !branchName.Contains('~') &&
               !branchName.Contains('^');
    }

    private static bool BeValidCommitSha(string sha)
    {
        if (string.IsNullOrEmpty(sha))
            return true;

        return sha.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
    }
}

/// <summary>
/// Validator for merge branch requests
/// </summary>
public class MergeBranchRequestValidator : AbstractValidator<MergeBranchRequest>
{
    public MergeBranchRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.SourceBranch)
            .NotEmpty()
            .WithMessage("Source branch is required")
            .MaximumLength(250)
            .WithMessage("Source branch name cannot exceed 250 characters");

        RuleFor(x => x.TargetBranch)
            .MaximumLength(250)
            .WithMessage("Target branch name cannot exceed 250 characters")
            .When(x => !string.IsNullOrEmpty(x.TargetBranch));

        RuleFor(x => x.CommitterName)
            .NotEmpty()
            .WithMessage("Committer name is required")
            .MaximumLength(100)
            .WithMessage("Committer name cannot exceed 100 characters");

        RuleFor(x => x.CommitterEmail)
            .NotEmpty()
            .WithMessage("Committer email is required")
            .EmailAddress()
            .WithMessage("Committer email must be a valid email address");

        RuleFor(x => x.MergeMessage)
            .MaximumLength(1000)
            .WithMessage("Merge message cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.MergeMessage));
    }
}

/// <summary>
/// Validator for push requests
/// </summary>
public class PushRequestValidator : AbstractValidator<PushRequest>
{
    public PushRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.Remote)
            .MaximumLength(100)
            .WithMessage("Remote name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Remote));

        RuleFor(x => x.Branch)
            .MaximumLength(250)
            .WithMessage("Branch name cannot exceed 250 characters")
            .When(x => !string.IsNullOrEmpty(x.Branch));

        RuleFor(x => x.Username)
            .MaximumLength(100)
            .WithMessage("Username cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Password)
            .MaximumLength(500)
            .WithMessage("Password cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}

/// <summary>
/// Validator for pull requests
/// </summary>
public class PullRequestValidator : AbstractValidator<PullRequest>
{
    public PullRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.Remote)
            .MaximumLength(100)
            .WithMessage("Remote name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Remote));

        RuleFor(x => x.Branch)
            .MaximumLength(250)
            .WithMessage("Branch name cannot exceed 250 characters")
            .When(x => !string.IsNullOrEmpty(x.Branch));

        RuleFor(x => x.Username)
            .MaximumLength(100)
            .WithMessage("Username cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Password)
            .MaximumLength(500)
            .WithMessage("Password cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.MergerName)
            .NotEmpty()
            .WithMessage("Merger name is required")
            .MaximumLength(100)
            .WithMessage("Merger name cannot exceed 100 characters");

        RuleFor(x => x.MergerEmail)
            .NotEmpty()
            .WithMessage("Merger email is required")
            .EmailAddress()
            .WithMessage("Merger email must be a valid email address");
    }
}

/// <summary>
/// Validator for get repository requests
/// </summary>
public class GetRepositoryRequestValidator : AbstractValidator<GetRepositoryRequest>
{
    public GetRepositoryRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");
    }
}

/// <summary>
/// Validator for get commits requests
/// </summary>
public class GetCommitsRequestValidator : AbstractValidator<GetCommitsRequest>
{
    public GetCommitsRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0");

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .WithMessage("Take must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Take cannot exceed 1000");

        RuleFor(x => x.Branch)
            .MaximumLength(250)
            .WithMessage("Branch name cannot exceed 250 characters")
            .When(x => !string.IsNullOrEmpty(x.Branch));

        RuleFor(x => x.Since)
            .Must(BeValidDate)
            .When(x => !string.IsNullOrEmpty(x.Since))
            .WithMessage("Since must be a valid date");

        RuleFor(x => x.Until)
            .Must(BeValidDate)
            .When(x => !string.IsNullOrEmpty(x.Until))
            .WithMessage("Until must be a valid date");

        RuleFor(x => x.Author)
            .MaximumLength(100)
            .WithMessage("Author name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Author));
    }

    private static bool BeValidDate(string dateString)
    {
        return DateTime.TryParse(dateString, out _);
    }
}

/// <summary>
/// Validator for get branches requests
/// </summary>
public class GetBranchesRequestValidator : AbstractValidator<GetBranchesRequest>
{
    public GetBranchesRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");
    }
}

/// <summary>
/// Validator for checkout branch requests
/// </summary>
public class CheckoutBranchRequestValidator : AbstractValidator<CheckoutBranchRequest>
{
    public CheckoutBranchRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(250)
            .WithMessage("Branch name cannot exceed 250 characters");
    }
}

/// <summary>
/// Validator for delete branch requests
/// </summary>
public class DeleteBranchRequestValidator : AbstractValidator<DeleteBranchRequest>
{
    public DeleteBranchRequestValidator()
    {
        RuleFor(x => x.RepositoryPath)
            .NotEmpty()
            .WithMessage("Repository path is required")
            .Must(Directory.Exists)
            .WithMessage("Repository path must exist");

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(250)
            .WithMessage("Branch name cannot exceed 250 characters");
    }
}