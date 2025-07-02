using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace Tracer.Converters
{
    public class ElapsedDaysAlertConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = EalpsedDays (int)
            // values[1] = SelectedAlertElapsedDay (int)
            // parameter = "IsAlert", "Hover", "Selected" または null

            if (values.Length < 2 || values[0] == null || values[1] == null)
                return GetDefaultValue(targetType, parameter as string);

            if (!(values[0] is int elapsedDays) || !(values[1] is int alertDays))
                return GetDefaultValue(targetType, parameter as string);

            // 経過日数が警告日数を超えているかチェック
            bool isAlert = elapsedDays > alertDays;

            string state = parameter as string;

            // Boolean型を返す場合（IsAlertパラメーター用）
            if (targetType == typeof(bool) || state == "IsAlert")
            {
                return isAlert;
            }

            // Brush型を返す場合
            if (isAlert)
            {
                switch (state)
                {
                    case "Hover":
                        return new SolidColorBrush(Color.FromRgb(181, 136, 155)); // Mauve + ホバー効果
                    case "Selected":
                        return new SolidColorBrush(Color.FromRgb(155, 89, 182)); // Mauve選択時
                    default:
                        return new SolidColorBrush(Color.FromRgb(230, 215, 222)); // 薄いMauve背景色
                }
            }
            else
            {
                switch (state)
                {
                    case "Hover":
                        return new SolidColorBrush(Color.FromRgb(240, 240, 240)); // 通常のホバー色
                    case "Selected":
                        return new SolidColorBrush(Color.FromRgb(155, 89, 182)); // Mauve選択色
                    default:
                        return Brushes.Transparent; // 通常時は透明
                }
            }
        }

        private object GetDefaultValue(Type targetType, string parameter)
        {
            if (targetType == typeof(bool) || parameter == "IsAlert")
                return false;

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}