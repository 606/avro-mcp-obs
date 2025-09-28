using Avro.Mcp.Git.Models;
using LibGit2Sharp;
using System.Text;

namespace Avro.Mcp.Git.Services;

/// <summary>
/// Interface for Git operations
/// </summary>
public interface IGitService
{
    Task<GitOperationResponse> CloneRepositoryAsync(CloneRepositoryRequest request);
    Task<GitOperationResponse> InitRepositoryAsync(InitRepositoryRequest request);
    Task<GitOperationResponse> CommitChangesAsync(CommitRequest request);
    Task<GitOperationResponse> CreateBranchAsync(CreateBranchRequest request);
    Task<GitOperationResponse> MergeBranchAsync(MergeBranchRequest request);
    Task<GitOperationResponse> PushChangesAsync(PushRequest request);
    Task<GitOperationResponse> PullChangesAsync(PullRequest request);
    Task<GitRepository?> GetRepositoryInfoAsync(string repositoryPath);
    Task<GitPagedResponse<GitCommit>> GetCommitsAsync(GetCommitsRequest request);
    Task<List<GitBranch>> GetBranchesAsync(GetBranchesRequest request);
    Task<GitStatus> GetStatusAsync(string repositoryPath);
    Task<GitOperationResponse> CheckoutBranchAsync(string repositoryPath, string branchName);
    Task<GitOperationResponse> DeleteBranchAsync(string repositoryPath, string branchName, bool force = false);
}

