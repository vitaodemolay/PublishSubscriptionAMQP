# Publish & Subscribe Framework


This is a simple library for help you use the Publish/Subscribe AMQP Pattern in your application. 
It supports use the structure of Azure ServiceBus and RabbitMQ. 
It have a separeted project with two Interfaces for you easily apply Dependecy injection.

## Getting Started

1.	Installation library
	You need to install / Add this library in your project application. 
	
	* [VMRCPACK.PublishSubscribe.Interfaces](https://www.nuget.org/packages/VMRCPACK.PublishSubscribe.Interfaces/) - Nuget Pack with Interface project. Used for Dependency Injection.
	* [VMRCPACK.PublishSubscribe.AzureServiceBus](https://www.nuget.org/packages/VMRCPACK.PublishSubscribe.AzureServiceBus/) - Nuget PAck with project focused on Azure ServiceBus implementation. 
	* [VMRCPACK.PublishSubscribe.RabbitMQ](https://www.nuget.org/packages/VMRCPACK.PublishSubscribe.RabbitMQ/) - Nuget Pack with project focused on RabbitMQ implementation.
	
	
2.	Dependencies
	If you want use this library with RabbitMQ, first you must be sure that the your host RabbitMQ is enable with [Delayed Message Plugin](https://github.com/rabbitmq/rabbitmq-delayed-message-exchange).  

3.	Coding
	For you starting coding with this library is very simple. On sequency a little code fragment: 
		
		
### Publish sample
		
```csharp
//....
[TestMethod]
public void Publish_Test() // Test for send message (Using the Azure ServiceBus) 
{
	//The constructor need connectionString of your Servicebus and the topic name. If the topic not exists the library will be create the topic on your host.
    Publish publish = new Publish(connectionString, topic);
	
	//The send method send your object for the topic informed. This object will be serialized on a json object.
    publish.Send(new YourCommandObject { id = 10 });

}

//....
[TestMethod]
public void Publish_Test() // Test for send message (Using the RabbitMQ) 
{
	//The constructor need host name of your RabbitMQ and the topic name. If the topic not exists the library will be create the topic on your host.
    Publish publish = new Publish(topic, host);
	
	//The send method send your object for the topic informed. This object will be serialized on a json object.
    publish.Send(new YourCommandObject { id = 10 });
}
//....
```
		
### Subscribe sample

* Create or use two Interfaces: one for Request and other to Notification. Later Create Your Classes for Commands and implement the appropriate interface. 
```csharp
//......
interface IRequest
{

}

interface INotification
{

}

class YourCommandObject : INotification
{
    public int id { get; set; }
}
//....
```		



* Create a method for callback for whe a new message will be receive
```csharp		
// This Method will be used for callback
private void receiveCallback(dynamic message)
{
	YourCommandObject cmd = message;
	this.teste = cmd.id;
	
	// your code
	
	//....
	return;
}
//.......
```

* On the sequency is the test of receiver		
```csharp		
[TestMethod]
public void Subscribe_Test() // Test for receiver message (Using the Azure ServiceBus) 
{
	//Subscribe class need informed Request and Notification type for correct interpretation 
	//of json object on Queue. The library read the assembly to find all objects what use this types for create a correct
	//instace object for your application. 
	//The constructor need the type of a object from your library for to read the assembly and to find all other classes that use
	//the Request or Notification interface informed. Need too connection string, topic and subscription. 
	//If the topic and / or subscrition not exists, the library will be create the topic and subscription on your host.
	Subscribe subscribe = new Subscribe<IRequest, INotification>(typeof(YourCommandObject), connectionString, topic, subscription);
	
	//The Subscribe create a Listener and when receive a message with a object equivalent the objects into your library 
	//the Subscribe invoke your callback method informed.
	subscribe.OnMessage(receiveCallback);
	
	// your code
	while (true)
	{
		//....
	}
}
//....

[TestMethod]
public void Subscribe_Test() // Test for receiver message (Using the RabbitMQ) 
{
	//Subscribe class need informed Request and Notification type for correct interpretation 
	//of json object on Queue. The library read the assembly to find all objects what use this types for create a correct
	//instace object for your application. 
	//The constructor need the type of a object from your library for to read the assembly and to find all other classes that use
	//the Request or Notification interface informed. Need too topic, host name and subscription name. 
	//If the topic and / or subscrition not exists, the library will be create the topic and subscription on your host.
	Subscribe subscribe = new Subscribe<IRequest, INotification>(typeof(YourCommandObject), topic, host, subscriptionName: subscription);
	
	//The Subscribe create a Listener and when receive a message with a object equivalent the objects into your library 
	//the Subscribe invoke your callback method informed.
	subscribe.OnMessage(receiveCallback);
	
	// your code
	while (true)
	{
		//....
	}
}


//....
```
		
	
	
