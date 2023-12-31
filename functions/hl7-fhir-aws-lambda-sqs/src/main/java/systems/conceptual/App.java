package systems.conceptual;

import java.util.ArrayList;
import java.util.List;

import org.apache.commons.math3.stat.descriptive.summary.Product;
import com.amazonaws.services.lambda.runtime.Context;
import com.amazonaws.services.lambda.runtime.RequestHandler;
import com.amazonaws.services.lambda.runtime.LambdaLogger;
import com.amazonaws.services.lambda.runtime.events.SQSBatchResponse;
import com.amazonaws.services.lambda.runtime.events.SQSEvent;
import com.amazonaws.services.lambda.runtime.events.SQSEvent.SQSMessage;

import software.amazon.awssdk.services.sqs.SqsClient;
import software.amazon.awssdk.services.sqs.model.GetQueueUrlRequest;
import software.amazon.awssdk.services.sqs.model.SendMessageBatchRequest;
import software.amazon.awssdk.services.sqs.model.SendMessageBatchRequestEntry;

public class App implements RequestHandler<SQSEvent, SQSBatchResponse> {
  private static final HL7toFHIR fhirConverter = new HL7toFHIR();
  private final SqsClient sqsClient;
  private final String outgoingQueueUrl;

  public App() {
    // Initialize the SDK client outside of the handler method so that it can be reused for subsequent invocations
    sqsClient = DependencyFactory.sqsClient();
    var getQueueRequest = GetQueueUrlRequest.builder().queueName("fhir-queue").build();
    System.out.println("Getting queue URL for " + getQueueRequest.queueName());
    outgoingQueueUrl =  sqsClient.getQueueUrl(getQueueRequest).queueUrl();
  }

  @Override
  public SQSBatchResponse handleRequest(SQSEvent event, Context context) {
    // Receive SQS event from HL7 queue
    LambdaLogger logger = context.getLogger();
    logger.log("Received event: " + event.getClass());
    
    List<SQSMessage> records = event.getRecords();
    if (records == null) {
      logger.log("No records found in event");
      return SQSBatchResponse.builder().build();
    }

    // Read events, extract HL7 messages, transform to FHIR JSON
    var fhirJsonMessages = new ArrayList<SendMessageBatchRequestEntry>();
    var batchItemFailures = new ArrayList<SQSBatchResponse.BatchItemFailure>();
    for (SQSMessage msg : records) {
      try {
        var messageId = msg.getMessageId();
        logger.log("Processing message: " + messageId);
        // Read HL7 message
        var messageBody = msg.getBody();
        if (messageBody == null) {
          logger.log("No message body found");
          continue;
        }
        String fhirJson = fhirConverter.convert(messageBody);
        logger.log(fhirJson);
        SendMessageBatchRequestEntry message = SendMessageBatchRequestEntry.builder().id(messageId).messageBody(fhirJson).build();
        fhirJsonMessages.add(message);
      } catch (Exception e) {
          var errorMessage = e.getMessage();
          logger.log("Error: " + errorMessage);
          SQSBatchResponse.BatchItemFailure failure = new SQSBatchResponse.BatchItemFailure(msg.getMessageId());
          batchItemFailures.add(failure);
          continue;
      }
    }

    // Send to FHIR SQS queue
    if (!fhirJsonMessages.isEmpty()) {
      var messageBatchRequest = SendMessageBatchRequest.builder().queueUrl(outgoingQueueUrl).entries(fhirJsonMessages).build();
      var sendMessageBatchResponse = sqsClient.sendMessageBatch(messageBatchRequest);
      logger.log("Response: " + sendMessageBatchResponse.toString());
      logger.log("Have any messages failed?" + sendMessageBatchResponse.hasFailed());
    }

    // Return any failures
    return SQSBatchResponse.builder().withBatchItemFailures(batchItemFailures).build();
  }
}
