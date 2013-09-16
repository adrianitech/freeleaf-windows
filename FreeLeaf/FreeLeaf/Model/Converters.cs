using System.Windows;
using System.Windows.Controls;

namespace FreeLeaf.Model
{
    public class DiscoveryListStyleSelector : StyleSelector
    {
        public Style ItemStyle { get; set; }
        public Style ButtonStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            return ((DeviceItem)item).ID == null ? ButtonStyle : ItemStyle;
        }
    }
}