/// <summary>
/// Git operations service using LibGit2Sharp
/// </summary>
public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    public async Task<GitOperationResponse> CloneRepositoryAsync(CloneRepositoryRequest request)
    {
        try
        {
            await Task.Run(() =>
            {
                // For now, clone without authentication (can be extended later)
                Repository.Clone(request.RemoteUrl, request.LocalPath);
            });

            _logger.LogInformation("Successfully cloned repository {RemoteUrl} to {LocalPath}", 
                request.RemoteUrl, request.LocalPath);

            return new GitOperationResponse
            {
                Success = true,
                Message = $"Repository cloned successfully to {request.LocalPath}",
                Data = new { RemoteUrl = request.RemoteUrl, LocalPath = request.LocalPath }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning repository {RemoteUrl}", request.RemoteUrl);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to clone repository",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitOperationResponse> InitRepositoryAsync(InitRepositoryRequest request)
    {
        try
        {
            await Task.Run(() =>
            {
                Repository.Init(request.LocalPath, request.IsBare);
            });

            _logger.LogInformation("Initialized repository at {LocalPath}", request.LocalPath);

            return new GitOperationResponse
            {
                Success = true,
                Message = $"Repository initialized at {request.LocalPath}",
                Data = new { LocalPath = request.LocalPath, IsBare = request.IsBare }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing repository at {LocalPath}", request.LocalPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to initialize repository",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitOperationResponse> CommitChangesAsync(CommitRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(request.RepositoryPath);
                
                // Stage files
                if (request.AddAll)
                {
                    Commands.Stage(repo, "*");
                }
                else if (request.FilesToAdd.Any())
                {
                    foreach (var file in request.FilesToAdd)
                    {
                        Commands.Stage(repo, file);
                    }
                }

                var signature = new Signature(request.AuthorName, request.AuthorEmail, DateTimeOffset.Now);
                var commit = repo.Commit(request.Message, signature, signature);

                _logger.LogInformation("Created commit {CommitSha} in {RepositoryPath}", 
                    commit.Sha, request.RepositoryPath);

                return new GitOperationResponse
                {
                    Success = true,
                    Message = $"Commit created successfully: {commit.Sha[..7]}",
                    Data = new 
                    { 
                        CommitSha = commit.Sha,
                        Message = request.Message,
                        Author = request.AuthorName
                    }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing changes in {RepositoryPath}", request.RepositoryPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to commit changes",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitOperationResponse> CreateBranchAsync(CreateBranchRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(request.RepositoryPath);
                
                Commit? fromCommit = null;
                if (!string.IsNullOrEmpty(request.FromCommit))
                {
                    fromCommit = repo.Lookup<Commit>(request.FromCommit);
                }
                
                var branch = repo.CreateBranch(request.BranchName, fromCommit ?? repo.Head.Tip);

                if (request.Checkout)
                {
                    Commands.Checkout(repo, branch);
                }

                _logger.LogInformation("Created branch {BranchName} in {RepositoryPath}", 
                    request.BranchName, request.RepositoryPath);

                return new GitOperationResponse
                {
                    Success = true,
                    Message = $"Branch '{request.BranchName}' created successfully",
                    Data = new 
                    { 
                        BranchName = request.BranchName,
                        CheckedOut = request.Checkout,
                        FromCommit = fromCommit?.Sha ?? repo.Head.Tip.Sha
                    }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch {BranchName} in {RepositoryPath}", 
                request.BranchName, request.RepositoryPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to create branch",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitOperationResponse> MergeBranchAsync(MergeBranchRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(request.RepositoryPath);
                
                var sourceBranch = repo.Branches[request.SourceBranch];
                if (sourceBranch == null)
                {
                    throw new ArgumentException($"Source branch '{request.SourceBranch}' not found");
                }

                if (!string.IsNullOrEmpty(request.TargetBranch))
                {
                    var targetBranch = repo.Branches[request.TargetBranch];
                    if (targetBranch == null)
                    {
                        throw new ArgumentException($"Target branch '{request.TargetBranch}' not found");
                    }
                    Commands.Checkout(repo, targetBranch);
                }

                var signature = new Signature(request.CommitterName, request.CommitterEmail, DateTimeOffset.Now);
                var result = repo.Merge(sourceBranch, signature);

                _logger.LogInformation("Merged branch {SourceBranch} in {RepositoryPath}", 
                    request.SourceBranch, request.RepositoryPath);

                return new GitOperationResponse
                {
                    Success = true,
                    Message = $"Successfully merged '{request.SourceBranch}' - Status: {result.Status}",
                    Data = new 
                    { 
                        SourceBranch = request.SourceBranch,
                        TargetBranch = request.TargetBranch ?? repo.Head.FriendlyName,
                        MergeStatus = result.Status.ToString(),
                        Commit = result.Commit?.Sha
                    }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging branch {SourceBranch} in {RepositoryPath}", 
                request.SourceBranch, request.RepositoryPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to merge branch",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitOperationResponse> PushChangesAsync(PushRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                // Simplified push without authentication for now
                return new GitOperationResponse
                {
                    Success = false,
                    Message = "Push operations require authentication setup",
                    Errors = { "Authentication not yet implemented in this version" }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing changes in {RepositoryPath}", request.RepositoryPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to push changes",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitOperationResponse> PullChangesAsync(PullRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                // Simplified pull without authentication for now  
                return new GitOperationResponse
                {
                    Success = false,
                    Message = "Pull operations require authentication setup",
                    Errors = { "Authentication not yet implemented in this version" }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling changes in {RepositoryPath}", request.RepositoryPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to pull changes",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitRepository?> GetRepositoryInfoAsync(string repositoryPath)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(repositoryPath);
                
                var repoInfo = new GitRepository
                {
                    Name = Path.GetFileName(repositoryPath),
                    LocalPath = repositoryPath,
                    RemoteUrl = repo.Network.Remotes.FirstOrDefault()?.Url ?? "",
                    CurrentBranch = repo.Head.FriendlyName,
                    IsBare = repo.Info.IsBare,
                    IsClean = repo.RetrieveStatus().IsDirty == false,
                    CommitCount = repo.Commits.Count(),
                    LastCommit = repo.Head.Tip?.Author.When.DateTime ?? DateTime.MinValue,
                    Branches = repo.Branches.Select(b => b.FriendlyName).ToList(),
                    Tags = repo.Tags.Select(t => t.FriendlyName).ToList()
                };

                return repoInfo;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository info for {RepositoryPath}", repositoryPath);
            return null;
        }
    }

    public async Task<GitPagedResponse<GitCommit>> GetCommitsAsync(GetCommitsRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(request.RepositoryPath);
                
                var branch = string.IsNullOrEmpty(request.Branch) 
                    ? repo.Head 
                    : repo.Branches[request.Branch];

                if (branch == null)
                {
                    throw new ArgumentException($"Branch '{request.Branch}' not found");
                }

                var commits = branch.Commits.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(request.Author))
                {
                    commits = commits.Where(c => c.Author.Name.Contains(request.Author, StringComparison.OrdinalIgnoreCase));
                }

                if (DateTime.TryParse(request.Since, out var sinceDate))
                {
                    commits = commits.Where(c => c.Author.When >= sinceDate);
                }

                if (DateTime.TryParse(request.Until, out var untilDate))
                {
                    commits = commits.Where(c => c.Author.When <= untilDate);
                }

                var totalCount = commits.Count();
                var pagedCommits = commits
                    .Skip(request.Skip)
                    .Take(request.Take)
                    .Select(c => MapCommit(c, repo))
                    .ToList();

                return new GitPagedResponse<GitCommit>
                {
                    Items = pagedCommits,
                    TotalCount = totalCount,
                    Skip = request.Skip,
                    Take = request.Take,
                    HasMore = (request.Skip + request.Take) < totalCount
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting commits for {RepositoryPath}", request.RepositoryPath);
            return new GitPagedResponse<GitCommit>();
        }
    }

    public async Task<List<GitBranch>> GetBranchesAsync(GetBranchesRequest request)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(request.RepositoryPath);
                
                var branches = repo.Branches
                    .Where(b => request.IncludeRemote || !b.IsRemote)
                    .Select(MapBranch)
                    .ToList();

                return branches;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branches for {RepositoryPath}", request.RepositoryPath);
            return new List<GitBranch>();
        }
    }

    public async Task<GitStatus> GetStatusAsync(string repositoryPath)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus();

                return new GitStatus
                {
                    IsClean = !status.IsDirty,
                    CurrentBranch = repo.Head.FriendlyName,
                    UntrackedFiles = status.Untracked.Count(),
                    ModifiedFiles = status.Modified.Count(),
                    StagedFiles = status.Staged.Count(),
                    Entries = status.Select(entry => new GitStatusEntry
                    {
                        FilePath = entry.FilePath,
                        Status = MapFileStatus(entry.State)
                    }).ToList()
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for {RepositoryPath}", repositoryPath);
            return new GitStatus { IsClean = false };
        }
    }

    public async Task<GitOperationResponse> CheckoutBranchAsync(string repositoryPath, string branchName)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(repositoryPath);
                var branch = repo.Branches[branchName];
                
                if (branch == null)
                {
                    throw new ArgumentException($"Branch '{branchName}' not found");
                }

                Commands.Checkout(repo, branch);

                return new GitOperationResponse
                {
                    Success = true,
                    Message = $"Checked out branch '{branchName}'",
                    Data = new { BranchName = branchName }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out branch {BranchName} in {RepositoryPath}", 
                branchName, repositoryPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to checkout branch",
                Errors = { ex.Message }
            };
        }
    }

    public async Task<GitOperationResponse> DeleteBranchAsync(string repositoryPath, string branchName, bool force = false)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var repo = new Repository(repositoryPath);
                var branch = repo.Branches[branchName];
                
                if (branch == null)
                {
                    throw new ArgumentException($"Branch '{branchName}' not found");
                }

                repo.Branches.Remove(branch);

                return new GitOperationResponse
                {
                    Success = true,
                    Message = $"Deleted branch '{branchName}'",
                    Data = new { BranchName = branchName, Force = force }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch {BranchName} in {RepositoryPath}", 
                branchName, repositoryPath);
            return new GitOperationResponse
            {
                Success = false,
                Message = "Failed to delete branch",
                Errors = { ex.Message }
            };
        }
    }

    private static GitCommit MapCommit(Commit commit, Repository repo)
    {
        return new GitCommit
        {
            Sha = commit.Sha,
            Message = commit.Message,
            Author = commit.Author.Name,
            AuthorEmail = commit.Author.Email,
            Date = commit.Author.When.DateTime,
            ParentShas = commit.Parents.Select(p => p.Sha).ToList(),
            Changes = new List<GitFileChange>() // Simplified for now
        };
    }

    private static GitBranch MapBranch(Branch branch)
    {
        return new GitBranch
        {
            Name = branch.FriendlyName,
            IsRemote = branch.IsRemote,
            IsCurrent = branch.IsCurrentRepositoryHead,
            LastCommitSha = branch.Tip?.Sha ?? "",
            LastCommitDate = branch.Tip?.Author.When.DateTime ?? DateTime.MinValue,
            Upstream = branch.TrackedBranch?.FriendlyName ?? "",
            AheadBy = branch.TrackingDetails?.AheadBy ?? 0,
            BehindBy = branch.TrackingDetails?.BehindBy ?? 0
        };
    }

    private static GitChangeType MapChangeType(ChangeKind changeKind)
    {
        return changeKind switch
        {
            ChangeKind.Added => GitChangeType.Added,
            ChangeKind.Modified => GitChangeType.Modified,
            ChangeKind.Deleted => GitChangeType.Deleted,
            ChangeKind.Renamed => GitChangeType.Renamed,
            ChangeKind.Copied => GitChangeType.Copied,
            _ => GitChangeType.Modified
        };
    }

    private static GitFileStatus MapFileStatus(FileStatus fileStatus)
    {
        return fileStatus switch
        {
            FileStatus.NewInWorkdir => GitFileStatus.Untracked,
            FileStatus.ModifiedInWorkdir => GitFileStatus.Modified,
            FileStatus.DeletedFromWorkdir => GitFileStatus.Deleted,
            FileStatus.RenamedInWorkdir => GitFileStatus.Renamed,
            FileStatus.NewInIndex => GitFileStatus.Staged,
            FileStatus.ModifiedInIndex => GitFileStatus.Staged,
            FileStatus.DeletedFromIndex => GitFileStatus.Staged,
            FileStatus.RenamedInIndex => GitFileStatus.Staged,
            _ => GitFileStatus.Modified
        };
    }
}