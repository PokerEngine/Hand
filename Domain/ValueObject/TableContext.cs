namespace Domain.ValueObject;

public record TableContext
{
    public required TableUid TableUid { get; init; }
    public required TableType TableType { get; init; }
}
