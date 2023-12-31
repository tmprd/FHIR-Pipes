package systems.conceptual;

import software.amazon.awssdk.auth.credentials.AwsCredentialsProvider;
import software.amazon.awssdk.auth.credentials.EnvironmentVariableCredentialsProvider;
import software.amazon.awssdk.auth.credentials.ProfileCredentialsProvider;
import software.amazon.awssdk.http.urlconnection.UrlConnectionHttpClient;
import software.amazon.awssdk.services.sqs.SqsClient;

/**
 * The module containing all dependencies required by the {@link App}.
 */
public class DependencyFactory {

    private DependencyFactory() {}

    // TODO LocalStack https://docs.localstack.cloud/getting-started/quickstart/

    /**
     * @return an instance of SqsClient
     */
    public static SqsClient sqsClient() {
        AwsCredentialsProvider credentialsProvider;
        if (isLocalEnvironment()) {
            credentialsProvider = ProfileCredentialsProvider.create();
        } else {
            // AWS SAM Local uses Docker environment variables passed in from local profile
            credentialsProvider = EnvironmentVariableCredentialsProvider.create();
        }
        credentialsProvider.resolveCredentials();

        return SqsClient.builder()
                    .credentialsProvider(credentialsProvider)
                    .httpClientBuilder(UrlConnectionHttpClient.builder())
                    .build();
    }

    private static boolean isLocalEnvironment() {
        // Doesn't include SAM docker containers identified by `AWS_SAM_LOCAL`
        return System.getenv("LAMBDA_TASK_ROOT") == null;
    }
}
