namespace Api.GrpcClients
{
    public static class Configuration
    {
        public static string URIRestServer { get; } = "http://ec2-34-239-177-116.compute-1.amazonaws.com:5000/";
        public static string URIGrpcServer { get; } = "ec2-34-239-177-116.compute-1.amazonaws.com:5001";
    }
}