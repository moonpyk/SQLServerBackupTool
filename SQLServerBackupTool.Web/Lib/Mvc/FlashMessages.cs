using SQLServerBackupTool.Lib.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SQLServerBackupTool.Web.Lib.Mvc
{
    public static class FlashMessagesHelper
    {
        private const string TempDataKey = "FlashMessages";

        public static void AddFlashMessage([NotNull] this ControllerBase c, string message, FlashMessageType t)
        {
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }

            var container = c.TempData[TempDataKey] as List<FlashMessage>;

            if (container == null)
            {
                container = new List<FlashMessage>();
                c.TempData[TempDataKey] = container;
            }

            container.Add(new FlashMessage
            {
                Message = message,
                Type = t,
            });
        }

        public static IHtmlString RenderFlashMessages([NotNull] this HtmlHelper h)
        {
            if (h == null)
            {
                throw new ArgumentNullException("h");
            }

            var container = h.ViewContext.TempData[TempDataKey] as List<FlashMessage>;

            if (container != null)
            {
                var sb = new StringBuilder();
                sb.Append("<div>");

                foreach (var f in container.OrderByDescending(_ => _.Type))
                {
                    sb.Append(f);
                }

                sb.Append("</div>");

                container.Clear();
                return MvcHtmlString.Create(sb.ToString());
            }

            return MvcHtmlString.Empty;
        }
    }

    public struct FlashMessage
    {
        public string Message { get; set; }
        public FlashMessageType Type { get; set; }

        public static string FlashMessageCssClass(FlashMessageType t)
        {
            switch (t)
            {
                case FlashMessageType.Success:
                    return "alert alert-success";

                case FlashMessageType.Warning:
                    return "alert alert-warning";

                case FlashMessageType.Error:
                    return "alert alert-error";

                default:
                    return "alert";
            }
        }

        public override string ToString()
        {
            var tb = new TagBuilder("div");
            tb.AddCssClass(FlashMessageCssClass(Type));

            var btn = new TagBuilder("button")
            {
                Attributes = { { "type", "button" }, { "class", "close" }, { "data-dismiss", "alert" } },
                InnerHtml = "&times;"
            };

            tb.InnerHtml = btn + Message;

            return tb.ToString();
        }

        public IHtmlString ToHtmlString()
        {
            return MvcHtmlString.Create(ToString());
        }
    }

    public enum FlashMessageType
    {
        Info    = 1,
        Success = 2,
        Warning = 3,
        Error   = 4,
    }
}