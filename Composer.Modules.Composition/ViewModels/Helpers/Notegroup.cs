using System;
using System.Collections.Generic;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{
    //chs contain 1 or more ngs. a a is a group of ns in a ch with the
    //same stem direction and the same d.
    public class Notegroup
    {
        public enum ExtremityMode
        {
            Tip,
            Root
        }

        private ExtremityMode _mode;

        public Guid Id { get; set; }
        public int GroupX { get; set; }
        public int GroupY { get; set; }
        public Decimal Duration { get; set; }
        public Double StartTime { get; set; }
        public short? Orientation { get; set; }
        public bool IsRest { get; set; }

        private bool _isSpanned;
        public bool IsSpanned
        {
            get { return _isSpanned; }
            set
            {
                if (value != _isSpanned)
                {
                    _isSpanned = value;
                    Root.IsSpanned = true;
                }
            }
        }

        private Repository.DataService.Note _root;
        public Repository.DataService.Note Root
        {
            get
            {
                _mode = ExtremityMode.Root;
                switch (Notes.Count)
                {
                    case 0:
                        _root = null;
                        break;
                    case 1:
                        _root = Notes[0];
                        break;
                    default:
                        _root = GetExtremity();
                        break;
                }
                return _root;
            }
            set
            {
                _root = value;
                if (_root != null)
                {
                    IsRest = _root.Pitch.Trim().ToUpper() == Infrastructure.Constants.Defaults.RestSymbol;
                }
            }
        }

        public List<Repository.DataService.Note> Notes { get; set; }

        public Notegroup(decimal d, Double st, short orientation)
        {
            Id = Guid.NewGuid();
            Duration = d;
            StartTime = st;
            Orientation = orientation;
            Notes = new List<Repository.DataService.Note>();
            Root = null;
            IsSpanned = false;
            _mode = ExtremityMode.Tip;
        }

        public Notegroup(decimal d, Double st, short? orientation, int? status, Repository.DataService.Note n, Repository.DataService.Chord ch)
        {
            Id = Guid.NewGuid();
            Duration = d;
            StartTime = st;
            Orientation = orientation;
            Notes = new List<Repository.DataService.Note>();
            IsSpanned = false;
            Notes.Add(n);
            GroupY = Root.Location_Y;
            if (ch != null)
            {
                GroupX = ch.Location_X;
            }
        }

        public void Reverse()
        {
            // reverse notegroup stem direction property
            Orientation = (Orientation == (short)_Enum.Orientation.Up) ? (short)_Enum.Orientation.Down : (short)_Enum.Orientation.Up;
        }

        public Repository.DataService.Note GetExtremity()
        {
            //a a root is either the top-most n in a up-stemmed ch or...
            //...the bottom-most n in a down-stemmed ch.
            //we need to know the root when the ch is spanned - x, y coords
            //of spans are calculated using the x, y coords of the root. in addition,
            //the root is the n that is flagged if the ch is not spanned, or that
            //has its flag removed if the ch is spanned.

            Repository.DataService.Note root = null;
            var found = false;
            if (Notes.Count > 0)
            {

                var rootY = (_mode == ExtremityMode.Root) ? Infrastructure.Constants.Defaults.PlusInfinity : Infrastructure.Constants.Defaults.MinusInfinity;
                if (Orientation == (short)_Enum.Orientation.Up)
                {
                    foreach (Repository.DataService.Note note in Notes)
                    {
                        //CollaborationManager.IsActionable answers the question: "is the n visible?"
                        //another way to ask the same question: "was this n created by the author or the current col?"
                        if (CollaborationManager.IsActive(note))
                        {
                            if (note.Location_Y < rootY && _mode == ExtremityMode.Root ||
                                note.Location_Y > rootY && _mode == ExtremityMode.Tip)
                            {
                                found = true;
                                rootY = note.Location_Y;
                                root = note;
                            }
                        }
                    }
                    if (!found)
                    {
                        rootY = (_mode == ExtremityMode.Root) ?
                                            Infrastructure.Constants.Defaults.PlusInfinity : Infrastructure.Constants.Defaults.MinusInfinity;
                        foreach (Repository.DataService.Note note in Notes)
                        {
                            if (note.Location_Y < rootY && _mode == ExtremityMode.Root ||
                                note.Location_Y > rootY && _mode == ExtremityMode.Tip)
                            {
                                found = true;
                                rootY = note.Location_Y;
                                root = note;
                            }
                        }
                    }
                }
                else
                {
                    rootY = (_mode == ExtremityMode.Root) ?
                                        Infrastructure.Constants.Defaults.MinusInfinity : Infrastructure.Constants.Defaults.PlusInfinity;
                    foreach (Repository.DataService.Note note in Notes)
                    {
                        if (CollaborationManager.IsActive(note))
                        {
                            if (note.Location_Y > rootY && _mode == ExtremityMode.Root ||
                                note.Location_Y < rootY && _mode == ExtremityMode.Tip)
                            {
                                found = true;
                                rootY = note.Location_Y;
                                root = note;
                            }
                        }
                    }
                    if (!found)
                    {
                        rootY = (_mode == ExtremityMode.Root) ?
                                        Infrastructure.Constants.Defaults.MinusInfinity : Infrastructure.Constants.Defaults.PlusInfinity;
                        foreach (var note in Notes)
                        {
                            if (note.Location_Y > rootY && _mode == ExtremityMode.Root ||
                                note.Location_Y < rootY && _mode == ExtremityMode.Tip)
                            {
                                found = true;
                                rootY = note.Location_Y;
                                root = note;
                            }
                        }
                    }
                }
                if (root != null)
                {
                    if (_mode == ExtremityMode.Root)
                    {
                        root.IsSpanned = IsSpanned;
                    }
                }
            }
            return root;
        }
    }
}
