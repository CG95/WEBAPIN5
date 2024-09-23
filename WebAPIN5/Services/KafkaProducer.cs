using Confluent.Kafka;
using Newtonsoft.Json;
using WebAPIN5.Models;

namespace WebAPIN5.Services
{
    public class KafkaProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;


        // Constructor for testing
        public KafkaProducer(IProducer<Null, string> producer, string topic)
        {
            _producer = producer;
            _topic = topic;
        }

        public KafkaProducer(string brokerList, string topic)
        {
            var config = new ProducerConfig { BootstrapServers = brokerList,RetryBackoffMs=1000 };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _topic = topic;
        }

        public async Task ProduceAsync(OperationMessageDTO message)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = jsonMessage });
        }

        public bool TestConnection()
        {
            try
            {
                // This is just a basic test to see if the broker is reachable
                _producer.Produce(_topic, new Message<Null, string> { Value = "Test connection" });
                _producer.Flush(TimeSpan.FromSeconds(5));
                return true;  // Connection successful
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Kafka connection failed: {ex.Error.Reason}");
                return false;  // Connection failed
            }
        }
    }
}
