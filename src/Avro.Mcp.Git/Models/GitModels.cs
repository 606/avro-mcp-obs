using System.ComponentModel.DataAnnotations;

namespace Avro.Mcp.Git.Models;

/// <summary>
/// Repository information
/// </summary>
public class GitRepository
{
    public string Name { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public string RemoteUrl { get; set; } = string.Empty;
    public string CurrentBranch { get; set; } = string.Empty;
    public bool IsBare { get; set; }
    public bool IsClean { get; set; }
    public int CommitCount { get; set; }
    public DateTime LastCommit { get; set; }
    public List<string> Branches { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Git commit information
/// </summary>
public class GitCommit
{
    public string Sha { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<string> ParentShas { get; set; } = new();
    public List<GitFileChange> Changes { get; set; } = new();
}

/// <summary>
/// File change information
/// </summary>
public class GitFileChange
{
    public string Path { get; set; } = string.Empty;
    public GitChangeType ChangeType { get; set; }
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
}

/// <summary>
/// Type of change made to a file
/// </summary>
public enum GitChangeType
{
    Added,
    Modified,
    Deleted,
    Renamed,
    Copied
}

/// <summary>
/// Branch information
/// </summary>
public class GitBranch
{
    public string Name { get; set; } = string.Empty;
    public bool IsRemote { get; set; }
    public bool IsCurrent { get; set; }
    public string LastCommitSha { get; set; } = string.Empty;
    public DateTime LastCommitDate { get; set; }
    public string Upstream { get; set; } = string.Empty;
    public int AheadBy { get; set; }
    public int BehindBy { get; set; }
}

/// <summary>
/// Git status information
/// </summary>
public class GitStatus
{
    public bool IsClean { get; set; }
    public List<GitStatusEntry> Entries { get; set; } = new();
    public string CurrentBranch { get; set; } = string.Empty;
    public int UntrackedFiles { get; set; }
    public int ModifiedFiles { get; set; }
    public int StagedFiles { get; set; }
}

/// <summary>
/// Individual file status entry
/// </summary>
public class GitStatusEntry
{
    public string FilePath { get; set; } = string.Empty;
    public GitFileStatus Status { get; set; }
}

/// <summary>
/// Status of a file in the working directory
/// </summary>
public enum GitFileStatus
{
    Untracked,
    Modified,
    Added,
    Deleted,
    Renamed,
    Copied,
    Staged
}

// Request/Response Models

/// <summary>
/// Clone repository request
/// </summary>
public class CloneRepositoryRequest
{
    [Required]
    [Url]
    public string RemoteUrl { get; set; } = string.Empty;
    
    [Required]
    public string LocalPath { get; set; } = string.Empty;
    
    public string? Branch { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool IsBare { get; set; } = false;
}

/// <summary>
/// Initialize repository request
/// </summary>
public class InitRepositoryRequest
{
    [Required]
    public string LocalPath { get; set; } = string.Empty;
    
    public bool IsBare { get; set; } = false;
}

/// <summary>
/// Commit changes request
/// </summary>
public class CommitRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    public string AuthorName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string AuthorEmail { get; set; } = string.Empty;
    
    public List<string> FilesToAdd { get; set; } = new();
    public bool AddAll { get; set; } = false;
}

/// <summary>
/// Create branch request
/// </summary>
public class CreateBranchRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    [Required]
    public string BranchName { get; set; } = string.Empty;
    
    public string? FromCommit { get; set; }
    public bool Checkout { get; set; } = true;
}

/// <summary>
/// Merge branches request
/// </summary>
public class MergeBranchRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    [Required]
    public string SourceBranch { get; set; } = string.Empty;
    
    public string? TargetBranch { get; set; }
    
    [Required]
    public string CommitterName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string CommitterEmail { get; set; } = string.Empty;
    
    public string? MergeMessage { get; set; }
}

/// <summary>
/// Push changes request
/// </summary>
public class PushRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    public string? Remote { get; set; } = "origin";
    public string? Branch { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool Force { get; set; } = false;
}

/// <summary>
/// Pull changes request
/// </summary>
public class PullRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    public string? Remote { get; set; } = "origin";
    public string? Branch { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    [Required]
    public string MergerName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string MergerEmail { get; set; } = string.Empty;
}

/// <summary>
/// Get repository information request
/// </summary>
public class GetRepositoryRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
}

/// <summary>
/// Get commits request
/// </summary>
public class GetCommitsRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    public string? Branch { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
    public string? Since { get; set; }
    public string? Until { get; set; }
    public string? Author { get; set; }
}

/// <summary>
/// Get branches request
/// </summary>
public class GetBranchesRequest
{
    [Required]
    public string RepositoryPath { get; set; } = string.Empty;
    
    public bool IncludeRemote { get; set; } = false;
}

/// <summary>
/// Generic Git operation response
/// </summary>
public class GitOperationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Paged response for commits and other collections
/// </summary>
public class GitPagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public bool HasMore { get; set; }
}