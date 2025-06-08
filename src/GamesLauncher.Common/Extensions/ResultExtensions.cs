using Flow.Launcher.Plugin;

namespace GamesLauncher.Common.Extensions
{
    public static class ResultExtensions
    {
        public static string GetInternalGameId(this Result result) => $"{result.SubTitle}_{result.Title}";
    }
}
