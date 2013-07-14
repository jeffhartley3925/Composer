using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;

namespace Composer.Modules.Palettes.Services
{
    public class PaletteItems
    {
        public string PaletteClass { get; set; }
        public PaletteItems(string paletteClass)
        {
            PaletteClass = paletteClass;
        }
        public List<PaletteItem> Parse()
        {
            var paletteItems = new List<PaletteItem>();
            StreamResourceInfo streamResourceInfo =
                Application.GetResourceStream(new Uri(string.Format("Composer.Silverlight.UI;component/{0}", "PaletteDefinitions/" + PaletteClass.Trim() + ".xml"), UriKind.Relative));

            if (streamResourceInfo != null)
            {
                if (streamResourceInfo.Stream != null)
                {
                    var stream = streamResourceInfo.Stream;
                    var xmlReader = XmlReader.Create(stream);
                    var xDocument = XDocument.Load(xmlReader);
                    var container = xDocument.Descendants("Palette");
                    foreach (var l1Node in container)
                    {
                        var node = l1Node;
                        var paletteId = (string)node.Attribute("Id");
                        var paletteCaption = (string)node.Attribute("Caption");

                        var items = xDocument.Descendants("Button");
                        foreach (var l2Node in items)
                        {
                            var element = l2Node;
                            if (element != null)
                            {
                                var target = (string)element.Attribute("Target");
                                var enabled = (string)element.Attribute("Enabled");
                                var groupName = (string)element.Attribute("GroupName");
                                var caption = (string)element.Attribute("Caption");
                                var tooltip = (string)element.Attribute("Tooltip");
                                var paletteItem = new PaletteItem(enabled, target, groupName, caption, tooltip, paletteId, paletteCaption);
                                paletteItems.Add(paletteItem);
                            }
                        }
                    }
                }
            }
            return paletteItems;
        }
    }
}
