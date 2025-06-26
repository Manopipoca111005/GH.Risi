using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MauiApp1
{
    public class AssiduidadeModel : INotifyPropertyChanged
    {
        private string _data;
        public string Data
        {
            get => _data;
            set
            {
                if (_data != value)
                {
                    _data = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ParsedDate));
                }
            }
        }

        private string _e1;
        public string E1
        {
            get => _e1;
            set { if (_e1 != value) { _e1 = value; OnPropertyChanged(); } }
        }

        private string _s1;
        public string S1
        {
            get => _s1;
            set { if (_s1 != value) { _s1 = value; OnPropertyChanged(); } }
        }

        private string _e2;
        public string E2
        {
            get => _e2;
            set { if (_e2 != value) { _e2 = value; OnPropertyChanged(); } }
        }

        private string _s2;
        public string S2
        {
            get => _s2;
            set { if (_s2 != value) { _s2 = value; OnPropertyChanged(); } }
        }

        private string _e3;
        public string E3
        {
            get => _e3;
            set { if (_e3 != value) { _e3 = value; OnPropertyChanged(); } }
        }

        private string _s3;
        public string S3
        {
            get => _s3;
            set { if (_s3 != value) { _s3 = value; OnPropertyChanged(); } }
        }

        private string _e4;
        public string E4
        {
            get => _e4;
            set { if (_e4 != value) { _e4 = value; OnPropertyChanged(); } }
        }

        private string _s4;
        public string S4
        {
            get => _s4;
            set { if (_s4 != value) { _s4 = value; OnPropertyChanged(); } }
        }

        private bool _temOcorrencia;
        public bool TemOcorrencia
        {
            get => _temOcorrencia;
            set
            {
                if (_temOcorrencia != value)
                {
                    _temOcorrencia = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime ParsedDate
        {
            get
            {
                if (DateTime.TryParse(Data, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    return date.Date;
                }
                if (DateTime.TryParseExact(Data, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date.Date;
                }
                if (DateTime.TryParseExact(Data, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date.Date;
                }
                if (Data != null && Data.Contains("T"))
                {
                    if (DateTime.TryParseExact(Data.Split('T')[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        return date.Date;
                    }
                }
                return DateTime.MinValue;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}