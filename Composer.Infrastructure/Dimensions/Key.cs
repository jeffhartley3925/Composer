using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure.Constants;

namespace Composer.Infrastructure.Dimensions
{
    public class Key
    {
        public short Id { get; set; }
        public short Index { get; set; }
        public string Name { get; set; }
        public Tuple<string, int?> BindingHelper { get; set; }
        public string Scale { get; set; }
        public int? AccidentalCount { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Vector { get; set; }
        public bool Listable { get; set; }

        public static Key operator +(Key key, int interval)
        {
            int keyIndex = (key.Id + interval) % Keys.SemitoneCount;
            return Keys.KeyList[keyIndex];
        }

        public static Key operator -(Key key, int interval)
        {
            int keyIndex = (key.Id - interval) % Keys.SemitoneCount;
            return Keys.KeyList[keyIndex];
        }
    }

    public class Interval
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class Scale
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Accidental { get; set; }
        public string Accidentaled { get; set; }
        public string Value { get; set; }
    }

    public static class Keys
    {
        public static List<Key> KeyList = new List<Key>();
        public static List<Interval> IntervalList;

        private static Key _key;

        public static Key Key
        {
            get { return _key; }
            set
            {
                _key = value;

                KeyScaleMap();//TODO make sure this is called only once globally.

                EditorState.AccidentalNotes = new List<string>();
                EditorState.TargetAccidental = (from a
                                                in keyScaleMap
                                                where (a.Name.IndexOf("Major", StringComparison.Ordinal) > 0 && a.Name.StartsWith(string.Format("{0} ", _key.Name)))
                                                select a.Accidental).First();

                string temp = (from a
                               in keyScaleMap
                               where (a.Name.IndexOf("Major", StringComparison.Ordinal) > 0 && a.Name.StartsWith(string.Format("{0} ", _key.Name)))
                               select a.Accidentaled).Single();
                string[] tempArr = temp.Split(',');
                foreach (string t in tempArr)
                {
                    EditorState.AccidentalNotes.Add(t);
                }
            }
        }

        public static Interval Interval = null;
        public static int SemitoneCount = 12;

        public static IDictionary<string, string> keySymbolVisibilityMap = new Dictionary<string, string>();
        public static IDictionary<string, string> keySymbolMap = new Dictionary<string, string>();
        public static List<Scale> keyScaleMap = new List<Scale>();

        static Keys()
        {
            //InitializeKeys();
            InitializeIntervals();
        }

        public static void InitializeIntervals()
        {
            IntervalList = new List<Interval>
                               {
                                   new Interval() {Id = 2, Name = "Major second", Value = 3},
                                   new Interval() {Id = 3, Name = "Major third", Value = 4},
                                   new Interval() {Id = 4, Name = "Major sixth", Value = 7},
                                   new Interval() {Id = 5, Name = "Major seventh", Value = 8},
                                   new Interval() {Id = 6, Name = "Perfect fourth", Value = 4},
                                   new Interval() {Id = 7, Name = "Perfect fifth", Value = 5},
                                   new Interval() {Id = 8, Name = "Minor second", Value = 2},
                                   new Interval() {Id = 9, Name = "Minor third", Value = 3},
                                   new Interval() {Id = 10, Name = "Minor sixth", Value = 5},
                                   new Interval() {Id = 11, Name = "Minor seventh", Value = 6}
                               };

            Interval = (from a in IntervalList where a.Name == Preferences.DefaultInterval select a).Single();
        }

