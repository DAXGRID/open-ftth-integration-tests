﻿using DAX.EventProcessing.Dispatcher;
using DAX.EventProcessing.Dispatcher.Topos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFTTH.Events.RouteNetwork;
using System;
using System.Threading;
using System.Threading.Tasks;
using Topos.Config;
using Topos.InMem;
using Topos.Logging.Console;

namespace OpenFTTH.EventWatchAndFetch
{
    public class WaitForAndFetchEvents<BaseEventType> : IDisposable
    {
        ILoggerFactory _loggerFactory;

        private string _kafkaServer;

        private string _topicName;

        private ILogger _logger;

        private InMemPositionsStorage _positionsStorage = new InMemPositionsStorage();

        private EventObserver<BaseEventType> _eventObserver;

        private EventConsumer<BaseEventType> _eventConsumer;


        public WaitForAndFetchEvents(ILoggerFactory loggerFactory, string kafkaServer, string topicName)
        {
            _loggerFactory = loggerFactory;
            _kafkaServer = kafkaServer;
            _topicName = topicName;
            _logger = new Logger<WaitForAndFetchEvents<BaseEventType>>(_loggerFactory);
        }

        public async Task WaitForEvents(Predicate<BaseEventType> startCriteria, Predicate<BaseEventType> stopCriteria)
        {
            var eventObservable = new ToposTypedEventObservable<BaseEventType>(new Logger<ToposTypedEventMediator<BaseEventType>>(_loggerFactory));

            _eventObserver = new EventObserver<BaseEventType>();
            _eventObserver.Subscribe(eventObservable.OnEvent, startCriteria, stopCriteria);
            _eventConsumer = new EventConsumer<BaseEventType>(new Logger<EventConsumer<BaseEventType>>(_loggerFactory), eventObservable, _kafkaServer, _topicName);

            var tokenSource = new CancellationTokenSource();

            var stoppingToken = tokenSource.Token;

            await _eventConsumer.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                if (_eventObserver.StartCriteriaMeet)
                    _logger.LogDebug("Start criterie meet!");

                if (_eventObserver.StopCriteriaMeet)
                {
                    _logger.LogDebug("Stop criterie meet!");
                    _logger.LogDebug("Cancel event consumer...");
                    tokenSource.Cancel();
                }

                _logger.LogDebug("Waiting... for the miracle to come.");

            }
        }

        public void Dispose()
        {
            if (_eventConsumer != null)
                _eventConsumer.Dispose();
        }
    }
}
