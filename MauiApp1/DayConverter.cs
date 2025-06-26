// DayConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiApp1 
{
    public class DayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dayDataString && parameter is string param)
            {
           
                string[] parts = dayDataString.Split('_');

                if (parts.Length == 6) 
                {
                    switch (param)
                    {
                        
                        case "Colab1DayOfMonth": return parts[0];
                        case "Colab1DayOfWeek": return parts[1];
                        case "Colab1Shift": return parts[2];
                        
                        case "Colab2DayOfMonth": return parts[3];
                        case "Colab2DayOfWeek": return parts[4];
                        case "Colab2Shift": return parts[5];
                    }
                }
            }
            return null; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
