using CloudDocs.Application.Common.Exceptions;
using CloudDocs.Application.Common.Interfaces.Persistence;
using CloudDocs.Application.Common.Interfaces.Services;
using CloudDocs.Domain.Entities;

namespace CloudDocs.Application.Features.Documents.SendDocumentToClient;

public class SendDocumentToClientService : ISendDocumentToClientService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;
    private readonly IUserRepository _userRepository;
    private readonly IDemoPolicyService _demoPolicyService;
    private readonly ISentEmailLogRepository _sentEmailLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendDocumentToClientService(
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        IFileStorageService fileStorageService,
        IEmailService emailService,
        IAuditService auditService,
        IDemoPolicyService demoPolicyService,
        ISentEmailLogRepository sentEmailLogRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
        _emailService = emailService;
        _auditService = auditService;
        _demoPolicyService = demoPolicyService;
        _sentEmailLogRepository = sentEmailLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        Guid documentId,
        Guid currentUserId,
        SendDocumentToClientRequest request,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document is null)
            throw new NotFoundException("Document not found.");

        if (document.Client is null || !document.Client.IsActive)
            throw new BadRequestException("The document does not have an active client.");

        if (string.IsNullOrWhiteSpace(document.Client.Email))
            throw new BadRequestException("Client does not have an email configured.");

        var fileStream = await _fileStorageService.GetFileAsync(document.StoragePath, cancellationToken);

        if (fileStream is null)
            throw new NotFoundException("Document file not found in storage.");

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? $"Document: {document.OriginalFileName}{document.FileExtension}"
            : request.Subject.Trim();

        var body = string.IsNullOrWhiteSpace(request.Message)
            ? $"Dear client,\n\nPlease find attached the document '{document.OriginalFileName}{document.FileExtension}'."
            : request.Message.Trim();


        var currentUser = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);

        if (currentUser is null || !currentUser.IsActive)
            throw new NotFoundException("Current user not found or inactive.");

        var currentEmailCount = await _sentEmailLogRepository.CountByUserAsync(
            currentUser.Id,
            cancellationToken);

        await _demoPolicyService.ValidateSendEmailAsync(
            currentUser,
            currentEmailCount,
            cancellationToken);

        await _emailService.SendAsync(
            document.Client.Email,
            subject,
            body,
            $"{document.OriginalFileName}{document.FileExtension}",
            fileStream,
            document.ContentType,
            cancellationToken);

        await _auditService.LogAsync(
            currentUserId,
            "SendToClient",
            "Documents",
            "Document",
            document.Id.ToString(),
            $"Document sent to client: {document.Client.Email}",
            null,
            cancellationToken);

        var sentEmailLog = new SentEmailLog
        {
            Id = Guid.NewGuid(),
            SentByUserId = currentUser.Id,
            DocumentId = document.Id,
            RecipientEmail = document.Client.Email!,
            Subject = subject,
            SentAt = DateTime.UtcNow
        };

        await _sentEmailLogRepository.AddAsync(sentEmailLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}