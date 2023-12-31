MAIN_ARTIFACTS = ./target/hl7-fhir-aws-lambda-sqs*.jar
STACK_NAME = hl7-fhir-lambda-sqs
STACK_TEMPLATE = template.yaml


build-deploy:
	mvn clean package
	sam deploy -t ${STACK_TEMPLATE}
	
delete-stack:
	sam delete --stack-name ${STACK_NAME}

# 272629760 max unzipped
check-size:
	du -hs ${MAIN_ARTIFACTS}
	unzip -Zt ${MAIN_ARTIFACTS}


# `sam build` sets `ARTIFACTS_DIR` to `.aws-sam/build/<project>` by default
build-LambdaHL7toFHIR:
	mvn clean package
	mkdir -p $(ARTIFACTS_DIR)/lib
	cp ${MAIN_ARTIFACTS} $(ARTIFACTS_DIR)/lib/