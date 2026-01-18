using System;
using CommunityToolkit.Mvvm.Messaging;
using Terrarium.Core.Interfaces.Services;

namespace Terrarium.Avalonia.Services;

public class MvvmEventBus : ITerrariumEventBus
{
    public void Publish<T>(T message) where T : class
    {
        WeakReferenceMessenger.Default.Send(message);
    }

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        WeakReferenceMessenger.Default.Register<T>(this, (r, m) => handler(m));
    }
}