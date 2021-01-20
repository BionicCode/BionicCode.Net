#region Info

// 2021/01/09  15:11
// BionicCode.Controls.Net.Core.Wpf

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BionicCode.Utilities.Net.Wpf.Extensions;

namespace BionicCode.Controls.Net.Wpf
{
  public class AnalogClockFace : ContentControl
  {
    public static ComponentResourceKey DefaultAnalogClockFaceStyleKey { get; } = new()
    {
      TypeInTargetAssembly = typeof(AnalogClockFace),
      ResourceId = "AnalogClockFaceStyle"
    };

    public static ComponentResourceKey DefaultAnalogClockFaceTimePickerStyleKey { get; } = new()
    {
      TypeInTargetAssembly = typeof(AnalogClockFace),
      ResourceId = "AnalogClockFaceTimePickerStyle"
    };

    #region SelectedTimeChangedRoutedEvent

    public static readonly RoutedEvent SelectedTimeChangedRoutedEvent = EventManager.RegisterRoutedEvent("SelectedTimeChanged",
      RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AnalogClockFace));

    public event RoutedEventHandler SelectedTimeChanged
    {
      add { AddHandler(AnalogClockFace.SelectedTimeChangedRoutedEvent, value); }
      remove { RemoveHandler(AnalogClockFace.SelectedTimeChangedRoutedEvent, value); }
    }

    #endregion

    #region IsCenterElementOnCircumferenceEnabled attached property

    public static readonly DependencyProperty IsCenterElementOnCircumferenceEnabledProperty = DependencyProperty.RegisterAttached(
      "IsCenterElementOnCircumferenceEnabled", 
      typeof(bool), 
      typeof(AnalogClockFace), 
      new PropertyMetadata(true));

    public static void SetIsCenterElementOnCircumferenceEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(AnalogClockFace.IsCenterElementOnCircumferenceEnabledProperty, value);

    public static bool GetIsCenterElementOnCircumferenceEnabled(DependencyObject attachingElement) => (bool) attachingElement.GetValue(AnalogClockFace.IsCenterElementOnCircumferenceEnabledProperty);


    #endregion IsCenterElementOnCircumferenceEnabled attached property
    #region Is15MinuteIntervalEnabled dependency property

