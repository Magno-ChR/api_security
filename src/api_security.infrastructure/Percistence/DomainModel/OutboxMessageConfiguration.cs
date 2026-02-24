using api_security.domain.Abstractions;
using Joseco.Outbox.Contracts.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace api_security.infrastructure.Percistence.DomainModel;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage<DomainEvent>>
{
    private const string Schema = "outbox";

    public void Configure(EntityTypeBuilder<OutboxMessage<DomainEvent>> builder)
    {
        builder.ToTable("outboxMessage", Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("outboxId");

        builder.Property(x => x.Created)
            .HasColumnName("created");

        builder.Property(x => x.Type)
            .HasColumnName("type");

        builder.Property(x => x.Processed)
            .HasColumnName("processed");

        builder.Property(x => x.ProcessedOn)
            .HasColumnName("processedOn");

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        var contentConverter = new ValueConverter<DomainEvent, string>(
            obj => JsonConvert.SerializeObject(obj, jsonSettings),
            stringValue => (DomainEvent)JsonConvert.DeserializeObject(stringValue, jsonSettings)!);

        builder.Property(x => x.Content)
            .HasConversion(contentConverter)
            .HasColumnName("content");
    }
}
