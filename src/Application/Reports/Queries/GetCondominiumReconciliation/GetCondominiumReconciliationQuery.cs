namespace CleanMinimalApi.Application.Reports.Queries.GetCondominiumReconciliation;

using CleanMinimalApi.Application.Reports;
using CleanMinimalApi.Application.Reports.Dtos;
using MediatR;

public sealed record GetCondominiumReconciliationQuery(Guid CondominiumId, Guid BillingPeriodId)
    : IRequest<CondominiumReconciliationDto>;

public sealed class GetCondominiumReconciliationHandler(ICondominiumReconciliationRepository repository)
    : IRequestHandler<GetCondominiumReconciliationQuery, CondominiumReconciliationDto>
{
    private readonly ICondominiumReconciliationRepository repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<CondominiumReconciliationDto> Handle(
        GetCondominiumReconciliationQuery request,
        CancellationToken cancellationToken)
    {
        var data = await this.repository.GetAsync(request.CondominiumId, request.BillingPeriodId, cancellationToken)
            ?? throw new KeyNotFoundException($"Condominium with id {request.CondominiumId} was not found.");
        var result = CondominiumReconciliationCalculator.Calculate(data, request.BillingPeriodId);

        return new CondominiumReconciliationDto(
            result.CondominiumId,
            result.BillingPeriodId,
            result.MainConsumption,
            result.SubMeterConsumption,
            result.Difference,
            result.DifferencePercentage,
            result.Suspects.Select(issue => new CondominiumPropertyIssueDto(
                issue.PropertyId,
                issue.PropertyName,
                issue.Consumption,
                issue.Reasons)).ToList(),
            result.Explanation);
    }
}