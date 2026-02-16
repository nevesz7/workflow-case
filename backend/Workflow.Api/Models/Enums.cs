namespace Workflow.Api.Models;

public enum UserRole
{
    User = 1,
    Manager = 2
}

public enum RequestPriority
{
    Low = 1,
    Medium = 2,
    High = 3
}

public enum RequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}