    public static readonly DependencyProperty Is15MinuteIntervalEnabledProperty = DependencyProperty.Register(
      "Is15MinuteIntervalEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(true));

    public bool Is15MinuteIntervalEnabled { get => (bool) GetValue(AnalogClockFace.Is15MinuteIntervalEnabledProperty); set => SetValue(AnalogClockFace.Is15MinuteIntervalEnabledProperty, value); }

    #endregion Is15MinuteIntervalEnabled dependency property

    #region Is5MinuteIntervalEnabled dependency property

    public static readonly DependencyProperty Is5MinuteIntervalEnabledProperty = DependencyProperty.Register(
      "Is5MinuteIntervalEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(true));

    public bool Is5MinuteIntervalEnabled { get => (bool) GetValue(AnalogClockFace.Is5MinuteIntervalEnabledProperty); set => SetValue(AnalogClockFace.Is5MinuteIntervalEnabledProperty, value); }

    #endregion Is5MinuteIntervalEnabled dependency property

    #region IsShowingBig12Interval dependency property

    public static readonly DependencyProperty IsShowingBig12IntervalProperty = DependencyProperty.Register(
      "IsShowingBig12Interval",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public bool IsShowingBig12Interval { get => (bool) GetValue(AnalogClockFace.IsShowingBig12IntervalProperty); set => SetValue(AnalogClockFace.IsShowingBig12IntervalProperty, value); }

    #endregion IsShowingBig12Interval dependency property

    #region FifteenMinuteIntervalElement dependency property

    public static readonly DependencyProperty FifteenMinuteIntervalElementProperty = DependencyProperty.Register(
      "FifteenMinuteIntervalElement",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public FrameworkElement FifteenMinuteIntervalElement { get => (FrameworkElement) GetValue(AnalogClockFace.FifteenMinuteIntervalElementProperty); set => SetValue(AnalogClockFace.FifteenMinuteIntervalElementProperty, value); }

    #endregion FifteenMinuteIntervalElement dependency property

    #region FiveMinuteIntervalElement dependency property

    public static readonly DependencyProperty FiveMinuteIntervalElementProperty = DependencyProperty.Register(
      "FiveMinuteIntervalElement",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public FrameworkElement FiveMinuteIntervalElement { get => (FrameworkElement) GetValue(AnalogClockFace.FiveMinuteIntervalElementProperty); set => SetValue(AnalogClockFace.FiveMinuteIntervalElementProperty, value); }

    #endregion FiveMinuteIntervalElement dependency property

    #region MinuteIntervalElement dependency property

    public static readonly DependencyProperty MinuteIntervalElementProperty = DependencyProperty.Register(
      "MinuteIntervalElement",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public FrameworkElement MinuteIntervalElement { get => (FrameworkElement) GetValue(AnalogClockFace.MinuteIntervalElementProperty); set => SetValue(AnalogClockFace.MinuteIntervalElementProperty, value); }

    #endregion MinuteIntervalElement dependency property

    #region FifteenMinuteIntervalLabel dependency property

    public static readonly DependencyProperty FifteenMinuteIntervalLabelProperty = DependencyProperty.Register(
      "FifteenMinuteIntervalLabel",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public FrameworkElement FifteenMinuteIntervalLabel { get => (FrameworkElement) GetValue(AnalogClockFace.FifteenMinuteIntervalLabelProperty); set => SetValue(AnalogClockFace.FifteenMinuteIntervalLabelProperty, value); }

    #endregion FifteenMinuteIntervalLabel dependency property

    #region FiveMinuteIntervalLabel dependency property

    public static readonly DependencyProperty FiveMinuteIntervalLabelProperty = DependencyProperty.Register(
      "FiveMinuteIntervalLabel",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public FrameworkElement FiveMinuteIntervalLabel { get => (FrameworkElement) GetValue(AnalogClockFace.FiveMinuteIntervalLabelProperty); set => SetValue(AnalogClockFace.FiveMinuteIntervalLabelProperty, value); }

    #endregion FiveMinuteIntervalLabel dependency property

    #region MinuteIntervalLabel dependency property

 public static readonly DependencyProperty MinuteIntervalLabelProperty = DependencyProperty.Register(
   "MinuteIntervalLabel",
   typeof(FrameworkElement),
   typeof(AnalogClockFace),
   new FrameworkPropertyMetadata(
     default(FrameworkElement),
     FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public FrameworkElement MinuteIntervalLabel { get => (FrameworkElement) GetValue(AnalogClockFace.MinuteIntervalLabelProperty); set => SetValue(AnalogClockFace.MinuteIntervalLabelProperty, value); }

    #endregion MinuteIntervalLabel dependency property

    #region IsDisplayIntervalLabelsEnabled dependency property

    public static readonly DependencyProperty IsDisplayIntervalLabelsEnabledProperty = DependencyProperty.Register(
      "IsDisplayIntervalLabelsEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(true));

    public bool IsDisplayIntervalLabelsEnabled { get => (bool) GetValue(AnalogClockFace.IsDisplayIntervalLabelsEnabledProperty); set => SetValue(AnalogClockFace.IsDisplayIntervalLabelsEnabledProperty, value); }

    #endregion IsDisplayIntervalLabelsEnabled dependency property

    #region IntervalLabelFormatter dependency property

    public static readonly DependencyProperty IntervalLabelFormatterProperty = DependencyProperty.Register(
      "IntervalLabelFormatter",
      typeof(Func<int, object>),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public Func<int, object> IntervalLabelFormatter { get => (Func<int, object>) GetValue(AnalogClockFace.IntervalLabelFormatterProperty); set => SetValue(AnalogClockFace.IntervalLabelFormatterProperty, value); }

    #endregion IntervalLabelFormatter dependency property

    #region HourHandElement dependency property

    public static readonly DependencyProperty HourHandElementProperty = DependencyProperty.Register(
      "HourHandElement",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(FrameworkElement), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, AnalogClockFace.OnHourHandElementChanged));

    public FrameworkElement HourHandElement { get => (FrameworkElement) GetValue(AnalogClockFace.HourHandElementProperty); set => SetValue(AnalogClockFace.HourHandElementProperty, value); }

    #endregion HourHandElement dependency property

    #region MinuteHandElement dependency property

    public static readonly DependencyProperty MinuteHandElementProperty = DependencyProperty.Register(
      "MinuteHandElement",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(FrameworkElement), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, AnalogClockFace.OnMinuteHandElementChanged));

    public FrameworkElement MinuteHandElement { get => (FrameworkElement) GetValue(AnalogClockFace.MinuteHandElementProperty); set => SetValue(AnalogClockFace.MinuteHandElementProperty, value); }

    #endregion MinuteHandElement dependency property

    #region SecondHandElement dependency property

    public static readonly DependencyProperty SecondHandElementProperty = DependencyProperty.Register(
      "SecondHandElement",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(FrameworkElement), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, AnalogClockFace.OnSecondHandElementChanged));

    public FrameworkElement SecondHandElement { get => (FrameworkElement) GetValue(AnalogClockFace.SecondHandElementProperty); set => SetValue(AnalogClockFace.SecondHandElementProperty, value); }

    #endregion SecondHandElement dependency property

    #region DateElement dependency property

    public static readonly DependencyProperty DateElementProperty = DependencyProperty.Register(
      "DateElement",
      typeof(FrameworkElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(FrameworkElement), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, AnalogClockFace.OnDateElementChanged));

    public FrameworkElement DateElement { get => (FrameworkElement)GetValue(AnalogClockFace.DateElementProperty); set => SetValue(AnalogClockFace.DateElementProperty, value); }

    #endregion DateElement dependency property

    #region SelectedHour dependency property

    public static readonly DependencyProperty SelectedHourProperty = DependencyProperty.Register(
      "SelectedHour",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnSelectedHourChanged, AnalogClockFace.CoerceHours));

    public double SelectedHour { get => (double) GetValue(AnalogClockFace.SelectedHourProperty); set => SetValue(AnalogClockFace.SelectedHourProperty, value); }

    #endregion SelectedHour dependency property

    #region SelectedMinute dependency property

    public static readonly DependencyProperty SelectedMinuteProperty = DependencyProperty.Register(
      "SelectedMinute",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnSelectedMinuteChanged, AnalogClockFace.CoerceMinutes));

    public double SelectedMinute { get => (double) GetValue(AnalogClockFace.SelectedMinuteProperty); set => SetValue(AnalogClockFace.SelectedMinuteProperty, value); }

    #endregion SelectedMinute dependency property

    #region SelectedSecond dependency property

    public static readonly DependencyProperty SelectedSecondProperty = DependencyProperty.Register(
      "SelectedSecond",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnSelectedSecondChanged, AnalogClockFace.CoerceSeconds));

    public double SelectedSecond { get => (double) GetValue(AnalogClockFace.SelectedSecondProperty); set => SetValue(AnalogClockFace.SelectedSecondProperty, value); }

    #endregion SelectedSecond dependency property

    #region SelectedTime dependency property

    public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
      "SelectedTime",
      typeof(DateTime),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AnalogClockFace.OnSelectedTimeChanged));

    public DateTime SelectedTime { get => (DateTime) GetValue(AnalogClockFace.SelectedTimeProperty); set => SetValue(AnalogClockFace.SelectedTimeProperty, value); }

    #endregion SelectedTime dependency property

    #region MinuteSelectionArcBrush dependency property

    public static readonly DependencyProperty MinuteSelectionArcBrushProperty = DependencyProperty.Register(
      "MinuteSelectionArcBrush",
      typeof(Brush),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public Brush MinuteSelectionArcBrush { get => (Brush) GetValue(AnalogClockFace.MinuteSelectionArcBrushProperty); set => SetValue(AnalogClockFace.MinuteSelectionArcBrushProperty, value); }

    #endregion MinuteSelectionArcBrush dependency property

    #region TimePickerHourSelectionArcBrush dependency property

    public static readonly DependencyProperty TimePickerHourSelectionArcBrushProperty = DependencyProperty.Register(
      "TimePickerHourSelectionArcBrush",
      typeof(Brush),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public Brush TimePickerHourSelectionArcBrush { get => (Brush) GetValue(AnalogClockFace.TimePickerHourSelectionArcBrushProperty); set => SetValue(AnalogClockFace.TimePickerHourSelectionArcBrushProperty, value); }

    #endregion cHourSelectionArcBrush dependency property

    #region TimePickerMinuteSelectionArcBrush dependency property

    public static readonly DependencyProperty TimePickerMinuteSelectionArcBrushProperty = DependencyProperty.Register(
      "TimePickerMinuteSelectionArcBrush",
      typeof(Brush),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public Brush TimePickerMinuteSelectionArcBrush { get => (Brush) GetValue(AnalogClockFace.TimePickerMinuteSelectionArcBrushProperty); set => SetValue(AnalogClockFace.TimePickerMinuteSelectionArcBrushProperty, value); }

    #endregion TimePickerMinuteSelectionArcBrush dependency property

    #region TimePickerSecondSelectionArcBrush dependency property

    public static readonly DependencyProperty TimePickerSecondSelectionArcBrushProperty = DependencyProperty.Register(
      "TimePickerSecondSelectionArcBrush",
      typeof(Brush),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public Brush TimePickerSecondSelectionArcBrush { get => (Brush) GetValue(AnalogClockFace.TimePickerSecondSelectionArcBrushProperty); set => SetValue(AnalogClockFace.TimePickerSecondSelectionArcBrushProperty, value); }

    #endregion TimePickerSecondSelectionArcBrush dependency property

    #region IntervalLabelRadiusOffset dependency property

    public static readonly DependencyProperty IntervalLabelRadiusOffsetProperty = DependencyProperty.Register(
      "IntervalLabelRadiusOffset",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public double IntervalLabelRadiusOffset { get => (double) GetValue(AnalogClockFace.IntervalLabelRadiusOffsetProperty); set => SetValue(AnalogClockFace.IntervalLabelRadiusOffsetProperty, value); }

    #endregion IntervalLabelRadiusOffset dependency property
    
    #region ClockFaceLoadedRoutedEvent

    public static readonly RoutedEvent ClockFaceLoadedRoutedEvent = EventManager.RegisterRoutedEvent("ClockFaceLoaded",
      RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AnalogClockFace));

    public event RoutedEventHandler ClockFaceLoaded
    {
      add => AddHandler(AnalogClockFace.ClockFaceLoadedRoutedEvent, value);
      remove => RemoveHandler(AnalogClockFace.ClockFaceLoadedRoutedEvent, value);
    }

    #endregion

    #region Diameter dependecy property

    protected static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
      "Diameter",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnDiameterChanged));

    public double Diameter
    {
      get => (double)GetValue(AnalogClockFace.DiameterProperty);
      set => SetValue(AnalogClockFace.DiameterProperty, value);
    }

    #endregion Diameter dependecy property

    #region Radius read-only dependecy property

    protected static readonly DependencyPropertyKey RadiusPropertyKey = DependencyProperty.RegisterReadOnly(
      "Radius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty RadiusProperty = AnalogClockFace.RadiusPropertyKey.DependencyProperty;

    public double Radius
    {
      get => (double)GetValue(AnalogClockFace.RadiusProperty);
      private set => SetValue(AnalogClockFace.RadiusPropertyKey, value);
    }

    #endregion Radius read-only dependecy property

    #region Center read-only dependency property
    protected static readonly DependencyPropertyKey CenterPropertyKey = DependencyProperty.RegisterReadOnly(
      "Center",
      typeof(Point),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(Point)));

    public static readonly DependencyProperty CenterProperty = AnalogClockFace.CenterPropertyKey.DependencyProperty;

    public Point Center
    {
      get => (Point) GetValue(AnalogClockFace.CenterProperty);
      private set => SetValue(AnalogClockFace.CenterPropertyKey, value);
    }

    #endregion Center read-only dependency property

    #region IntervalMarkerCenterRadius read-only dependency property
    protected static readonly DependencyPropertyKey IntervalMarkerCenterRadiusPropertyKey = DependencyProperty.RegisterReadOnly(
      "IntervalMarkerCenterRadius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty IntervalMarkerCenterRadiusProperty = AnalogClockFace.IntervalMarkerCenterRadiusPropertyKey.DependencyProperty;

    public double IntervalMarkerCenterRadius
    {
      get => (double) GetValue(AnalogClockFace.IntervalMarkerCenterRadiusProperty);
      private set => SetValue(AnalogClockFace.IntervalMarkerCenterRadiusPropertyKey, value);
    }

    #endregion IntervalMarkerCenterRadius read-only dependency property

    #region IntervalMarkerMinuteCenterRadius read-only dependency property
    protected static readonly DependencyPropertyKey IntervalMarkerInnerCenterRadiusPropertyKey = DependencyProperty.RegisterReadOnly(
      "IntervalMarkerMinuteCenterRadius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty IntervalMarkerMinuteCenterRadiusProperty = AnalogClockFace.IntervalMarkerInnerCenterRadiusPropertyKey.DependencyProperty;

    public double IntervalMarkerMinuteCenterRadius
    {
      get => (double) GetValue(AnalogClockFace.IntervalMarkerMinuteCenterRadiusProperty);
      private set => SetValue(AnalogClockFace.IntervalMarkerInnerCenterRadiusPropertyKey, value);
    }

    #endregion IntervalMarkerMinuteCenterRadius read-only dependency property

    #region IntervalMarkerSecondCenterRadius read-only dependency property
    protected static readonly DependencyPropertyKey IntervalMarkerSecondCenterRadiusPropertyKey = DependencyProperty.RegisterReadOnly(
      "IntervalMarkerSecondCenterRadius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty IntervalMarkerSecondCenterRadiusProperty = AnalogClockFace.IntervalMarkerSecondCenterRadiusPropertyKey.DependencyProperty;

    public double IntervalMarkerSecondCenterRadius
    {
      get => (double) GetValue(AnalogClockFace.IntervalMarkerSecondCenterRadiusProperty);
      private set => SetValue(AnalogClockFace.IntervalMarkerSecondCenterRadiusPropertyKey, value);
    }

    #endregion IntervalMarkerSecondCenterRadius read-only dependency property

    #region HourHandRadius dependency property

    public static readonly DependencyProperty HourHandRadiusProperty = DependencyProperty.Register(
      "HourHandRadius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnHourHandRadiusChanged));

    public double HourHandRadius { get => (double) GetValue(AnalogClockFace.HourHandRadiusProperty); set => SetValue(AnalogClockFace.HourHandRadiusProperty, value); }

    #endregion HourHandRadius dependency property

    #region MinuteHandRadius dependency property

    public static readonly DependencyProperty MinuteHandRadiusProperty = DependencyProperty.Register(
      "MinuteHandRadius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnMinuteHandRadiusChanged));

    public double MinuteHandRadius { get => (double) GetValue(AnalogClockFace.MinuteHandRadiusProperty); set => SetValue(AnalogClockFace.MinuteHandRadiusProperty, value); }

    #endregion MinuteHandRadius dependency property

    #region SecondHandRadius dependency property

    public static readonly DependencyProperty SecondHandRadiusProperty = DependencyProperty.Register(
      "SecondHandRadius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnSecondHandRadiusChanged));

    public double SecondHandRadius { get => (double) GetValue(AnalogClockFace.SecondHandRadiusProperty); set => SetValue(AnalogClockFace.SecondHandRadiusProperty, value); }

    #endregion SecondHandRadius dependency property

    #region HourHandDiameter read-only dependency property
    protected static readonly DependencyPropertyKey HourHandDiameterPropertyKey = DependencyProperty.RegisterReadOnly(
      "HourHandDiameter",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty HourHandDiameterProperty = AnalogClockFace.HourHandDiameterPropertyKey.DependencyProperty;

    public double HourHandDiameter
    {
      get => (double) GetValue(AnalogClockFace.HourHandDiameterProperty);
      private set => SetValue(AnalogClockFace.HourHandDiameterPropertyKey, value);
    }
    #endregion HourHandDiameter read-only dependency property

    #region MinuteHandDiameter read-only dependency property
    protected static readonly DependencyPropertyKey MinuteHandDiameterPropertyKey = DependencyProperty.RegisterReadOnly(
      "MinuteHandDiameter",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty MinuteHandDiameterProperty = AnalogClockFace.MinuteHandDiameterPropertyKey.DependencyProperty;

    public double MinuteHandDiameter
    {
      get => (double) GetValue(AnalogClockFace.MinuteHandDiameterProperty);
      private set => SetValue(AnalogClockFace.MinuteHandDiameterPropertyKey, value);
    }

    #endregion MinuteHandDiameter read-only dependency property

    #region SecondHandDiameter read-only dependency property
    protected static readonly DependencyPropertyKey SecondHandDiameterPropertyKey = DependencyProperty.RegisterReadOnly(
      "SecondHandDiameter",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty SecondHandDiameterProperty = AnalogClockFace.SecondHandDiameterPropertyKey.DependencyProperty;

    public double SecondHandDiameter
    {
      get => (double) GetValue(AnalogClockFace.SecondHandDiameterProperty);
      private set => SetValue(AnalogClockFace.SecondHandDiameterPropertyKey, value);
    }

    #endregion SecondHandDiameter read-only dependency property

    #region Is24HModeEnabled dependency property

    public static readonly DependencyProperty Is24HModeEnabledProperty = DependencyProperty.Register(
      "Is24HModeEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public bool Is24HModeEnabled { get => (bool) GetValue(AnalogClockFace.Is24HModeEnabledProperty); set => SetValue(AnalogClockFace.Is24HModeEnabledProperty, value); }

    #endregion Is24HModeEnabled dependency property

    #region IsDisplayDateEnabled dependency property

    public static readonly DependencyProperty IsDisplayDateEnabledProperty = DependencyProperty.Register(
      "IsDisplayDateEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public bool IsDisplayDateEnabled { get => (bool)GetValue(AnalogClockFace.IsDisplayDateEnabledProperty); set => SetValue(AnalogClockFace.IsDisplayDateEnabledProperty, value); }

    #endregion IsDisplayDateEnabled dependency property

    #region IsMinuteSelectionArcEnabled dependency property

    public static readonly DependencyProperty IsMinuteSelectionArcEnabledProperty = DependencyProperty.Register(
      "IsMinuteSelectionArcEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public bool IsMinuteSelectionArcEnabled { get => (bool) GetValue(AnalogClockFace.IsMinuteSelectionArcEnabledProperty); set => SetValue(AnalogClockFace.IsMinuteSelectionArcEnabledProperty, value); }

    #endregion IsMinuteSelectionArcEnabled dependency property

    #region IsTimePickerModeEnabled dependency property

    public static readonly DependencyProperty IsTimePickerModeEnabledProperty = DependencyProperty.Register(
      "IsTimePickerModeEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(bool), AnalogClockFace.OnIsTimePickerModeEnabledChanged));

    public bool IsTimePickerModeEnabled { get => (bool)GetValue(AnalogClockFace.IsTimePickerModeEnabledProperty); set => SetValue(AnalogClockFace.IsTimePickerModeEnabledProperty, value); }

    #endregion IsTimePickerModeEnabled dependency property

    #region IsTimePickerClockHandVisible dependency property

    public static readonly DependencyProperty IsTimePickerClockHandVisibleProperty = DependencyProperty.Register(
      "IsTimePickerClockHandVisible",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public bool IsTimePickerClockHandVisible { get => (bool)GetValue(AnalogClockFace.IsTimePickerClockHandVisibleProperty); set => SetValue(AnalogClockFace.IsTimePickerClockHandVisibleProperty, value); }

    #endregion IsTimePickerClockHandVisible dependency property

    private Canvas ClockFaceCanvas { get; set; }
    private RotateTransform HourHandTransform { get; set; }
    private RotateTransform MinuteHandTransform { get; set; }
    private RotateTransform SecondHandTransform { get; set; }
    private RotateTransform DateElementTransform { get; set; }
    private RotateTransform IntervalElementTransform { get; set; }
    private ScaleTransform ClockFaceScaleTransform { get; set; }
    private Path SelectedHourArc { get; set; }
    private Path SelectedMinuteArc { get; set; }
    private Path SelectedSecondArc { get; set; }
    private PathGeometry SelectedHourArcBounds { get; set; }
    private PathGeometry SelectedMinuteArcBounds { get; set; }
    private PathGeometry SelectedSecondArcBounds { get; set; }
    private bool IsUpdatingSelectedTimeComponent { get; set; }
    private bool IsUpdatingSelectedTime { get; set; }

    static AnalogClockFace()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AnalogClockFace), new FrameworkPropertyMetadata(typeof(AnalogClockFace)));
    }
    public AnalogClockFace()
    {
      this.Diameter = 100;
      this.SelectedHourArc = new Path();
      this.SelectedMinuteArc = new Path();
      this.SelectedSecondArc = new Path();
      InitializeClockRotateTransforms();
      InitializeClockFaceCanvas();

      this.IntervalLabelFormatter = value => value.ToString();
    }
    
    private void InitializeClockFaceCanvas()
    {
      this.ClockFaceScaleTransform = new ScaleTransform(1, 1);
      this.ClockFaceCanvas = new Canvas
      {
        RenderTransform = this.ClockFaceScaleTransform,
        Background = Brushes.Transparent,
        Width = this.Diameter,
        Height = this.Diameter,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top
      };
      this.Content = this.ClockFaceCanvas;

      var widthBinding = new Binding(nameof(this.Diameter)) {Source = this};
      this.ClockFaceCanvas.SetBinding(FrameworkElement.WidthProperty, widthBinding);
      var heightBinding = new Binding(nameof(this.Diameter)) {Source = this};
      this.ClockFaceCanvas.SetBinding(FrameworkElement.HeightProperty, heightBinding);
    }

    private void InitializeClockRotateTransforms()
    {
      this.IntervalElementTransform = new RotateTransform(0, this.Radius, this.Radius);
      this.HourHandTransform = new RotateTransform(GetClockAngle(this.SelectedHour, this.Is24HModeEnabled ? 24 : 12), this.Radius, this.Radius);
      this.MinuteHandTransform = new RotateTransform(GetClockAngle(this.SelectedMinute, 60), this.Radius, this.Radius);
      this.SecondHandTransform = new RotateTransform(GetClockAngle(this.SelectedSecond, 60), this.Radius, this.Radius);
      this.DateElementTransform = new RotateTransform(0, this.Radius, this.Radius);
      var radiusBinding = new Binding(nameof(this.Radius)) { Source = this };
      BindingOperations.SetBinding(this.IntervalElementTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.IntervalElementTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.HourHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.HourHandTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.MinuteHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.MinuteHandTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.SecondHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.SecondHandTransform, RotateTransform.CenterYProperty, radiusBinding);
    }

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);
      if (this.IsTimePickerModeEnabled)
      {
        this.Style = FindResource(AnalogClockFace.DefaultAnalogClockFaceTimePickerStyleKey) as Style;
      }
      else
      {
        this.Style = FindResource(AnalogClockFace.DefaultAnalogClockFaceStyleKey) as Style;
      }
    }

    #endregion

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      double scaleFactor = Math.Min(constraint.Width, constraint.Height) / this.Diameter;
      double requiredWidth = this.Diameter * scaleFactor;
      double requiredHeight = this.Diameter * scaleFactor;
      constraint = new Size(double.IsInfinity(requiredWidth) ? this.Diameter : requiredWidth, double.IsInfinity(requiredHeight) ? this.Diameter : requiredHeight);
      base.MeasureOverride(constraint);
      return constraint;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      if (this.Is24HModeEnabled || this.IsTimePickerModeEnabled)
      {
        DrawAnalog24Clock();
      }
      else
      {
        DrawAnalogClock();
      }
      Draw24HTimePickerSelectionBounds();
      double scaleFactor = Math.Min(arrangeBounds.Width, arrangeBounds.Height) / this.Diameter;
      this.ClockFaceScaleTransform.ScaleX = scaleFactor;
      this.ClockFaceScaleTransform.ScaleY = scaleFactor;
      arrangeBounds = base.ArrangeOverride(arrangeBounds);
      return arrangeBounds;
    }

    #endregion

    #region Overrides of UIElement

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseLeftButtonDown(e);
      if (!this.IsTimePickerModeEnabled)
      {
        return;
      }
      Point screenPoint = e.GetPosition(this.ClockFaceCanvas);
      var clockAngle = GetAngleFromCartesianPoint(screenPoint);
      this.IsUpdatingSelectedTimeComponent = true;
      double degreeOfStep;

      switch (screenPoint)
      {
        case { } when this.SelectedHourArcBounds.FillContains(screenPoint):
          double steps = this.Is24HModeEnabled ? 24 : 12;
          degreeOfStep = 360 / steps;
          double hourValue = Math.Round(clockAngle / degreeOfStep, MidpointRounding.AwayFromZero) % steps;
          this.SelectedHour = hourValue;
          break;
        case { } when this.SelectedMinuteArcBounds.FillContains(screenPoint):
          degreeOfStep = 360 / 60.0;
          double minuteValue = Math.Round(clockAngle / degreeOfStep, MidpointRounding.AwayFromZero) % 60;
          this.SelectedMinute = minuteValue;
          break;
        case { } when this.SelectedSecondArcBounds.FillContains(screenPoint):
          degreeOfStep = 360 / 60.0;
          double secondValue = Math.Round(clockAngle / degreeOfStep, MidpointRounding.AwayFromZero) % 60;
          this.SelectedSecond = secondValue;
          break;
      }
      this.IsUpdatingSelectedTimeComponent = false;
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
      base.OnPreviewMouseWheel(e);
      if (!this.IsTimePickerModeEnabled)
      {
        return;
      }
      Point screenPoint = e.GetPosition(this.ClockFaceCanvas);
      this.IsUpdatingSelectedTimeComponent = true;
      int change = e.Delta > 0 ? -1 : 1;

      switch (screenPoint)
      {
        case { } when this.SelectedHourArcBounds.FillContains(screenPoint):
          double steps = this.Is24HModeEnabled ? 24 : 12;
          double hourValue = (steps + this.SelectedHour + change) % steps;
          this.SelectedHour = hourValue;
          break;
        case { } when this.SelectedMinuteArcBounds.FillContains(screenPoint):
          double minuteValue = (60.0 + this.SelectedMinute + change) % 60;
          this.SelectedMinute = minuteValue;
          break;
        case { } when this.SelectedSecondArcBounds.FillContains(screenPoint):
          double secondValue = (60.0 + this.SelectedSecond + change) % 60;
          this.SelectedSecond = secondValue;
          break;
      }
      this.IsUpdatingSelectedTimeComponent = false;
    }

    #endregion


    private static void OnDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogClockFace;
      this_.Radius = (double)e.NewValue / 2;
      this_.Center = new Point(this_.Radius, this_.Radius);
    }


    private static void OnDateElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogClockFace;
      this_.OnDateElementChanged(e.OldValue as FrameworkElement, e.NewValue as FrameworkElement);
    }

    protected virtual void OnDateElementChanged(FrameworkElement oldDateElement, FrameworkElement newDateElement)
    {
      if (oldDateElement != null)
      {
        if (oldDateElement.RenderTransform is TransformGroup oldTransformGroup)
        {
          oldTransformGroup.Children.Remove(this.DateElementTransform);
        }
        else
        {
          oldDateElement.RenderTransform = null;
        }
      }

      RemoveElementFromClockFace(oldDateElement);

      if (newDateElement == null)
      {
        return;
      }

      if (newDateElement.RenderTransform is TransformGroup newTransformGroup)
      {
        newTransformGroup.Children.Add(this.DateElementTransform);
      }
      else
      {
        newDateElement.RenderTransform = this.DateElementTransform;
      }
    }


    private static void OnHourHandElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnHourHandElementChanged(e.OldValue as FrameworkElement, e.NewValue as FrameworkElement);
    }

    private static void OnMinuteHandElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnMinuteHandElementChanged(e.OldValue as FrameworkElement, e.NewValue as FrameworkElement);
    }

    private static void OnSecondHandElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSecondHandElementChanged(e.OldValue as FrameworkElement, e.NewValue as FrameworkElement);
    }

    private static void OnSelectedHourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSelectedHourChanged((double) e.OldValue, (double) e.NewValue);
    }

    private static void OnSelectedMinuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSelectedMinuteChanged((double)e.OldValue, (double)e.NewValue);
    }

    private static void OnSelectedSecondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSelectedSecondChanged((double)e.OldValue, (double)e.NewValue);
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogClockFace;

      this_.OnSelectedTimeChanged((DateTime) e.OldValue, (DateTime) e.NewValue);
    }

    private static void OnHourHandRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogClockFace;
      this_.HourHandDiameter = (double) e.NewValue * 2;
    }

    private static void OnMinuteHandRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogClockFace;
      this_.MinuteHandDiameter = (double)e.NewValue * 2;
    }

    private static void OnSecondHandRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogClockFace;
      this_.SecondHandDiameter = (double)e.NewValue * 2;
    }

    private static void OnIsTimePickerModeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnIsTimePickerModeEnabledChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static object CoerceHours(DependencyObject d, object basevalue)
    {
      var this_ = d as AnalogClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }
      this_.IsUpdatingSelectedTimeComponent = true;

      var hourValue = (double)basevalue;
      double hours = this_.Is24HModeEnabled 
        ? Math.Truncate(hourValue) % 24 
        : Math.Truncate(hourValue) % 12 == 0
          ? 12
          : Math.Truncate(hourValue) % 12;
      double decimalPart = hourValue - Math.Truncate(hourValue);
      var decimalMinutes = decimalPart * 60;
      this_.SelectedMinute = Math.Truncate(decimalMinutes);
      decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      var decimalSeconds = decimalPart * 60;
      this_.SelectedSecond = Math.Round(decimalSeconds, MidpointRounding.AwayFromZero);
      
      this_.IsUpdatingSelectedTimeComponent = false;
      return hours;
    }

    // Accept time in minutes (convert to h:m:s)
    private static object CoerceMinutes(DependencyObject d, object basevalue)
    {
      var this_ = d as AnalogClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }
      this_.IsUpdatingSelectedTimeComponent = true;
      var minuteValue = (double) basevalue;
      double decimalHours = minuteValue / 60;
      this_.SelectedHour = this_.Is24HModeEnabled 
        ? Math.Truncate(decimalHours) % 24 
        : Math.Truncate(decimalHours) % 12 == 0
          ? 12
          : Math.Truncate(decimalHours) % 12;
      double decimalPart = decimalHours - Math.Truncate(decimalHours);
      var decimalMinutes = decimalPart * 60;
      var minutes = Math.Truncate(decimalMinutes);
      decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      double decimalSeconds = decimalPart * 60;
      this_.SelectedSecond = Math.Round(decimalSeconds, MidpointRounding.AwayFromZero);

      this_.IsUpdatingSelectedTimeComponent = false;
      return minutes;
    }

