namespace CloudDocs.Domain.Enums;

public enum DocumentAccessLevel
{
    InternalPublic = 0,
    Private = 1,
    AdminOnly = 2,
    OwnerOnly = 3,
    DepartmentOnly = 4
}