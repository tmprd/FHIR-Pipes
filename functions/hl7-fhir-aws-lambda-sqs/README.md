# HL7v2 to FHIR using AWS Lambda & SQS
# Development
### Requirements
* Java 1.11+
* [Apache Maven](https://maven.apache.org/)
* [LinuxForHealth HL7v2-FHIR Converter](https://github.com/LinuxForHealth/hl7v2-fhir-converter)
* Optional: [AWS SAM (Serverless Application Model) CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html)
* Optional: [GNU Make](https://www.gnu.org/software/make/)
* Optional: [Docker](https://www.docker.com/)

### Environment Configuration
* Add local AWS Credentials

### Build & Deploy
```shell
# Build & Deploy CloudFormation stack with Make
make build-deploy
make delete-stack
make check-size

# Build with Maven
mvn clean install

# Run local unit & integration tests
mvn test
# Run test in docker
sam local invoke

# Deploy with AWS SAM CLI & CloudFormation template
sam build -t template.yaml
sam deploy -g -t template.yaml
```

### TODO
- [ ] Extract reusable mapping logic into separate module
- [ ] Light dependency injection with something like [Dagger](https://dagger.dev/) at compile time

# Architecture
### Mapping Implementation
* [LinuxForHealth HL7v2-FHIR Converter](https://github.com/LinuxForHealth/hl7v2-fhir-converter)
    * [HAPI HL7](https://hapifhir.github.io/hapi-hl7v2/)
    * [HAPI FHIR](https://hapifhir.io/)

### Event-Driven ETL
* The lambda function is triggered by incoming HL7 messages from an SQS queue
* The function transforms HL7 messages into FHIR JSON
* Individual failures are tracked and returned as a [partial batch response](https://docs.aws.amazon.com/prescriptive-guidance/latest/lambda-event-filtering-partial-batch-responses-for-sqs/benefits-partial-batch-responses.html)
* Successful transformations are sent to another SQS queue holding FHIR JSON

### Infrastructure as Code
An AWS CloudFormation template provides configuration for:
* AWS Lambda transformation function
* Incoming HL7 queue
* Outgoing FHIR queue
* IAM roles + policies for access to queues

# Review
* Advantages
    * Reuse of stable HAPI HL7 & FHIR infrastructure
* Disadvantages
    * High memory usage from the mapping library used and Java generally
    * Startup is potentially too slow for certain use cases
    * Each queue should be limited to specific HL7 message types & FHIR resources from specific FHIR versions. 