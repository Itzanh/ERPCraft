using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace serverHashes
{
    public class PubSub
    {
        private ConcurrentDictionary<string, TopicSubscriptionsCollection> subscriptions = new ConcurrentDictionary<string, TopicSubscriptionsCollection>();

        public PubSub(string[] topics)
        {
            foreach (string topicName in topics)
            {
                subscriptions.TryAdd(topicName, new TopicSubscriptionsCollection());
            }
        }

        public bool addTopic(string topic)
        {
            return subscriptions.TryAdd(topic, new TopicSubscriptionsCollection());
        }

        public bool removeTopic(string topic)
        {
            return subscriptions.TryRemove(topic, out _);
        }

        public bool onSubscribe(NetEventIO sender, string topicName)
        {
            TopicSubscriptionsCollection value;
            try
            {
                subscriptions.TryGetValue(topicName, out value);
                if (value == null) return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            value.Add(sender);
            return true;
        }

        public bool onUnsubscribe(NetEventIO sender, string topicName)
        {
            TopicSubscriptionsCollection value;
            try
            {
                subscriptions.TryGetValue(topicName, out value);
                if (value == null) return false;
            }
            catch (Exception)
            {
                return false;
            }

            bool result = value.Remove(sender);
            return result;
        }

        public void onPush(string topicName, SubscriptionChangeType changeType, int pos, string newValue)
        {
            new Thread(new ThreadStart(() =>
            {
                TopicSubscriptionsCollection value;
                try
                {
                    subscriptions.TryGetValue(topicName, out value);
                    if (value == null) return;
                }
                catch (Exception)
                {
                    return;
                }

                foreach (NetEventIO client in value.ToList())
                {
                    if (client != null)
                        client.subscriptionPush(topicName, changeType, pos, newValue);
                }

            })).Start();
        }

        public bool onUnsubscribeAll(NetEventIO client)
        {
            List<string> topics = new List<string>(subscriptions.Keys);
            TopicSubscriptionsCollection value;

            foreach (string topicName in topics)
            {
                try
                {
                    subscriptions.TryGetValue(topicName, out value);

                    value.Remove(client);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal class TopicSubscriptionsCollection
    {
        private readonly Semaphore MUTEX;
        private readonly List<NetEventIO> subscriptions;

        public TopicSubscriptionsCollection()
        {
            this.MUTEX = new Semaphore(1, 1);
            this.subscriptions = new List<NetEventIO>();
        }

        public void Add(NetEventIO client)
        {
            lock (MUTEX)
                this.subscriptions.Add(client);
        }

        public bool Remove(NetEventIO client)
        {
            lock (MUTEX)
                return this.subscriptions.Remove(client);
        }

        public List<NetEventIO> ToList()
        {
            lock (MUTEX)
            {
                List<NetEventIO> copiedArray = new List<NetEventIO>();

                foreach (NetEventIO item in this.subscriptions)
                {
                    copiedArray.Add(item);
                }

                return copiedArray;
            }
        }
    }
}