    // Accept time in seconds (convert to h:m:s)
    private static object CoerceSeconds(DependencyObject d, object basevalue)
    {
      var this_ = d as AnalogClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }
      this_.IsUpdatingSelectedTimeComponent = true;
      var secondsValue = (double) basevalue;
      double decimalHours = secondsValue / 3600;
      this_.SelectedHour = this_.Is24HModeEnabled
        ? Math.Truncate(decimalHours) % 24
        : Math.Truncate(decimalHours) % 12 == 0
          ? 12
          : Math.Truncate(decimalHours) % 12;
      var decimalPart = decimalHours - Math.Truncate(decimalHours);
      double decimalMinutes = decimalPart * 60;
      this_.SelectedMinute = Math.Truncate(decimalMinutes);
      decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      double decimalSeconds = decimalPart * 60;
      var seconds = Math.Round(decimalSeconds, MidpointRounding.AwayFromZero);

      this_.IsUpdatingSelectedTimeComponent = false;
      return seconds;
    }

    protected virtual void OnIsTimePickerModeEnabledChanged(bool oldValue, bool newValue)
    {
      if (newValue)
      {
        this.Style = FindResource(AnalogClockFace.DefaultAnalogClockFaceTimePickerStyleKey) as Style;
      }
      else
      {
        this.Style = FindResource(AnalogClockFace.DefaultAnalogClockFaceStyleKey) as Style;
      }
    }

    protected virtual void OnSelectedTimeChanged(DateTime oldValue, DateTime newValue)
    {
      if (this.IsUpdatingSelectedTimeComponent)
      {
        RaiseEvent(new RoutedEventArgs(AnalogClockFace.SelectedTimeChangedRoutedEvent, this));
        return;
      }
      this.IsUpdatingSelectedTime = true;
      this.SelectedSecond = newValue.TimeOfDay.TotalSeconds;
      this.IsUpdatingSelectedTime = false;
      RaiseEvent(new RoutedEventArgs(AnalogClockFace.SelectedTimeChangedRoutedEvent, this));
    }

    protected virtual void OnSelectedHourChanged(double oldValue, double newValue)
    {
      if (this.HourHandTransform != null)
      {
        double angle = GetClockAngle(newValue, this.Is24HModeEnabled ? 24 : 12);
        this.HourHandTransform.Angle = angle;
      }

      if (this.IsTimePickerModeEnabled)
      {
        Draw24HHourArcFromZeroToCurrent();
      }

      if (this.IsUpdatingSelectedTime)
      {
        return;
      }

      this.SelectedTime = new DateTime(this.SelectedTime.Year, this.SelectedTime.Month, this.SelectedTime.Day, (int) newValue, (int) this.SelectedMinute, (int) this.SelectedSecond);
    }

    protected virtual void OnSelectedMinuteChanged(double oldValue, double newValue)
    {
      if (this.MinuteHandTransform != null)
      {
        double angle = GetClockAngle(newValue, 60);
        this.MinuteHandTransform.Angle = angle;

        if (!this.IsTimePickerModeEnabled)
        {
          OnSelectedHourChanged(this.SelectedHour, this.SelectedHour + newValue / 60);
        }
      }

      if (this.Is24HModeEnabled)
      {
        Draw24HMinuteArcFromZeroToCurrent();
      }
      else
      {
        DrawArcFromZeroToCurrent();
      }

      if (this.IsUpdatingSelectedTime)
      {
        return;
      }

      this.SelectedTime = new DateTime(this.SelectedTime.Year, this.SelectedTime.Month, this.SelectedTime.Day, (int) this.SelectedHour, (int) newValue, (int)this.SelectedSecond);
    }

    protected virtual void OnSelectedSecondChanged(double oldValue, double newValue)
    {
      if (this.SecondHandTransform != null)
      {
        double angle = GetClockAngle(newValue, 60);
        this.SecondHandTransform.Angle = angle;
      }

      if (this.Is24HModeEnabled)
      {
        Draw24HSecondArcFromZeroToCurrent();
      }

      if (this.IsUpdatingSelectedTime)
      {
        return;
      }

      this.SelectedTime = new DateTime(this.SelectedTime.Year, this.SelectedTime.Month, this.SelectedTime.Day, (int) this.SelectedHour, (int)this.SelectedMinute, (int) newValue);
    }

    protected virtual void OnHourHandElementChanged(FrameworkElement oldClockHand, FrameworkElement newClockHand)
    {
      if (oldClockHand != null)
      {
        if (oldClockHand.RenderTransform is TransformGroup oldTransformGroup)
        {
          oldTransformGroup.Children.Remove(this.HourHandTransform);
        }
        else
        {
          oldClockHand.RenderTransform = null;
        }
      }

      RemoveElementFromClockFace(oldClockHand);

      if (newClockHand == null)
      {
        return;
      }

      if (newClockHand.RenderTransform is TransformGroup newTransformGroup)
      {
        newTransformGroup.Children.Add(this.HourHandTransform);
      }
      else
      {
        newClockHand.RenderTransform = this.HourHandTransform;
      }
      AddElementToClockFace(newClockHand, new Point(Canvas.GetLeft(newClockHand), Canvas.GetTop(newClockHand)));
    }

    protected virtual void OnMinuteHandElementChanged(FrameworkElement oldClockHand, FrameworkElement newClockHand)
    {
      if (oldClockHand != null)
      {
        if (oldClockHand.RenderTransform is TransformGroup oldTransformGroup)
        {
          oldTransformGroup.Children.Remove(this.MinuteHandTransform);
        }
        else
        {
          oldClockHand.RenderTransform = null;
        }
      }

      RemoveElementFromClockFace(oldClockHand);

      if (newClockHand == null)
      {
        return;
      }

      if (newClockHand.RenderTransform is TransformGroup newTransformGroup)
      {
        newTransformGroup.Children.Add(this.MinuteHandTransform);
      }
      else
      {
        newClockHand.RenderTransform = this.MinuteHandTransform;
      }
      AddElementToClockFace(newClockHand, new Point(Canvas.GetLeft(newClockHand), Canvas.GetTop(newClockHand)));
    }

    protected virtual void OnSecondHandElementChanged(FrameworkElement oldClockHand, FrameworkElement newClockHand)
    {
      if (oldClockHand != null)
      {
        if (oldClockHand.RenderTransform is TransformGroup oldTransformGroup)
        {
          oldTransformGroup.Children.Remove(this.SecondHandTransform);
        }
        else
        {
          oldClockHand.RenderTransform = null;
        }
      }

      RemoveElementFromClockFace(oldClockHand);

      if (newClockHand == null)
      {
        return;
      }

      if (newClockHand.RenderTransform is TransformGroup newTransformGroup)
      {
        newTransformGroup.Children.Add(this.SecondHandTransform);
      }
      else
      {
        newClockHand.RenderTransform = this.SecondHandTransform;
      }
      AddElementToClockFace(newClockHand, new Point(Canvas.GetLeft(newClockHand), Canvas.GetTop(newClockHand)));
    }

    protected virtual FrameworkElement Create15MinuteIntervalVisual()
    {
      FrameworkElement intervalElement = this.FifteenMinuteIntervalElement != null
        ? CloneElement(this.FifteenMinuteIntervalElement)
        : new Ellipse
        {
          Width = 8,
          Height = 8,
          Fill = Brushes.Red
        };
        AddIntervalElementTransformToElement(intervalElement);
        return intervalElement;
    }

    protected virtual FrameworkElement Create5MinuteIntervalVisual()
    {
      FrameworkElement intervalElement = this.FiveMinuteIntervalElement != null
        ? CloneElement(this.FiveMinuteIntervalElement)
        : new Ellipse
        {
          Width = 4, Height = 4, Fill = Brushes.AntiqueWhite
        };
      AddIntervalElementTransformToElement(intervalElement);
      return intervalElement;
    }

    protected virtual FrameworkElement CreateMinuteIntervalVisual()
    {
      FrameworkElement intervalElement = this.MinuteIntervalElement != null
        ? CloneElement(this.MinuteIntervalElement)
        : new Ellipse
        {
          Width = 2, Height = 2, Fill = Brushes.AntiqueWhite
        };
      AddIntervalElementTransformToElement(intervalElement);
      return intervalElement;
    }

    protected virtual FrameworkElement Create15MinuteIntervalLabel(int labelValue)
    {
      object formattedLabelValue = this.IntervalLabelFormatter?.Invoke(labelValue) ?? labelValue;
      if (this.FifteenMinuteIntervalLabel != null
          && CloneElement(this.FifteenMinuteIntervalLabel) is FrameworkElement label && label.TryAssignValueToUnknownElement(formattedLabelValue))
      {
        return label;
      }

      return new TextBlock { Text = formattedLabelValue.ToString(), Padding = new Thickness(0)};
    }

    protected virtual FrameworkElement Create5MinuteIntervalLabel(int labelValue)
    {
      object formattedLabelValue = this.IntervalLabelFormatter?.Invoke(labelValue) ?? labelValue;
      if (this.FiveMinuteIntervalLabel != null
          && CloneElement(this.FiveMinuteIntervalLabel) is FrameworkElement label && label.TryAssignValueToUnknownElement(formattedLabelValue))
      {
        return label;
      }

      return new TextBlock { Text = formattedLabelValue.ToString(), Padding = new Thickness(0) };
    }

    protected virtual FrameworkElement CreateMinuteIntervalLabel(int labelValue)
    {
      object formattedLabelValue = this.IntervalLabelFormatter?.Invoke(labelValue) ?? labelValue;
      if (this.MinuteIntervalLabel != null 
          && CloneElement(this.MinuteIntervalLabel) is FrameworkElement label && label.TryAssignValueToUnknownElement(formattedLabelValue))
      {
        return label;
      }

      return new TextBlock { Text = formattedLabelValue.ToString(), Padding = new Thickness(0) };
    }

    protected virtual FrameworkElement CloneElement(FrameworkElement elementToClone) => elementToClone.CloneElement() as FrameworkElement;

    protected virtual void OnClockFaceLoaded() =>
      RaiseEvent(new RoutedEventArgs(AnalogClockFace.ClockFaceLoadedRoutedEvent, this));

    private double GetClockAngle(double reading, int steps) => reading * 360 / steps;

    private int GetStepLabel(int step) => this.Is24HModeEnabled 
      ? step 
      : step == 0 ? 12 : step / 5;

    private void AddIntervalElementTransformToElement(FrameworkElement intervalElement)
    {
      if (!(intervalElement is FrameworkElement frameworkElement))
      {
        return;
      }

      RotateTransform renderTransform = this.IntervalElementTransform.Clone();
      renderTransform.CenterX = AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(frameworkElement) ? frameworkElement.Width / 2 : 0;
      renderTransform.CenterY = AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(frameworkElement) ? frameworkElement.Height / 2 : 0;

      if (intervalElement.RenderTransform is TransformGroup transformGroup)
      {
        transformGroup.Children.Add(renderTransform);
      }
      else
      {
        intervalElement.RenderTransform = renderTransform;
      }
    }

    protected virtual void DrawAnalogClock()
    {
      this.ClockFaceCanvas.Children.Clear();

      if (this.Diameter == 0)
      {
        return;
      }

      DrawIntervals();
      DrawClockFaceBackground();
      DrawSelectionArcs();

      if (this.IsDisplayDateEnabled)
      {
        AddDateElement();
      }

      AddClockHands();
      OnClockFaceLoaded();
    }

    private void DrawSelectionArcs()
    {
      if (!this.IsMinuteSelectionArcEnabled)
      {
        return;
      }
      DrawArcFromZeroToCurrent();
    }

    private void DrawTimePickerSelectionArcs()
    {
      Draw24HHourArcFromZeroToCurrent();
      Draw24HMinuteArcFromZeroToCurrent();
      Draw24HSecondArcFromZeroToCurrent();
    }

    private void DrawIntervals()
    {
      double degreeOfStep = 360.0 / 60.0;
      this.IntervalMarkerCenterRadius = -1;
      for (int step = 0; step < 60; step++)
      {
        double degreesOfCurrentStep = step * degreeOfStep;

        FrameworkElement intervalMarker = CreateIntervalMarker(step);
        if (this.IntervalMarkerCenterRadius.Equals(-1))
        {
          InitializeRadiusOfIntervalMarkers(intervalMarker, this.Radius);
        }

        DrawIntervalMarker(degreesOfCurrentStep, intervalMarker);

        if (!this.IsDisplayIntervalLabelsEnabled)
        {
          continue;
        }

        FrameworkElement intervalMarkerLabel = CreateIntervalLabel(step);
        if (intervalMarkerLabel != null)
        {
          DrawIntervalLabel(degreesOfCurrentStep, intervalMarkerLabel);
        }
      }
    }

    protected virtual void DrawAnalog24Clock()
    {
      this.ClockFaceCanvas.Children.Clear();

      if (this.Diameter == 0)
      {
        return;
      }

      Draw24HHourIntervals();
      Draw24HMinuteIntervals();
      Draw24HSecondIntervals();
      Draw24ClockFaceBackground();
      DrawTimePickerSelectionArcs();
      if (!this.IsTimePickerModeEnabled || this.IsTimePickerClockHandVisible)
      {
        AddClockHands();
      }
      OnClockFaceLoaded();
    }

    private void Draw24HHourIntervals()
    {
      double degreeOfStep = 360.0 / 24.0;
      this.IntervalMarkerCenterRadius = -1;
      for (int step = 0; step < 24; step++)
      {
        double degreesOfCurrentStep = step * degreeOfStep;

        FrameworkElement intervalMarker = Create24HHoursIntervalMarker(step);
        if (this.IntervalMarkerCenterRadius.Equals(-1))
        {
          InitializeRadiusOfIntervalMarkers(intervalMarker, this.HourHandRadius);
        }

        DrawIntervalMarker(degreesOfCurrentStep, intervalMarker);

        FrameworkElement intervalMarkerLabel = CreateIntervalLabel(step);
        if (intervalMarkerLabel != null)
        {
          DrawIntervalLabel(degreesOfCurrentStep, intervalMarkerLabel);
        }
      }
    }

    private FrameworkElement Create24HHoursIntervalMarker(int step)
    {
      FrameworkElement intervalMarker;
      switch (step)
      {
        case { } is15MinutesStep when is15MinutesStep % 2 == 0:
        {
          intervalMarker = this.Is15MinuteIntervalEnabled
            ? Create15MinuteIntervalVisual()
            : this.Is5MinuteIntervalEnabled
              ? Create5MinuteIntervalVisual()
              : CreateMinuteIntervalVisual();
          break;
        }
        default:
        {
          intervalMarker = CreateMinuteIntervalVisual();
          break;
        }
      }

      return intervalMarker;
    }

    private void Draw24HMinuteIntervals()
    {
      var steps = 60.0;
      double degreeOfStep = 360.0 / steps;
      this.IntervalMarkerMinuteCenterRadius = -1;
      for (int step = 0; step < steps; step++)
      {
        double degreesOfCurrentStep = step * degreeOfStep;

        FrameworkElement intervalMarker = CreateIntervalMarker(step);
        if (this.IntervalMarkerMinuteCenterRadius.Equals(-1))
        {
          InitializeRadiusOfMinuteIntervalMarkers(intervalMarker, this.MinuteHandRadius);
        }

        Draw24HMinuteIntervalMarker(degreesOfCurrentStep, intervalMarker);

        if (step % 5 == 0)
        {
          FrameworkElement intervalMarkerLabel = CreateIntervalLabel(step);
          if (intervalMarkerLabel != null)
          {
            Draw24HMinuteIntervalLabel(degreesOfCurrentStep, intervalMarkerLabel);
          }
        }
      }
    }

    private void Draw24HSecondIntervals()
    {
      if (this.MinuteHandRadius != this.SecondHandRadius)
      {
        var steps = 60.0;
        double degreeOfStep = 360.0 / steps;
        this.IntervalMarkerSecondCenterRadius = -1;
        for (int step = 0; step < steps; step++)
        {
          double degreesOfCurrentStep = step * degreeOfStep;

          FrameworkElement intervalMarker = CreateIntervalMarker(step);
          if (this.IntervalMarkerSecondCenterRadius.Equals(-1))
          {
            InitializeRadiusOfSecondIntervalMarkers(intervalMarker, this.SecondHandRadius);
          }

          Draw24HSecondIntervalMarker(degreesOfCurrentStep, intervalMarker);
        }
      }
    }

    private void AddDateElement()
    {
      if (this.DateElement == null)
      {
        return;
      }
      
      AddElementToClockFace(this.DateElement, new Point(Canvas.GetLeft(this.DateElement), Canvas.GetTop(this.DateElement)), 2);
    }

    private void DrawClockFaceBackground()
    {
      double deltaToMiddleRadius = (this.Radius - this.IntervalMarkerCenterRadius) * 2;
      var clockFaceBackgroundPosition = new Point();
      clockFaceBackgroundPosition.Offset(deltaToMiddleRadius / 2, deltaToMiddleRadius / 2);
      var ellipse = new Ellipse()
      {
        Height = this.Diameter - deltaToMiddleRadius, Width = this.Diameter - deltaToMiddleRadius, Fill = this.Background
      };
      AddElementToClockFace(ellipse, clockFaceBackgroundPosition, 0);
    }

    private void DrawArcFromZeroToCurrent()
    {
      RemoveElementFromClockFace(this.SelectedMinuteArc);

      double angle = GetClockAngle(this.SelectedMinute, 60);
      Point cartesianMinutePointOnArc = GetCartesianPointOfStep(angle, this.IntervalMarkerCenterRadius);
      bool isLargeSelectionMinuteArc = angle > 180.0;
      Point selectedMinuteArcPoint = cartesianMinutePointOnArc.ToScreenPoint(this.Diameter);
      var selectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerCenterRadius);
      var selectionArcSize = new Size(this.IntervalMarkerCenterRadius, this.IntervalMarkerCenterRadius);
      this.SelectedMinuteArc = new Path() { Data = new PathGeometry(
        new List<PathFigure>()
        {
          new PathFigure(
            this.Center,
            new List<PathSegment>()
            {
              new LineSegment(selectionArcStart, false),
              new ArcSegment(
                selectedMinuteArcPoint,
                selectionArcSize,
                0,
                isLargeSelectionMinuteArc,
                SweepDirection.Clockwise,
                false)
            },
            true)
        }), Fill = this.MinuteSelectionArcBrush};
      AddElementToClockFace(this.SelectedMinuteArc, new Point(), 1);
    }

    protected virtual void Draw24HHourArcFromZeroToCurrent()
    {
      RemoveElementFromClockFace(this.SelectedHourArc);

      double hourValue = this.IsTimePickerModeEnabled 
        ? this.SelectedHour 
        : this.SelectedHour + this.SelectedMinute / 60;
      double angle = GetClockAngle(hourValue, 24);
      bool isLargeSelectionMinuteArc = angle > 180.0;
      Point cartesianHourPointOnArc = GetCartesianPointOfStep(angle, this.IntervalMarkerCenterRadius);
      Point outerSelectedHourArcPoint = cartesianHourPointOnArc.ToScreenPoint(this.Diameter);
      Point cartesianInnerSelectedHourArcPoint = GetCartesianPointOfStep(angle, this.IntervalMarkerMinuteCenterRadius);
      Point innerSelectedHourArcPoint = cartesianInnerSelectedHourArcPoint.ToScreenPoint(this.Diameter);
      var innerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerMinuteCenterRadius);
      var outerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerCenterRadius);
      var outerSelectionArcSize = new Size(this.IntervalMarkerCenterRadius, this.IntervalMarkerCenterRadius);
      var innerSelectionArcSize = new Size(this.IntervalMarkerMinuteCenterRadius, this.IntervalMarkerMinuteCenterRadius);
      this.SelectedHourArc = new Path
      {
        Data = new PathGeometry(
          new List<PathFigure>()
          {
            new PathFigure(
              innerSelectionArcStart,
              new List<PathSegment>()
              {
                new LineSegment(outerSelectionArcStart, false),
                new ArcSegment(
                  outerSelectedHourArcPoint,
                  outerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Clockwise,
                  false),
                new LineSegment(innerSelectedHourArcPoint, false),
                new ArcSegment(
                  innerSelectionArcStart,
                  innerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Counterclockwise,
                  false)
              },
              true)
          }),
        Fill = this.TimePickerHourSelectionArcBrush
      };
      AddElementToClockFace(this.SelectedHourArc, new Point(), 1);
    }

    protected virtual void Draw24HMinuteArcFromZeroToCurrent()
    {
      RemoveElementFromClockFace(this.SelectedMinuteArc);

      double angle = GetClockAngle(this.SelectedMinute, 60);
      bool isLargeSelectionMinuteArc = angle > 180.0;
      Point cartesianHourPointOnArc = GetCartesianPointOfStep(angle, this.IntervalMarkerMinuteCenterRadius);
      Point outerSelectedHourArcPoint = cartesianHourPointOnArc.ToScreenPoint(this.Diameter);
      Point cartesianInnerSelectedHourArcPoint = GetCartesianPointOfStep(angle, this.IntervalMarkerSecondCenterRadius);
      Point innerSelectedHourArcPoint = cartesianInnerSelectedHourArcPoint.ToScreenPoint(this.Diameter);
      var innerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerSecondCenterRadius);
      var outerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerMinuteCenterRadius);
      var outerSelectionArcSize = new Size(this.IntervalMarkerMinuteCenterRadius, this.IntervalMarkerMinuteCenterRadius);
      var innerSelectionArcSize = new Size(this.IntervalMarkerSecondCenterRadius, this.IntervalMarkerSecondCenterRadius);
      this.SelectedMinuteArc = new Path
      {
        Data = new PathGeometry(
          new List<PathFigure>()
          {
            new PathFigure(
              innerSelectionArcStart,
              new List<PathSegment>()
              {
                new LineSegment(outerSelectionArcStart, false),
                new ArcSegment(
                  outerSelectedHourArcPoint,
                  outerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Clockwise,
                  false),
                new LineSegment(innerSelectedHourArcPoint, false),
                new ArcSegment(
                  innerSelectionArcStart,
                  innerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Counterclockwise,
                  false)
              },
              true)
          }),
        Fill = this.TimePickerMinuteSelectionArcBrush
      };
      AddElementToClockFace(this.SelectedMinuteArc, new Point(), 1);
    }

    protected virtual void Draw24HSecondArcFromZeroToCurrent()
    {
      RemoveElementFromClockFace(this.SelectedSecondArc);

      double angle = GetClockAngle(this.SelectedSecond, 60);
      bool isLargeSelectionMinuteArc = angle > 180.0;
      Point cartesianHourPointOnArc = GetCartesianPointOfStep(angle, this.IntervalMarkerSecondCenterRadius);
      Point outerSelectedHourArcPoint = cartesianHourPointOnArc.ToScreenPoint(this.Diameter);
      Point cartesianInnerSelectedHourArcPoint = GetCartesianPointOfStep(angle, 0);
      Point innerSelectedHourArcPoint = cartesianInnerSelectedHourArcPoint.ToScreenPoint(this.Diameter);
      var innerSelectionArcStart = new Point(this.Radius, this.Radius - 0);
      var outerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerSecondCenterRadius);
      var outerSelectionArcSize = new Size(this.IntervalMarkerSecondCenterRadius, this.IntervalMarkerSecondCenterRadius);
      var innerSelectionArcSize = new Size(0, 0);
      this.SelectedSecondArc = new Path
      {
        Data = new PathGeometry(
          new List<PathFigure>()
          {
            new PathFigure(
              innerSelectionArcStart,
              new List<PathSegment>()
              {
                new LineSegment(outerSelectionArcStart, false),
                new ArcSegment(
                  outerSelectedHourArcPoint,
                  outerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Clockwise,
                  false),
                new LineSegment(innerSelectedHourArcPoint, false),
                new ArcSegment(
                  innerSelectionArcStart,
                  innerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Counterclockwise,
                  false)
              },
              true)
          }),
        Fill = this.TimePickerSecondSelectionArcBrush
      };
      AddElementToClockFace(this.SelectedSecondArc, new Point(), 1);
    }

    protected virtual void Draw24HTimePickerSelectionBounds()
    {
      DrawTimePickerSecondsSelectionBounds();
      DrawTimePickerMinutesSelectionBounds();
      DrawTimePickerHoursSelectionBounds();
    }

    private void DrawTimePickerSecondsSelectionBounds()
    {
      double angle = 359.9999;
      bool isLargeSelectionMinuteArc = angle > 180.0;
      Point cartesianHourPointOnArc = GetCartesianPointOfStep(angle, this.IntervalMarkerSecondCenterRadius);
      Point outerSelectedHourArcPoint = cartesianHourPointOnArc.ToScreenPoint(this.Diameter);
      Point cartesianInnerSelectedHourArcPoint = GetCartesianPointOfStep(angle, 0);
      Point innerSelectedHourArcPoint = cartesianInnerSelectedHourArcPoint.ToScreenPoint(this.Diameter);
      var innerSelectionArcStart = new Point(this.Radius, this.Radius - 0);
      var outerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerSecondCenterRadius);
      var outerSelectionArcSize = new Size(this.IntervalMarkerSecondCenterRadius, this.IntervalMarkerSecondCenterRadius);
      var innerSelectionArcSize = new Size(0, 0);
      this.SelectedSecondArcBounds = new PathGeometry(
        new List<PathFigure>()
        {
          new PathFigure(
            innerSelectionArcStart,
            new List<PathSegment>()
            {
              new LineSegment(outerSelectionArcStart, false),
              new ArcSegment(
                outerSelectedHourArcPoint,
                outerSelectionArcSize,
                0,
                isLargeSelectionMinuteArc,
                SweepDirection.Clockwise,
                false),
              new LineSegment(innerSelectedHourArcPoint, false),
              new ArcSegment(
                innerSelectionArcStart,
                innerSelectionArcSize,
                0,
                isLargeSelectionMinuteArc,
                SweepDirection.Counterclockwise,
                false)
            },
            true)
        });
    }

    protected virtual void DrawTimePickerMinutesSelectionBounds()
    {
      double angle = 359.9999;
      bool isLargeSelectionMinuteArc = angle > 180.0;
      Point cartesianHourPointOnArc = GetCartesianPointOfStep(angle, this.IntervalMarkerMinuteCenterRadius);
      Point outerSelectedHourArcPoint = cartesianHourPointOnArc.ToScreenPoint(this.Diameter);
      Point cartesianInnerSelectedHourArcPoint = GetCartesianPointOfStep(angle, this.IntervalMarkerSecondCenterRadius);
      Point innerSelectedHourArcPoint = cartesianInnerSelectedHourArcPoint.ToScreenPoint(this.Diameter);
      var innerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerSecondCenterRadius);
      var outerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerMinuteCenterRadius);
      var outerSelectionArcSize = new Size(this.IntervalMarkerMinuteCenterRadius, this.IntervalMarkerMinuteCenterRadius);
      var innerSelectionArcSize = new Size(this.IntervalMarkerSecondCenterRadius, this.IntervalMarkerSecondCenterRadius);
      this.SelectedMinuteArcBounds = new PathGeometry(
          new List<PathFigure>()
          {
            new PathFigure(
              innerSelectionArcStart,
              new List<PathSegment>()
              {
                new LineSegment(outerSelectionArcStart, false),
                new ArcSegment(
                  outerSelectedHourArcPoint,
                  outerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Clockwise,
                  false),
                new LineSegment(innerSelectedHourArcPoint, false),
                new ArcSegment(
                  innerSelectionArcStart,
                  innerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Counterclockwise,
                  false)
              },
              true)
          });
    }

    protected virtual void DrawTimePickerHoursSelectionBounds()
    {
      double angle = 359.9999;
      bool isLargeSelectionMinuteArc = angle > 180.0;
      Point cartesianHourPointOnArc = GetCartesianPointOfStep(angle, this.IntervalMarkerCenterRadius);
      Point outerSelectedHourArcPoint = cartesianHourPointOnArc.ToScreenPoint(this.Diameter);
      Point cartesianInnerSelectedHourArcPoint = GetCartesianPointOfStep(angle, this.IntervalMarkerMinuteCenterRadius);
      Point innerSelectedHourArcPoint = cartesianInnerSelectedHourArcPoint.ToScreenPoint(this.Diameter);
      var innerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerMinuteCenterRadius);
      var outerSelectionArcStart = new Point(this.Radius, this.Radius - this.IntervalMarkerCenterRadius);
      var outerSelectionArcSize = new Size(this.IntervalMarkerCenterRadius, this.IntervalMarkerCenterRadius);
      var innerSelectionArcSize = new Size(this.IntervalMarkerMinuteCenterRadius, this.IntervalMarkerMinuteCenterRadius);
      var selectionArcPathBrush = new SolidColorBrush(Colors.Black) { Opacity = 0.6 };
      this.SelectedHourArcBounds = new PathGeometry(
          new List<PathFigure>()
          {
            new PathFigure(
              innerSelectionArcStart,
              new List<PathSegment>()
              {
                new LineSegment(outerSelectionArcStart, false),
                new ArcSegment(
                  outerSelectedHourArcPoint,
                  outerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Clockwise,
                  false),
                new LineSegment(innerSelectedHourArcPoint, false),
                new ArcSegment(
                  innerSelectionArcStart,
                  innerSelectionArcSize,
                  0,
                  isLargeSelectionMinuteArc,
                  SweepDirection.Counterclockwise,
                  false)
              },
              true)
          });
    }

    private void Draw24ClockFaceBackground()
    {
      double deltaToMiddleRadius = (this.Radius - this.IntervalMarkerCenterRadius) * 2;
      var clockFaceBackgroundPosition = new Point();
      clockFaceBackgroundPosition.Offset(deltaToMiddleRadius / 2, deltaToMiddleRadius / 2);
      var ellipse = new Ellipse()
      {
        Height = this.Diameter - deltaToMiddleRadius,
        Width = this.Diameter - deltaToMiddleRadius,
        Fill = this.Background
      };
      AddElementToClockFace(ellipse, clockFaceBackgroundPosition, 0);
    }

    private void DrawIntervalLabel(double degreesOfCurrentStep, FrameworkElement intervalMarkerLabel)
    {
      Point cartesianPoint;
      cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, this.IntervalMarkerCenterRadius + this.IntervalLabelRadiusOffset);
      AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarkerLabel);
      AddCartesianElementToClockFace(intervalMarkerLabel, cartesianPoint, 2);
    }

    private void Draw24HMinuteIntervalLabel(double degreesOfCurrentStep, FrameworkElement intervalMarkerLabel)
    {
      Point cartesianPoint;
      cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, this.IntervalMarkerMinuteCenterRadius + this.IntervalLabelRadiusOffset);
      AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarkerLabel);
      AddCartesianElementToClockFace(intervalMarkerLabel, cartesianPoint, 2);
    }

    private void Draw24HSecondIntervalLabel(double degreesOfCurrentStep, FrameworkElement intervalMarkerLabel)
    {
      Point cartesianPoint;
      cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, this.IntervalMarkerSecondCenterRadius + this.IntervalLabelRadiusOffset);
      AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarkerLabel);
      AddCartesianElementToClockFace(intervalMarkerLabel, cartesianPoint, 2);
    }

    private void Draw24HSecondIntervalMarker(
      double degreesOfCurrentStep,
      FrameworkElement intervalMarker)
    {
      AnalogClockFace.RotateIntervalMarker(degreesOfCurrentStep, intervalMarker);
      Point cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, this.IntervalMarkerSecondCenterRadius);
      if (AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(intervalMarker))
      {
        AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarker);
      }

      AddCartesianElementToClockFace(intervalMarker, cartesianPoint, 2);
    }

    private void Draw24HMinuteIntervalMarker(
      double degreesOfCurrentStep,
      FrameworkElement intervalMarker)
    {
      AnalogClockFace.RotateIntervalMarker(degreesOfCurrentStep, intervalMarker);
      Point cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, this.IntervalMarkerMinuteCenterRadius);
      if (AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(intervalMarker))
      {
        AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarker);
      }

      AddCartesianElementToClockFace(intervalMarker, cartesianPoint, 2);
    }

    private void DrawIntervalMarker(
      double degreesOfCurrentStep,
      FrameworkElement intervalMarker)
    {
      AnalogClockFace.RotateIntervalMarker(degreesOfCurrentStep, intervalMarker);
      Point cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, this.IntervalMarkerCenterRadius);
      if (AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(intervalMarker))
      {
        AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarker);
      }

      AddCartesianElementToClockFace(intervalMarker, cartesianPoint, 2);
    }

    private static void RotateIntervalMarker(double degreesOfCurrentStep, FrameworkElement intervalMarker)
    {
      RotateTransform rotateTransform;
      if (intervalMarker.RenderTransform is TransformGroup transformGroup)
      {
        rotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();
      }
      else
      {
        rotateTransform = intervalMarker.RenderTransform as RotateTransform;
      }

      if (rotateTransform != null)
      {
        rotateTransform.Angle = degreesOfCurrentStep;
      }
    }

    private FrameworkElement CreateIntervalMarker(int step)
    {
      FrameworkElement intervalMarker;
      switch (step)
      {
        case { } is15MinutesStep when is15MinutesStep % 15 == 0:
        {
          intervalMarker = this.Is15MinuteIntervalEnabled
            ? Create15MinuteIntervalVisual()
            : this.Is5MinuteIntervalEnabled
              ? Create5MinuteIntervalVisual()
              : CreateMinuteIntervalVisual();
          break;
        }
        case { } is5MinutesStep when is5MinutesStep % 5 == 0:
        {
          intervalMarker = Create5MinuteIntervalVisual();
          break;
        }
        default:
        {
          intervalMarker = CreateMinuteIntervalVisual();
          break;
        }
      }

      return intervalMarker;
    }

    private FrameworkElement CreateIntervalLabel(int step)
    {
      FrameworkElement intervalMarkerLabel = null;
      switch (step)
      {
        case { } is15MinutesStep when is15MinutesStep % 15 == 0:
        {
          var stepLabel = GetStepLabel(step);
          intervalMarkerLabel = this.Is15MinuteIntervalEnabled
            ? Create15MinuteIntervalLabel(stepLabel)
            : this.Is5MinuteIntervalEnabled
              ? Create5MinuteIntervalLabel(stepLabel)
              : CreateMinuteIntervalLabel(stepLabel);
          break;
        }
        case { } is5MinutesStep when is5MinutesStep % 5 == 0:
        {
          var stepLabel = GetStepLabel(step);
          intervalMarkerLabel = Create5MinuteIntervalLabel(stepLabel);
          break;
        }
        default:
        {
          if (this.Is24HModeEnabled)
          {
            var stepLabel = GetStepLabel(step);
            intervalMarkerLabel = CreateMinuteIntervalLabel(stepLabel);
          }
          break;
        }
      }

      return intervalMarkerLabel;
    }

    private void InitializeRadiusOfIntervalMarkers(FrameworkElement intervalMarker, double radius)
    {
      double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
      this.IntervalMarkerCenterRadius = radius + radiusOffset;
    }

    private void InitializeRadiusOfMinuteIntervalMarkers(FrameworkElement intervalMarker, double radius)
    {
      double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
      this.IntervalMarkerMinuteCenterRadius = radius + radiusOffset;
    }

    private void InitializeRadiusOfSecondIntervalMarkers(FrameworkElement intervalMarker, double radius)
    {
      double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
      this.IntervalMarkerSecondCenterRadius = radius + radiusOffset;
    }

    private void AlignElementCenterPointToRadius(ref Point cartesianPoint, FrameworkElement element)
    {
      if (!element.IsMeasureValid)
      {
        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        element.Arrange(new Rect(new Point(), element.DesiredSize));
      }

      double elementVerticalCenterPointOffset = element.DesiredSize.Height / 2;
      double elementHorizontalCenterPointOffset = element.DesiredSize.Width / 2;

      cartesianPoint.Offset(-elementHorizontalCenterPointOffset, elementVerticalCenterPointOffset);
    }

    private void AddClockHands()
    {
      if (this.HourHandElement != null)
      {
        AddElementToClockFace(this.HourHandElement, new Point(Canvas.GetLeft(this.HourHandElement), Canvas.GetTop(this.HourHandElement)), Panel.GetZIndex(this.HourHandElement) + 3);
      }
      if (this.MinuteHandElement != null)
      {
        AddElementToClockFace(this.MinuteHandElement, new Point(Canvas.GetLeft(this.MinuteHandElement), Canvas.GetTop(this.MinuteHandElement)), Panel.GetZIndex(this.MinuteHandElement) + 4);
      }
      if (this.SecondHandElement != null)
      {
        AddElementToClockFace(this.SecondHandElement, new Point(Canvas.GetLeft(this.SecondHandElement), Canvas.GetTop(this.SecondHandElement)), Panel.GetZIndex(this.SecondHandElement) + 5);
      }
    }

    private double CalculateIntervalMarkerCenterRadiusOffset(FrameworkElement intervalMarker)
    {
      if (!intervalMarker.IsMeasureValid)
      {
        intervalMarker.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      }
      double intervalElementCenterPointOffset = intervalMarker.DesiredSize.Height / 2;
      return -intervalElementCenterPointOffset;
    }

    private Point GetCartesianPointOfStep(double degreesOfCurrentStep, double radius)
    {
      double axisOffset = this.Radius;

      // Rotate and invert degrees in order to move the 0 clock value to the top
      // (instead of the original right -> cartesian based circle)
      double rotatedDegrees = degreesOfCurrentStep - 90;
      double invertedDegrees =  -rotatedDegrees;
      var arcRadiantDegree = invertedDegrees * Math.PI / 180;

      double x = Math.Cos(arcRadiantDegree) * radius;
      double y = Math.Sin(arcRadiantDegree) * radius;
      var cartesianPoint = new Point(x, y);

      // Move origin from circle center to bottom left edge by its radius,
      // so that full circle is inside first quadrant
      cartesianPoint.Offset(axisOffset, axisOffset);
      return cartesianPoint;
    }

    private double GetAngleFromCartesianPoint(Point screenPoint)
    {
      var cartesianPoint = screenPoint.ToCartesianPoint(this.Diameter);
      double axisOffset = this.Radius;
      cartesianPoint.Offset(-axisOffset, -axisOffset);

      double angleFromCartesianPoint = Math.Atan(cartesianPoint.Y / cartesianPoint.X) / Math.PI * 180;

      // Rotate and invert degrees in order to move the 0 clock value to the top
      // (instead of the original right -> cartesian based circle)
      double rotatedDegrees = angleFromCartesianPoint - 90;
      double invertedDegrees = -rotatedDegrees;
      return cartesianPoint.X < 0 ? 180 + invertedDegrees : invertedDegrees;
    }

    public void AddCartesianElementToClockFace(FrameworkElement clockElement, Point cartesianPoint, int zIndex = 1)
    {
      Point screenPoint = cartesianPoint.ToScreenPoint(this.Diameter);
      AddElementToClockFace(clockElement, screenPoint, zIndex);
    }

    public void AddElementToClockFace(FrameworkElement clockElement, Point screenPoint, int zIndex = 1)
    {
      Canvas.SetLeft(clockElement, screenPoint.X);
      Canvas.SetTop(clockElement, screenPoint.Y);
      Panel.SetZIndex(clockElement, zIndex);
      this.ClockFaceCanvas.Children.Add(clockElement);
    }

    public void RemoveElementFromClockFace(FrameworkElement clockElement) => this.ClockFaceCanvas.Children.Remove(clockElement);
  }
}