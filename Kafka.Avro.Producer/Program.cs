using Bogus.Extensions.UnitedStates;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Kafka.Avro.Models;

var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:29092"
};

var schemaRegistryConfig = new SchemaRegistryConfig
{
    Url = "http://localhost:8081"
};

using var registryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

using var producer = new ProducerBuilder<string, person>(producerConfig)
    .SetValueSerializer(new AvroSerializer<person>(registryClient))
    .Build();

var seed = 0;

while (true)
{
    var p = new Bogus.Person(seed: ++seed);

    var ssn = p.Ssn();
    var fullName = p.FullName;

    var person = new person
    {
        ssn = ssn,
        name = fullName
    };

    var result = await producer.ProduceAsync("person-avro", new Message<string, person> { Key = ssn, Value = person });

    Console.WriteLine($"SSN: {result.Message.Key}, Name: {result.Message.Value.name}");

    await Task.Delay(1000);
}