using DAX.EventProcessing.Dispatcher;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topos.Config;
using Topos.InMem;

namespace OpenFTTH.EventWatchAndFetch
{
    public class EventConsumer<BaseEventType> : BackgroundService
    {
        private readonly ILogger<EventConsumer<BaseEventType>> _logger;

        private readonly IToposTypedEventObservable<BaseEventType> _eventDispatcher;

        private readonly string _kafkaServer;

        private readonly string _topicName;

        private InMemPositionsStorage _positionsStorage = new InMemPositionsStorage();

        private IDisposable _consumer;
        

        public EventConsumer(ILogger<EventConsumer<BaseEventType>> logger, IToposTypedEventObservable<BaseEventType> eventObservable, string kafkaServer, string topicName)
        {
            _logger = logger;
            _eventDispatcher = eventObservable;
            _kafkaServer = kafkaServer;
            _topicName = topicName;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Event consumer started at: {time}", DateTimeOffset.Now);

            _consumer = _eventDispatcher.Config(_topicName + "-", c => c.UseKafka(_kafkaServer))
                          .Logging(l => l.UseSerilog())
                           .Positions(p => p.StoreInMemory(_positionsStorage))
                          .Topics(t => t.Subscribe(_topicName))
                          .Start();

            stoppingToken.Register(() =>
                _logger.LogDebug($"Event consumer is stopping.."));

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            

            _logger.LogDebug($"Event consumer is stopping...");
        }

        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
