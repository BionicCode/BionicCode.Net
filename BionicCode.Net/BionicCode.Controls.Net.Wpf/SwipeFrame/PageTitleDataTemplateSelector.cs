namespace BionicCode.Controls.Net.Wpf
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    internal class PageTitleDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextDataTemplate { get; set; }
        public DataTemplate ImageSourceDataTemplate { get; set; }
        public DataTemplate ObjectDataTemplate { get; set; }

        #region Overrides of TitleDataTemplateSelector

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case string _:
                    return this.TextDataTemplate;
                case ImageSource _:
                    return this.ImageSourceDataTemplate;
                default:
                    return this.ObjectDataTemplate;
            }
        }

        #endregion
    }
}
