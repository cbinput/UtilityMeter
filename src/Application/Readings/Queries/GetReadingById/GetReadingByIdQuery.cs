namespace CleanMinimalApi.Application.Readings.Queries.GetReadingById;

using CleanMinimalApi.Application.Common.Exceptions;
using CleanMinimalApi.Application.Readings.Entities;
using MediatR;

public sealed record GetReadingByIdQuery(Guid Id) : IRequest<Reading>;

public sealed class GetReadingByIdHandler(IReadingsRepository readingsRepository)
    : IRequestHandler<GetReadingByIdQuery, Reading>
{
    private readonly IReadingsRepository readingsRepository = readingsRepository ?? throw new ArgumentNullException(nameof(readingsRepository));

    public async Task<Reading> Handle(GetReadingByIdQuery request, CancellationToken cancellationToken)
    {
        var reading = await this.readingsRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new NotFoundException($"Reading with id {request.Id} not found");

        return reading;
    }
}
