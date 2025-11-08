namespace TransnetAEX.Web.Helpers
{
    public static class LayoutHelper
    {
        /// <summary>
        /// Returns the correct layout file path for a given role.
        /// </summary>
        public static string GetLayoutForRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "_Layout"; // default

            return role switch
            {
                "Admin" => "~/Views/Shared/_LayoutAdmin.cshtml",
                "HR" => "~/Views/Shared/_LayoutHr.cshtml",
                _ => "_Layout"
            };
        }
    }
}
