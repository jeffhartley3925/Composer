using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Composer.Silverlight.UI.Web.Models;
using Composer.Messaging;
using System.Drawing;
using System.IO;
using System.Web.UI;

namespace Composer.Silverlight.UI.Web.Models
{
    [HandleError]
    public class HomeController : Controller
    {
        private Message message = new Message();

        public ActionResult Index(string id, string index)
        {
            ViewData["CompositionId"] = id;
            ViewData["CollaborationIndex"] = index;
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateFile(Message data)
        {
            var response = new Message();
            if (data == null)
            {
                response.Text = @"data == null";
            }
            else
            {
                var path = Server.MapPath("/composer/compositionfiles/");
                string fileType = "htm";
                var fileName = data.CompositionId.ToString() + "_" + data.CollaborationId.ToString() + "." + fileType;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(string.Format("{0}/{1}", path, fileName)))
                {
                    StringWriter stringWriter = new StringWriter();
                    // Put HtmlTextWriter in using block because it needs to call Dispose.
                    using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                    {

                        writer.RenderBeginTag(HtmlTextWriterTag.Html);

                            writer.AddAttribute("prefix", "og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# wecontrib: http://ogp.me/ns/fb/wecontrib#");

                            writer.RenderBeginTag(HtmlTextWriterTag.Head);

                                writer.AddAttribute("http-equiv", "X-UA-Compatible");
                                writer.AddAttribute("content", "chrome=1, IE=edge");
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); writer.RenderEndTag();

                                writer.AddAttribute("property", "fb:admins");
                                writer.AddAttribute("content", "675485908");
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); writer.RenderEndTag();

                                writer.AddAttribute("property", "fb:app_id");
                                writer.AddAttribute("content", "171096762940671");
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); writer.RenderEndTag();

                                writer.AddAttribute("property", "og:url");
                                writer.AddAttribute("content", @"https://www.wecontrib.com/composer/compositionfiles/" + data.CompositionId.ToString() + "_" + data.CollaborationId.ToString() + ".htm");
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); ; writer.RenderEndTag();

                                writer.AddAttribute("property", "og:description");
                                writer.AddAttribute("content", data.CompositionTitle);
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); writer.RenderEndTag();

                                writer.AddAttribute("property", "og:type");
                                writer.AddAttribute("content", "wecontrib:composition");
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); writer.RenderEndTag();

                                writer.AddAttribute("property", "og:title");
                                writer.AddAttribute("content", data.CompositionTitle);
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); writer.RenderEndTag();

                                writer.AddAttribute("property", "og:image");
                                writer.AddAttribute("content", @"https://www.wecontrib.com/composer/compositionimages/" + data.CompositionId.ToString() + "_" + data.CollaborationId.ToString() + ".bmp");
                                writer.RenderBeginTag(HtmlTextWriterTag.Meta); writer.RenderEndTag();

                                writer.WriteLine("<script type='text/javascript'>");
                                string url = "window.location = 'https://www.wecontrib.com/composer/deck/card?id=" + data.CompositionId.ToString() + "&index=" + data.CollaborationId.ToString() + "';";
                                writer.WriteLine(url);
                                writer.WriteLine("</script>");
                            writer.RenderEndTag();
                            writer.RenderBeginTag(HtmlTextWriterTag.Body);

                            //writer.AddAttribute("src", "https://www.wecontrib.com/composer/compositionimages/" + data.CompositionId.ToString() + "_" + data.CollaborationId.ToString() + ".bmp");
                            //writer.RenderBeginTag(HtmlTextWriterTag.Img);
                            //writer.RenderEndTag();

                            writer.RenderEndTag();

                        writer.RenderEndTag();
                        file.WriteLine(stringWriter.ToString());
                    }
                }
            }
            return Json(response, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public JsonResult PushMessage(Message data)
        {
            var response = new Message();
            if (data == null)
            {
                response.Text = @"data == null";
            }
            else if (data.Text == null)
            {
                response.Text = @"data.Text == null";
            }
            else
            {
                var base64 = Composer.Messaging.Compression.Decompress(data.Text);
                var bitmapData = Convert.FromBase64String((base64));
                using (System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData))
                {
                    using (Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap)))
                    {
                        var path = Server.MapPath("/composer/compositionimages/");
                        string fileType = "bmp";
                        var fileName = data.CompositionId.ToString() + "_" + data.CollaborationId.ToString() + "." + fileType;
                        bitImage.Save(string.Format(@"{0}\{1}", path, fileName));
                        response.Text = string.Format("{0}", "Success");
                    }
                }
            }
            return Json(response, JsonRequestBehavior.DenyGet);
        }
    }
}