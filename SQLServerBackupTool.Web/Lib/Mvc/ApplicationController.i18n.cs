using i18n;
using System.Web;
using System.Web.Mvc;

namespace SQLServerBackupTool.Web.Lib.Mvc
{
    public partial class ApplicationController : ILocalizing
    {
        private static readonly ILocalizingService LocaleService = new LocalizingService();

        public string _(string text)
        {
            if (Request == null)
            {
                return text;
            }

            // // Prefer a stored value to browser-supplied preferences
            // var stored = LanguageSession.GetLanguageFromSession(ControllerContext.HttpContext);
            // if (stored != null)
            // {
            //     return _service.GetText(text, new[] { stored });
            // }

            // Find the most appropriate fit from the user's browser settings
            return LocaleService.GetText(text, Request.UserLanguages);
        }

        IHtmlString ILocalizing._(string text)
        {
            return new MvcHtmlString(_(text));
        }
    }
}