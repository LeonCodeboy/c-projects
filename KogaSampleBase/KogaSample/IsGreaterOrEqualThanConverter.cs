using System;
using System.Globalization;
using System.Windows.Data;

namespace KogaSample
{
    public class IsEqualOrGreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IComparable cValue = value.ToString() as IComparable;
            IComparable cParameter = parameter as IComparable;

            return (cValue.CompareTo(cParameter) >= 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