        public static void InitializeKeys()
        {
            if (KeyList.Count == 0)
            {
                KeyList = new List<Key>();

                KeyList.Add(new Key() { Id = 0, Index = 0, Name = "C", AccidentalCount = 0, Scale = "C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb", Caption = "Key of C", Listable = true, BindingHelper = new Tuple<string, int?>("", 0) });
                KeyList.Add(new Key() { Id = 1, Index = 7, Name = "G", AccidentalCount = 1, Scale = "G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb", Caption = "Key of G", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Sharp, 1) });
                KeyList.Add(new Key() { Id = 2, Index = 2, Name = "D", AccidentalCount = 2, Scale = "D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db", Caption = "Key of D", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Sharp, 2) });
                KeyList.Add(new Key() { Id = 3, Index = 9, Name = "A", AccidentalCount = 3, Scale = "A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab", Caption = "Key of A", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Sharp, 3) });
                KeyList.Add(new Key() { Id = 4, Index = 4, Name = "E", AccidentalCount = 4, Scale = "E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb", Caption = "Key of E", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Sharp, 4) });
                KeyList.Add(new Key() { Id = 5, Index = 11, Name = "B", AccidentalCount = 5, Scale = "B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb", Caption = "Key of B", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Sharp, 5) });
                KeyList.Add(new Key() { Id = 12, Index = 6, Name = "Gb", AccidentalCount = 6, Scale = "Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F", Caption = "Key of Gb", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Flat, 6) });
                KeyList.Add(new Key() { Id = 13, Index = 1, Name = "Db", AccidentalCount = 5, Scale = "Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C", Caption = "Key of Db", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Flat, 5) });
                KeyList.Add(new Key() { Id = 8, Index = 8, Name = "Ab", AccidentalCount = 4, Scale = "Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G", Caption = "Key of Ab", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Flat, 4) });
                KeyList.Add(new Key() { Id = 9, Index = 3, Name = "Eb", AccidentalCount = 3, Scale = "Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D", Caption = "Key of Eb", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Flat, 3) });
                KeyList.Add(new Key() { Id = 10, Index = 10, Name = "Bb", AccidentalCount = 2, Scale = "As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A", Caption = "Key of Bb", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Flat, 2) });

                //In no circumstances may this note be called E♯
                KeyList.Add(new Key() { Id = 11, Index = 5, Name = "F", AccidentalCount = 1, Scale = "F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb", Caption = "Key of F", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Flat, 1) });
                KeyList.Add(new Key() { Id = 6, Index = 6, Name = "Fs", AccidentalCount = 6, Scale = "Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F", Caption = "Key of Fs", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Sharp, 6) });
                KeyList.Add(new Key() { Id = 7, Index = 1, Name = "Cs", AccidentalCount = 7, Scale = "Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C", Caption = "Key of Cs", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Sharp, 7) });

                //1. the enharmonically equivalent key signature B major (five sharps) is usually used instead
                //2. C♭ major is the only major or minor key, other than theoretical keys, which has "flat" or "sharp" in its name, but whose tonic note is the enharmonic equivalent of a natural note (a white key on a keyboard instrument).
                KeyList.Add(new Key() { Id = 14, Index = 11, Name = "Cb", AccidentalCount = 7, Scale = "B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb", Caption = "Key of Cb", Listable = true, BindingHelper = new Tuple<string, int?>(_Accidental.Flat, 7) });

                //In no circumstances may this note be used as F Major
                KeyList.Add(new Key() { Id = 18, Index = 5, Name = "Es", AccidentalCount = null, Scale = "F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb", Caption = "", Listable = false, BindingHelper = new Tuple<string, int?>("", null) });

                //For clarity and simplicitly, F-flat major is usually notated as its enharmonic equivalent of E major.
                KeyList.Add(new Key() { Id = 19, Index = 4, Name = "Fb", AccidentalCount = null, Scale = "E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb", Caption = "Key of Fb", Listable = false, BindingHelper = new Tuple<string, int?>("", null) });

                //Theoretical keys
                //For clarity and simplicity, G-sharp major is usually notated as its enharmonic equivalent of A-flat major
                KeyList.Add(new Key() { Id = 15, Index = 8, Name = "Gs", AccidentalCount = null, Scale = "Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G", Caption = "Theoretical", Listable = false, BindingHelper = new Tuple<string, int?>("", null) });
                KeyList.Add(new Key() { Id = 16, Index = 3, Name = "Ds", AccidentalCount = null, Scale = "Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb,C,Cs-Db,D", Caption = "Theoretical", Listable = false, BindingHelper = new Tuple<string, int?>("", null) });
                KeyList.Add(new Key() { Id = 17, Index = 10, Name = "As", AccidentalCount = null, Scale = "As-Bb,B-Cb,C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A", Caption = "Theoretical", Listable = false, BindingHelper = new Tuple<string, int?>("", null) });
                KeyList.Add(new Key() { Id = 18, Index = 0, Name = "Bs", AccidentalCount = null, Scale = "C,Cs-Db,D,Ds-Eb,E-Fb,F,Fs-Gb,G,Gs-Ab,A,As-Bb,B-Cb", Caption = "", Listable = false, BindingHelper = new Tuple<string, int?>("", null) });
                //End Theoretical keys

                Key = (from a in KeyList where a.Name == Preferences.DefaultKey select a).Single();

                KeyScaleMap();
            }
        }

        private static void KeyScaleMap()
        {
            keyScaleMap = new List<Scale>();
            keyScaleMap.Add(new Scale() { Id = 0, Name = "C Major", ShortName = "C", Accidental = "", Value = "C,D,E,F,G,A,B", Accidentaled = "" });
            keyScaleMap.Add(new Scale() { Id = 1, Name = "F Major", ShortName = "F", Accidental = "f", Value = "F,G,A,Bb,C,D,E", Accidentaled = "B" });
            keyScaleMap.Add(new Scale() { Id = 2, Name = "Bb Major", ShortName = "Bb", Accidental = "f", Value = "Bb,C,D,Eb,F,G,A", Accidentaled = "B,E" });
            keyScaleMap.Add(new Scale() { Id = 3, Name = "Eb Major", ShortName = "Eb", Accidental = "f", Value = "Eb,F,G,Ab,Bb,C,D", Accidentaled = "B,E,A" });
            keyScaleMap.Add(new Scale() { Id = 4, Name = "Ab Major", ShortName = "Ab", Accidental = "f", Value = "Ab,Bb,C,Db,Eb,F,G", Accidentaled = "B,E,A,D" });
            keyScaleMap.Add(new Scale() { Id = 5, Name = "Db Major", ShortName = "Db", Accidental = "f", Value = "Db,Eb,F,Gb,Ab,B,C", Accidentaled = "B,E,A,D,G" });
            keyScaleMap.Add(new Scale() { Id = 6, Name = "Gb Major", ShortName = "Gb", Accidental = "f", Value = "Gb,Ab,Bb,Cb,Db,Eb,F", Accidentaled = "B,E,A,D,G,C" });
            keyScaleMap.Add(new Scale() { Id = 7, Name = "Cb Major", ShortName = "Cb", Accidental = "f", Value = "Cb,Db,Eb,Fb,Gb,Ab,Bb", Accidentaled = "B,E,A,D,G,C,F" });
            keyScaleMap.Add(new Scale() { Id = 8, Name = "G Major", ShortName = "G", Accidental = "s", Value = "G,A,B,C,D,E,Fs", Accidentaled = "F" });
            keyScaleMap.Add(new Scale() { Id = 9, Name = "D Major", ShortName = "D", Accidental = "s", Value = "D,E,Fs,G,A,B,Cs", Accidentaled = "F,G" });
            keyScaleMap.Add(new Scale() { Id = 10, Name = "A Major", ShortName = "A", Accidental = "s", Value = "A,B,Cs,D,E,Fs,Gs", Accidentaled = "F,G,C" });
            keyScaleMap.Add(new Scale() { Id = 11, Name = "E Major", ShortName = "E", Accidental = "s", Value = "E,Fs,Gs,A,B,Cs,Ds", Accidentaled = "F,G,C,D" });
            keyScaleMap.Add(new Scale() { Id = 12, Name = "B Major", ShortName = "B", Accidental = "s", Value = "B,Cs,Ds,E,Fs,Gs,As", Accidentaled = "F,G,C,D,A" });
            keyScaleMap.Add(new Scale() { Id = 13, Name = "Fs Major", ShortName = "Fs", Accidental = "s", Value = "Fs,Gs,As,B,Cs,Ds,Es", Accidentaled = "F,G,C,D,A,E" });
            keyScaleMap.Add(new Scale() { Id = 14, Name = "Cs Major", ShortName = "Cs", Accidental = "s", Value = "Cs,Ds,Es,Fs,Gs,As,Bs", Accidentaled = "F,G,C,D,A,E,B" });
        }

        private static Key ChangeKey(string key)
        {
            var k = (from a in KeyList where a.Name.Trim() == key select a).DefaultIfEmpty(null).Single();
            if (k != null)
            {
                Key = k;
            }
            return Key;
        }

        public static Key ChangeKey(int interval)
        {
            return Key + interval;
        }

        public static Key ChangeKey(Key key, int interval)
        {
            return key + interval;
        }
    }
}