using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBaseSubjects;

public class AssignCrmHerbBaseSubjectOwnerCommand : IRequest<bool>
{
    public CrmHerbBaseSubjectAssignOwnerRequest Request { get; set; } = null!;
}

public class AssignCrmHerbBaseSubjectOwnerHandler : IRequestHandler<AssignCrmHerbBaseSubjectOwnerCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AssignCrmHerbBaseSubjectOwnerHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(AssignCrmHerbBaseSubjectOwnerCommand request, CancellationToken cancellationToken)
    {
        var subjectIds = request.Request.HerbBaseSubjectIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        if (subjectIds.Count == 0)
            throw new BusinessException(400, "请选择要分配的药材基地主体");

        if (request.Request.OwnerUserId.HasValue)
        {
            var ownerExists = await _dbContext.SystemUsers.AnyAsync(
                user => user.Id == request.Request.OwnerUserId.Value && user.IsActive,
                cancellationToken);
            if (!ownerExists)
                throw new BusinessException(404, "负责人不存在");
        }

        var subjects = await _dbContext.CrmHerbBaseSubjects
            .Where(subject => subjectIds.Contains(subject.Id))
            .ToListAsync(cancellationToken);
        if (subjects.Count != subjectIds.Count)
            throw new BusinessException(404, "药材基地主体不存在");

        Guid? operatorUserId = Guid.TryParse(_currentUserService.UserId, out var parsedUserId)
            ? parsedUserId
            : null;
        var remark = request.Request.Remark?.Trim() ?? string.Empty;
        foreach (var subject in subjects)
        {
            var fromOwnerUserId = subject.OwnerUserId;
            subject.AssignOwner(request.Request.OwnerUserId);
            _dbContext.CrmTransferRecords.Add(CrmTransferRecord.Create(
                CrmCodes.HerbBaseSubjectEntityType,
                subject.Id,
                fromOwnerUserId,
                request.Request.OwnerUserId,
                operatorUserId,
                remark));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
