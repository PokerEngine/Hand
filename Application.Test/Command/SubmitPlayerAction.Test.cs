using Application.Command;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Service.Evaluator;
using Application.Test.Service.Randomizer;
using Application.Test.Storage;
using Application.Test.UnitOfWork;
using Domain.Entity;
using Domain.Event;

namespace Application.Test.Command;

public class SubmitPlayerActionTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldSubmitPlayerAction()
    {
        // Arrange
        var unitOfWork = CreateUnitOfWork();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();
        var handUid = await StartHandAsync(unitOfWork, randomizer, evaluator);

        var command = new SubmitPlayerActionCommand
        {
            Uid = handUid,
            Nickname = "Charlie",
            Type = "RaiseBy",
            Amount = 25
        };
        var handler = new SubmitPlayerActionHandler(unitOfWork.Repository, unitOfWork, randomizer, evaluator);

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(handUid, response.Uid);

        var hand = Hand.FromEvents(
            response.Uid,
            randomizer,
            evaluator,
            await unitOfWork.Repository.GetEventsAsync(response.Uid)
        );
        var state = hand.GetState();
        Assert.Equal(3, state.Pot.CurrentBets.Count);

        var detailView = await unitOfWork.Storage.GetDetailViewAsync(hand.Uid);
        Assert.Equal(3, detailView.Pot.CurrentBets.Count);
        Assert.Equal("Charlie", detailView.Pot.CurrentBets[2].Nickname);
        Assert.Equal(25, detailView.Pot.CurrentBets[2].Amount);

        var events = await unitOfWork.EventDispatcher.GetDispatchedEventsAsync(response.Uid);
        Assert.Equal(2, events.Count);
        Assert.IsType<PlayerActedEvent>(events[0]);
    }

    private async Task<Guid> StartHandAsync(
        StubUnitOfWork unitOfWork,
        StubRandomizer randomizer,
        StubEvaluator evaluator
    )
    {
        var handler = new StartHandHandler(unitOfWork.Repository, unitOfWork, randomizer, evaluator);
        var command = new StartHandCommand
        {
            TableUid = Guid.NewGuid(),
            TableType = "Cash",
            Rules = new StartHandCommandRules
            {
                Game = "NoLimitHoldem",
                SmallBlind = 5,
                BigBlind = 10,
                MaxSeat = 6
            },
            Table = new StartHandCommandTable
            {
                Positions = new StartHandCommandPositions
                {
                    SmallBlindSeat = 1,
                    BigBlindSeat = 2,
                    ButtonSeat = 6
                },
                Players = [
                    new()
                    {
                        Nickname = "Alice",
                        Seat = 1,
                        Stack = 1000
                    },
                    new()
                    {
                        Nickname = "Bobby",
                        Seat = 2,
                        Stack = 1000
                    },
                    new()
                    {
                        Nickname = "Charlie",
                        Seat = 6,
                        Stack = 1000
                    }
                ]
            }
        };
        var response = await handler.HandleAsync(command);
        await unitOfWork.EventDispatcher.ClearDispatchedEventsAsync(response.Uid);
        return response.Uid;
    }

    private StubUnitOfWork CreateUnitOfWork()
    {
        var repository = new StubRepository();
        var storage = new StubStorage();
        var eventDispatcher = new StubEventDispatcher();
        return new StubUnitOfWork(repository, storage, eventDispatcher);
    }
}
