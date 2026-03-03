using api_security.domain.Abstractions;
using api_security.domain.Entities.Patients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace api_security.application.Integration.Patients;

internal sealed class SyncPatientFromIntegrationHandler : IRequestHandler<SyncPatientFromIntegrationCommand, Unit>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncPatientFromIntegrationHandler> _logger;

    public SyncPatientFromIntegrationHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        ILogger<SyncPatientFromIntegrationHandler> logger)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(SyncPatientFromIntegrationCommand request, CancellationToken cancellationToken)
    {
        var first = request.FirstName ?? string.Empty;
        var middle = request.MiddleName ?? string.Empty;
        var last = request.LastName ?? string.Empty;
        var doc = request.DocumentNumber ?? string.Empty;

        if (request.IsCreated)
        {
            var existing = await _patientRepository.GetByIdAsync(request.PatientId, true);
            if (existing is not null)
            {
                _logger.LogInformation("Patient {PatientId} already exists, skipping create", request.PatientId);
                return Unit.Value;
            }
            var patient = Patient.Create(request.PatientId, first, middle, last, doc);
            await _patientRepository.AddAsync(patient);
            _logger.LogInformation("Patient created from integration: {PatientId}", request.PatientId);
        }
        else
        {
            var patient = await _patientRepository.GetByIdAsync(request.PatientId);
            if (patient is null)
            {
                _logger.LogWarning("Patient {PatientId} not found for update, creating as new", request.PatientId);
                patient = Patient.Create(request.PatientId, first, middle, last, doc);
                await _patientRepository.AddAsync(patient);
            }
            else
            {
                patient.UpdateDetails(first, middle, last, doc);
            }
            _logger.LogInformation("Patient updated from integration: {PatientId}", request.PatientId);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}
