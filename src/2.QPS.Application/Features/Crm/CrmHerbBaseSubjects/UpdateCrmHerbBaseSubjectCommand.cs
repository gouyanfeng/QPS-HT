using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBaseSubjects;

public class UpdateCrmHerbBaseSubjectCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public CrmHerbBaseSubjectUpdateRequest Request { get; set; } = null!;
}

public class UpdateCrmHerbBaseSubjectHandler : IRequestHandler<UpdateCrmHerbBaseSubjectCommand, bool>
{
    private readonly IDbContext _dbContext;

    public UpdateCrmHerbBaseSubjectHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateCrmHerbBaseSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (subject == null)
            throw new BusinessException(404, "药材基地主体不存在");

        subject.UpdateBasicInfo(
            request.Request.SubjectName,
            request.Request.DisplayName,
            request.Request.SubjectType,
            request.Request.Status,
            request.Request.Grade,
            request.Request.Score,
            request.Request.Remark);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
