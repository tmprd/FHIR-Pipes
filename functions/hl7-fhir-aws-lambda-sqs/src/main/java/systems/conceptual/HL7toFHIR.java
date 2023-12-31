package systems.conceptual;

import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;
import org.apache.commons.io.IOUtils;

import ca.uhn.hl7v2.DefaultHapiContext;
import ca.uhn.hl7v2.HL7Exception;
import ca.uhn.hl7v2.HapiContext;
import ca.uhn.hl7v2.model.Message;
import ca.uhn.hl7v2.model.v26.segment.MSH;
import ca.uhn.hl7v2.util.Hl7InputStreamMessageStringIterator;

import io.github.linuxforhealth.hl7.HL7ToFHIRConverter;
import io.github.linuxforhealth.hl7.message.HL7MessageModel;
import io.github.linuxforhealth.hl7.parsing.HL7HapiParser;

public class HL7toFHIR {

    private static HapiContext HL7Context;

    private static HL7ToFHIRConverter genericConverter = new HL7ToFHIRConverter();

    private static Map<String, HL7MessageModel> messagetemplates = new HashMap<>();
    
    public HL7toFHIR() {
        HL7Context = new DefaultHapiContext();
    }

    /**
     * Convert HL7v2 message to FHIR JSON, using custom converters for certain message types
     */
    public String convert(String hl7messageData) throws HL7Exception, IllegalArgumentException {
        Message message = parseHl7Message(hl7messageData);
        String messageType = getHl7MessageType(message);
        
        if (hasCustomConversion(messageType)) {
            return convertCustom(message, messageType);
        }
        // Use generic FHIR conversion for everything else
        return genericConverter.convert(hl7messageData);
    }


    private String getHl7MessageType(Message message) throws HL7Exception {
        MSH messageHeader = (MSH)message.get("MSH");
        var messageType = messageHeader.getMessageType();
        return messageType.getMsg1_MessageCode().getValue() + "_" + messageType.getMsg2_TriggerEvent().getValue();
    }

    private Message parseHl7Message(String hl7messageData) throws HL7Exception {
        Message hl7message = null;
        InputStream ins = IOUtils.toInputStream(hl7messageData, StandardCharsets.UTF_8);
        Hl7InputStreamMessageStringIterator iterator = new Hl7InputStreamMessageStringIterator(ins);

        if (iterator.hasNext()) {
            HL7HapiParser hparser = new HL7HapiParser();
            hl7message = hparser.getParser().parse(iterator.next());
        }

        return hl7message;
    }

    private boolean hasCustomConversion(String messageType) {
        // check for in list of custom conversions
        // CustomMessageTypes.valueOf(messageType).
        return false;
    }

    private String convertCustom(Message message, String messageType) {
        // TODO use HAPI FHIR
        // TODO serialize to JSON
        // throw new NotImplementedException();
        return null;
    }
}
