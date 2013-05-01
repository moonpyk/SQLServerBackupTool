using i18n;
using System.Web;
using System.Web.Mvc;

namespace SQLServerBackupTool.Web.Lib.Mvc
{
    public abstract class SsbtWebViewPage : WebViewPage, ILocalizing
    {
        private readonly ILocalizingService _service = new LocalizingService();
        public IHtmlString _(string text)
        {
            return MvcHtmlString.Create(__(text));
        }

        public string __(string text)
        {
            return _service.GetText(text, Request.UserLanguages);
        }
    }

    public abstract class SsbtWebViewPage<T> : WebViewPage<T>, ILocalizing
    {
        private readonly ILocalizingService _service = new LocalizingService();

        public IHtmlString _(string text)
        {
            return MvcHtmlString.Create(__(text));
        }

        public string __(string text)
        {
            return _service.GetText(text, Request.UserLanguages);
        }
    }
}
