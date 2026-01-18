namespace Terrarium.Core.Interfaces.Services;

public interface ITerrariumEventBus
{
    void Publish<T>(T message) where T : class;
    void Subscribe<T>(Action<T> handler) where T : class;
}