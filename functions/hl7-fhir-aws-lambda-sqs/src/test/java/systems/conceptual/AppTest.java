package systems.conceptual;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;
import static org.junit.jupiter.api.Assertions.assertThrows;
import org.junit.Ignore;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;

import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.UUID;

import ca.uhn.hl7v2.HL7Exception;

import com.amazonaws.services.lambda.runtime.events.SQSEvent;
import com.amazonaws.services.lambda.runtime.events.SQSEvent.SQSMessage;
import software.amazon.awssdk.services.sqs.SqsClient;
import software.amazon.awssdk.services.sqs.model.GetQueueUrlRequest;
import software.amazon.awssdk.services.sqs.model.ReceiveMessageRequest;
import software.amazon.awssdk.services.sqs.model.SendMessageBatchRequest;
import software.amazon.awssdk.services.sqs.model.SendMessageBatchRequestEntry;

public class AppTest 
{
    private final SqsClient sqsClient = DependencyFactory.sqsClient();

    private final HL7toFHIR fhirConverter = new HL7toFHIR();

    private final String WORKING_DIR = new File("src/test/resources/hl7").getAbsolutePath();

    @ParameterizedTest
    @ValueSource(strings = {"ADT_A01_ex1_v2.8.2.hl7", "ADT_unix.hl7", "OML_O21_ex1.hl7", "ORU-multiline-short-mixed.hl7"})
    public void Convert_HL7_Returns_FHIR_JSON(String messageFilename) throws IOException, HL7Exception, IllegalArgumentException {
        var hl7Content = readWorkingFile(messageFilename);
        var fhirJson = fhirConverter.convert(hl7Content);
        assertTrue(fhirJson.contains("resourceType"));
    }

    @ParameterizedTest
    @ValueSource(strings = {"Malformed_ADT_A01_ex1_v2.8.2.hl7"})
    public void Convert_Malformed_HL7_Throws_Error(String messageFilename) throws IOException, HL7Exception, IllegalArgumentException {
        var hl7Content = readWorkingFile(messageFilename);
        Exception exception = assertThrows(
            Exception.class,
            () -> fhirConverter.convert(hl7Content),
            "Expected convert() to throw, but it didn't"
        );
    }

    @Disabled("TODO Integration Test")
    @ParameterizedTest
    @ValueSource(strings = "ADT_A01_ex1_v2.8.2.hl7")
    public void Convert_HL7_Sends_To_FHIR_Queue(String messageFilename) throws IOException {
        var hl7Content = readWorkingFile(messageFilename);
        // New message emulating HL7 queue sender
        SQSMessage msg = new SQSMessage();
        msg.setBody(hl7Content);
        msg.setMessageId(UUID.randomUUID().toString());
        SQSEvent event = new SQSEvent();
        event.setRecords(Arrays.asList(msg));
        // Send to Lambda
        var batchResponse = new App().handleRequest(event, new TestContext());
        System.out.println(batchResponse);

        // FHIR Queue should have new message
    }

    // @Disabled("Integration Test")
    @ParameterizedTest
    @ValueSource(strings = "Malformed_ADT_A01_ex1_v2.8.2.hl7")
    public void Convert_Malformed_HL7_Sends_Partial_Batch_Response(String messageFilename) throws IOException {
        var hl7Content = readWorkingFile(messageFilename);
        // New message emulating HL7 queue sender
        SQSMessage msg = new SQSMessage();
        msg.setBody(hl7Content);
        msg.setMessageId(UUID.randomUUID().toString());
        SQSEvent event = new SQSEvent();
        event.setRecords(Arrays.asList(msg));
        // Send to Lambda
        var batchResponse = new App().handleRequest(event, new TestContext());
        System.out.println(batchResponse);

        // Should have returned partial response
    }

    @Disabled("TODO Integration Test")
    @ParameterizedTest
    @ValueSource(strings = "ADT_A01_ex1_v2.8.2.hl7")
    public void HL7_Queue_Triggers_FHIR_Conversion_Queue(String messageFilename) throws IOException {
        String hl7Content = readWorkingFile(messageFilename);
        
        // Send new message to HL7 queue
        String hl7QueueUrl = sqsClient.getQueueUrl(GetQueueUrlRequest.builder().queueName("hl7-queue").build()).queueUrl();
        
        var sendMessageBatchResponse = sqsClient.sendMessageBatch(SendMessageBatchRequest.builder()
            .queueUrl(hl7QueueUrl)
            .entries(SendMessageBatchRequestEntry.builder().id("id1").messageBody(hl7Content).build())
            .build());
        
        assertEquals(false, sendMessageBatchResponse.hasFailed());
        
        // Check that FHIR Converter Lambda was triggered by HL7 queue
        
        // FHIR Queue should contain new message
        String fhirQueueUrl = sqsClient.getQueueUrl(GetQueueUrlRequest.builder().queueName("fhir-queue").build())
            .queueUrl();
        
        var receiveMessageResponse = sqsClient.receiveMessage(ReceiveMessageRequest.builder()
            .queueUrl(fhirQueueUrl)
            .maxNumberOfMessages(5)
            .build());
        
        // TODO check for original message
        receiveMessageResponse.messages().forEach(message -> {
            System.out.println(message.body());
        });
    }

    private String readWorkingFile(String filename) throws IOException {
        var filePath = Path.of(WORKING_DIR, filename).toString();
        return Files.readString(Paths.get(filePath), StandardCharsets.ISO_8859_1);
    }
}
