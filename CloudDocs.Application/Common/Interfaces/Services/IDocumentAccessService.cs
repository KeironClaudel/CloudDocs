using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Common.Interfaces.Services;

public interface IDocumentAccessService
{
    bool CanAccessDocument(User currentUser, Document document);
}