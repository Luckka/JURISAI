// Registra o serializer padrão para todas as Lambda Functions deste assembly.
[assembly: Amazon.Lambda.Core.LambdaSerializer(
    typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
