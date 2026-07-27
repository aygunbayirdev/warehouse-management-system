namespace WMS.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
