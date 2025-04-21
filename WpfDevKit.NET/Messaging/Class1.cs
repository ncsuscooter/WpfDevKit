using System;
using System.Collections.Generic;
using System.Linq;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Messaging
{
    public interface IMessage
    {

    }

    public interface IMessageBus
    {
        void Publish<TMessage>(TMessage message) where TMessage : IMessage;
        void Subscribe<TMessage>(Action<TMessage> handler) where TMessage : IMessage;
        void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : IMessage;
    }

    public class MessageBus : IMessageBus
    {
        private readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

        public void Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            if (message == null) return;

            var type = typeof(TMessage);
            if (!subscribers.TryGetValue(type, out var handlers)) return;

            foreach (var handler in handlers.OfType<Action<TMessage>>())
                handler.Invoke(message);
        }

        public void Subscribe<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            var type = typeof(TMessage);
            if (!subscribers.TryGetValue(type, out var handlers))
                handlers = subscribers[type] = new List<Delegate>();

            handlers.Add(handler);
        }

        public void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            var type = typeof(TMessage);
            if (subscribers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                    subscribers.Remove(type);
            }
        }
    }

    public static class MessageBusExtensions
    {
        public static IServiceCollection AddLogProvider<TProvider, TOptions>(this IServiceCollection services) =>
            services.AddSingleton<MessageBus>().AddSingleton<IMessageBus>(p => p.GetRequiredService<MessageBus>());
    }
}
