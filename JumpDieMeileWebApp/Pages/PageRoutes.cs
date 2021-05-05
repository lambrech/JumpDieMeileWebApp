namespace JumpDieMeileWebApp.Pages
{
    using Microsoft.AspNetCore.Components;

    public static class PageRoutes
    {
        public const string RegisterRunnerRoute = "/register-runner";

        public const string RegisterRunRoute = "/register-run";

        public const string RegisterSponsorRoute = "/register-sponsor";

        public static string CreateRoute(this NavigationManager nav, string relativeRoute)
        {
            return nav.BaseUri.TrimEnd().TrimEnd('/') + relativeRoute;
        }
    }
}