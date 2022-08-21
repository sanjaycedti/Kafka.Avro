using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Kafka.Avro.Models;

var config = new ConsumerConfig
{
    BootstrapServers = "localhost:29092",
    GroupId = "people-avro"
};

var schemaRegistryConfig = new SchemaRegistryConfig
{
    Url = "localhost:8081"
};

using var registryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

using var consumer = new ConsumerBuilder<string, person>(config)
    .SetValueDeserializer(new AvroDeserializer<person>(registryClient).AsSyncOverAsync())
    .Build();

consumer.Subscribe("person-avro");

while (true)
{
    var result = consumer.Consume();

    Console.WriteLine($"SSN: {result.Message.Key}, Name: {result.Message.Value.name}");
}