namespace QPS.Application.Features.Crm;

public static class CrmCodes
{
    public const string HerbBaseEntityType = "CRM_HERB_BASE";
    public const string HerbBaseSubjectEntityType = "CRM_HERB_BASE_SUBJECT";
    public const string VendorEntityType = "CRM_VENDOR";
    public const string MainProductAttributeCode = "CRM_MAIN_PRODUCT";

    public static class Status
    {
        public const string Pending = "PENDING";
        public const string Following = "FOLLOWING";
        public const string Interested = "INTERESTED";
        public const string Deal = "DEAL";
        public const string Lost = "LOST";
    }

    public static class FollowResult
    {
        public const string Connected = "CONNECTED";
        public const string Interested = "INTERESTED";
    }
}
