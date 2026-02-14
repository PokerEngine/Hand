using Domain.Entity;

namespace Application.UnitOfWork;

public interface IUnitOfWork
{
    void RegisterHand(Hand hand);
    Task CommitAsync(bool updateViews = true);
}
