﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DistribuicaoTurmas.Helpers
{
    public class NotBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Visibility v && v != Visibility.Visible;
    }
}
