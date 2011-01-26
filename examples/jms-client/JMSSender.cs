using System;
using javax.jms;
using javax.naming;

public class JMSSender {

	public static void Main(string[] args) {
		Console.WriteLine("Looking up context factory");
		Context context = NamingContextHelper.getContext();

		Console.WriteLine("Looking up connection factory");
		QueueConnectionFactory queueConnectionFactory =
				new QueueConnectionFactoryJNIImpl(context.lookup(new java.lang.String("ConnectionFactory")));

		Console.WriteLine("Looking up queue");
		Queue queue = new QueueJNIImpl(context.lookup(new java.lang.String("queue/A")));

		Console.WriteLine("Creating queue connection");
		QueueConnection queueConnection = 
			queueConnectionFactory.createQueueConnection();

		Console.WriteLine("Creating queue session");
		QueueSession queueSession = 
			queueConnection.createQueueSession(false, 
					SessionJNIImpl.AUTO_ACKNOWLEDGE);

		Console.WriteLine("Creating sender");
		QueueSender queueSender = queueSession.createSender(queue);
		TextMessage message = queueSession.createTextMessage();
		for (int i = 0; i < 5; i++) {
			message.setText(new java.lang.String("This is message " + (i + 1)));
			Console.WriteLine("Sending message: " + message.getText());
			queueSender.send(message);
		}
		queueConnection.close();
	}
}
