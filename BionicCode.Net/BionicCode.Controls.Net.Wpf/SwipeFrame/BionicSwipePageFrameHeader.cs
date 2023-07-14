namespace BionicCode.Controls.Net.Wpf
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;

  [TemplatePart(Name = "PART_Title", Type = typeof(ContentControl))]
  [TemplatePart(Name = "PART_PageHeaderTitleHostPanel", Type = typeof(Panel))]
  public class BionicSwipePageFrameHeader : Control
  {
    public static System.Windows.ResourceKey ButtonStyleKey { get; }
    
    [System.ComponentModel.TypeConverter(typeof(System.Windows.LengthConverter))]
    public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(
      "IconHeight",
      typeof(double),
      typeof(BionicSwipePageFrameHeader),
      new PropertyMetadata(double.NaN));

    [System.ComponentModel.TypeConverter(typeof(System.Windows.LengthConverter))]

    public double IconHeight { get { return (double) GetValue(BionicSwipePageFrameHeader.IconHeightProperty); } set { SetValue(BionicSwipePageFrameHeader.IconHeightProperty, value); } }

    public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(
      "IconWidth",
      typeof(double),
      typeof(BionicSwipePageFrameHeader),
      new PropertyMetadata(double.NaN));

    [System.ComponentModel.TypeConverter(typeof(System.Windows.LengthConverter))]
    public double IconWidth { get { return (double) GetValue(BionicSwipePageFrameHeader.IconWidthProperty); } set { SetValue(BionicSwipePageFrameHeader.IconWidthProperty, value); } }
    
    public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
      "TitleTemplate",
      typeof(DataTemplate),
      typeof(BionicSwipePageFrameHeader),
      new PropertyMetadata(default(DataTemplate), BionicSwipePageFrameHeader.OnTitleTemplateChanged));

    public DataTemplate TitleTemplate
    {
      get => (DataTemplate)GetValue(BionicSwipePageFrameHeader.TitleTemplateProperty);
      set => SetValue(BionicSwipePageFrameHeader.TitleTemplateProperty, value);
    }

    public static readonly DependencyProperty TitleDataTemplateSelectorProperty = DependencyProperty.Register(
      "TitleDataTemplateSelector",
      typeof(DataTemplateSelector),
      typeof(BionicSwipePageFrameHeader),
      new PropertyMetadata(default(DataTemplateSelector), BionicSwipePageFrameHeader.OnTitleDataTemplateSelectorChanged));

    public DataTemplateSelector TitleDataTemplateSelector
    {
      get => (DataTemplateSelector)GetValue(BionicSwipePageFrameHeader.TitleDataTemplateSelectorProperty);
      set => SetValue(BionicSwipePageFrameHeader.TitleDataTemplateSelectorProperty, value);
    }

    static BionicSwipePageFrameHeader()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BionicSwipePageFrameHeader), new FrameworkPropertyMetadata(typeof(BionicSwipePageFrameHeader)));

      BionicSwipePageFrameHeader.ButtonStyleKey = new ComponentResourceKey(typeof(BionicSwipePageFrameHeader), "ButtonStyle");
    }

    public BionicSwipePageFrameHeader()
    {
      this.Loaded += InitializeParts;
    }

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.PART_Title = GetTemplateChild("PART_Title") as ContentControl;
      this.PART_PageHeaderHostPanel = GetTemplateChild("PART_PageHeaderTitleHostPanel") as Panel;
    }

    #endregion

    private static void OnTitleDataTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrameHeader);
      if (_this.PART_Title != null)
      {
        _this.PART_Title.ContentTemplateSelector = e.NewValue as DataTemplateSelector;
      }
    }

    private static void OnTitleTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var _this = (d as BionicSwipePageFrameHeader);
      if (_this.PART_Title != null)
      {
        _this.PART_Title.ContentTemplate = e.NewValue as DataTemplate;
      }
    }

    private void InitializeParts(object sender, RoutedEventArgs routedEventArgs)
    {
      this.PART_Title.RenderTransform = new TranslateTransform();
      if (this.PART_Title.ContentTemplateSelector == null)
      {
        this.PART_Title.ContentTemplateSelector = this.TitleDataTemplateSelector;
      }
      if (this.PART_Title.ContentTemplate == null && this.TitleTemplate != null)
      {
        this.PART_Title.ContentTemplate = this.TitleTemplate;
      }
    }

    internal Panel PART_PageHeaderHostPanel { get; set; }

    internal ContentControl PART_Title { get; set; }
  }
}
