namespace MyWebApi.Application.Auth;

public static class AuthorizationConstants
{
    public static class ClaimTypes
    {
        public const string Permission = "permission";
    }

    public static class Permissions
    {
        public const string ApInitiator = "AP:INITIATOR";
        public const string ApAuthoriser = "AP:AUTHORISER";
        public const string ArInitiator = "AR:INITIATOR";
        public const string ArAuthoriser = "AR:AUTHORISER";
        public const string GlInitiator = "GL:INITIATOR";
        public const string GlAuthoriser = "GL:AUTHORISER";
    }

    public static class Policies
    {
        public const string CanInitiateAp = "CanInitiateAp";
        public const string CanAuthoriseAp = "CanAuthoriseAp";
        public const string CanInitiateAr = "CanInitiateAr";
        public const string CanAuthoriseAr = "CanAuthoriseAr";
        public const string CanInitiateGl = "CanInitiateGl";
        public const string CanAuthoriseGl = "CanAuthoriseGl";
    }
}
