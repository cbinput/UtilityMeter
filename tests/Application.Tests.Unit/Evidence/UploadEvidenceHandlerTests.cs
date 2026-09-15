namespace CleanMinimalApi.Application.Tests.Unit.Evidence;

using System.Text;
using CleanMinimalApi.Application.BackgroundJobs.Commands;
using CleanMinimalApi.Application.Evidence;
using CleanMinimalApi.Application.Evidence.Commands.UploadEvidence;
using MediatR;
using NSubstitute;
using Serilog;
using Shouldly;
using Xunit;

public class UploadEvidenceHandlerTests
{
    [Fact]
    public async Task Handle_ShouldStageNonSeekableContent_AndPreserveHashAndPayload()
    {
        var evidenceRepository = Substitute.For<IEvidenceRepository>();
        var objectStorage = Substitute.For<CleanMinimalApi.Application.Storage.IObjectStorage>();
        var sender = Substitute.For<ISender>();
        var logger = new LoggerConfiguration().CreateLogger();
        var payload = Encoding.UTF8.GetBytes("meter evidence payload");
        byte[]? uploadedPayload = null;

        objectStorage.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), "image/jpeg", Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await using var uploadedStream = call.ArgAt<Stream>(0);
                uploadedStream.CanSeek.ShouldBeTrue();

                using var ms = new MemoryStream();
                await uploadedStream.CopyToAsync(ms);
                uploadedPayload = ms.ToArray();

                return call.ArgAt<string>(1);
            });

        sender.Send(Arg.Any<EnqueueOcrExtractionJobCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new UploadEvidenceHandler(evidenceRepository, objectStorage, sender, logger);

        using var stream = new NonSeekableReadStream(payload);
        var response = await handler.Handle(new UploadEvidenceCommand("meter.jpg", "image/jpeg", stream), CancellationToken.None);

        uploadedPayload.ShouldBe(payload);
        response.FileName.ShouldBe("meter.jpg");
        response.Hash.ShouldBe("4F373D98D3391E1507290FD64C7FCB09FC453A813D72D3BBF03CECD6200A543C");
        await sender.Received(1).Send(Arg.Any<EnqueueOcrExtractionJobCommand>(), Arg.Any<CancellationToken>());
        await evidenceRepository.Received(1).AddAsync(Arg.Is<CleanMinimalApi.Application.Evidence.Entities.Evidence>(item =>
            item.FileName == "meter.jpg"
            && item.ContentType == "image/jpeg"
            && item.Hash == response.Hash), Arg.Any<CancellationToken>());
    }

    private sealed class NonSeekableReadStream(byte[] content) : MemoryStream(content, writable: false)
    {
        public override bool CanSeek => false;
        public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();
        public override long Position
        {
            get => base.Position;
            set => throw new NotSupportedException();
        }
    }
}
