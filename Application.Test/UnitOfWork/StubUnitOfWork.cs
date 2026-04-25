using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Storage;

namespace Application.Test.UnitOfWork;

public class StubUnitOfWork(
    StubHandRepository handRepository,
    StubHandStorage handStorage,
    StubEventDispatcher eventDispatcher
) : Application.UnitOfWork.UnitOfWork(handRepository, handStorage, eventDispatcher)
{
    public readonly StubHandRepository HandRepository = handRepository;
    public readonly StubHandStorage HandStorage = handStorage;
    public readonly StubEventDispatcher EventDispatcher = eventDispatcher;
}
