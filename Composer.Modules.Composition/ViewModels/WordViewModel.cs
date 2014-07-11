using System;
using System.Globalization;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class WordViewModel : BaseViewModel, IWordViewModel
    {
        private const double CharWidth = 7.0;

        private string _word;
        public string Word
        {
            get { return _word; }
            set
            {
                _word = value;
                OnPropertyChanged(() => Word);
            }
        }

        private string _wordAlignment;
        public string alignment
        {
            get { return _wordAlignment; }
            set
            {
                _wordAlignment = value;
                OnPropertyChanged(() => alignment);
            }
        }

        private string _left;
        public string Left
        {
            get { return _left; }
            set
            {
                _left = value;
                OnPropertyChanged(() => Left);
            }
        }

        private int _locationX;
        public int Location_X
        {
            get { return _locationX; }
            set
            {
                _locationX = value;
                OnPropertyChanged(() => Location_X);
            }
        }

        private string _margin = string.Format("{0},{1},0,0", Preferences.VerseWordLeftMargin, Preferences.VerseWordTopMargin);
        public string Margin
        {
            get { return _margin; }
            set
            {
                _margin = value;
                OnPropertyChanged(() => Margin);
            }
        }

        public WordViewModel(string joinedMetaWords)
        {
            var metaWords = joinedMetaWords.Split(Defaults.VerseWordPropertyDelimitter);

            Word = metaWords[2];
            Location_X = int.Parse(metaWords[3]);

            var midX = (int)Math.Ceiling(Word.Length * CharWidth / 2);

            alignment = metaWords[5];

            //???? Left is calculated here.....
            Left = (Word.Length > 2) ?
                (Location_X - midX + 14 + Word.Length - 6).ToString(CultureInfo.InvariantCulture) : 
                (Location_X - midX + 8).ToString(CultureInfo.InvariantCulture);

            //???? then if alignment = 'Left' we set Left to Measure.Padding, and if alignment != 'Left' then set Left = Left ?
            //i'm sure it all made sense when I originally wrote it.
            Left = (alignment == "Left") ? 
                (Measure.Padding).ToString(CultureInfo.InvariantCulture) : 
                Left;

            DefineCommands();
            SubscribeEvents();
        }

        private void DefineCommands()
        {
        }

        private void SubscribeEvents()
        {
        }
        private string _emptyBind = string.Empty;
        public new string EmptyBind
        {
            get
            {
                return _emptyBind;
            }
            set
            {
                if (value != _emptyBind)
                {
                    _emptyBind = value;
                    OnPropertyChanged(() => EmptyBind);
                }
            }
        }
    }
}