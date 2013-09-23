using System;
using System.Collections.Generic;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{
    //chords contain 1 or more notegroups. a a is a group of notes in a chord with the
    //same stem direction and the same duration.
    public class Notegroup
    {
        public enum ExtremityMode
        {
            Tip,
            Root
        }

        private ExtremityMode _extremityMode;

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
                    if (Root != null)
                    {
                        Root.IsSpanned = true;
                    }
                }
            }
        }

        private Repository.DataService.Note _root;
        public Repository.DataService.Note Root
        {
            get
            {
                _extremityMode = ExtremityMode.Root;
                _root = GetExtremity();
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

        public Notegroup(decimal duration, Double starttime, short orientation)
        {
            Id = Guid.NewGuid();
            Duration = duration;
            StartTime = starttime;
            Orientation = orientation;
            Notes = new List<Repository.DataService.Note>();
            Root = null;
            IsSpanned = false;
            _extremityMode = ExtremityMode.Tip;
        }

        public Notegroup(decimal duration, Double starttime, short? orientation, int? status, Repository.DataService.Note note, Repository.DataService.Chord chord)
        {
            Id = Guid.NewGuid();
            Duration = duration;
            StartTime = starttime;
            Orientation = orientation;
            Notes = new List<Repository.DataService.Note>();
            IsSpanned = false;
            Notes.Add(note);
            if (Root != null)
            {
                GroupY = Root.Location_Y;
                if (chord != null)
                {
                    GroupX = chord.Location_X;
                }
            }
        }

        public void Reverse()
        {
            //reverse a stem direction
            Orientation = (Orientation == (short)_Enum.Orientation.Up) ?
                (short)_Enum.Orientation.Down : (short)_Enum.Orientation.Up;
        }

        public Repository.DataService.Note GetExtremity()
        {
            //a a root is either the top-most note in a up-stemmed chord or...
            //...the bottom-most note in a down-stemmed chord.
            //we need to know the root when the chord is spanned - x, y coords
            //of spans are calculated using the x, y coords of the root. in addition,
            //the root is the note that is flagged if the chord is not spanned, or that
            //has its flag removed if the chord is spanned.

            Repository.DataService.Note root = null;
            var found = false;
            if (Notes.Count > 0)
            {
                var rootY = (_extremityMode == ExtremityMode.Root) ? Infrastructure.Constants.Defaults.PlusInfinity : Infrastructure.Constants.Defaults.MinusInfinity;
                if (Orientation == (short)_Enum.Orientation.Up)
                {
                    foreach (Repository.DataService.Note note in Notes)
                    {
                        //CollaborationManager.IsActionable answers the question: "is the note visible?"
                        //another way to ask the same question: "was this note created by the author or the current collaborator?"
                        if (CollaborationManager.IsActive(note))
                        {
                            if (note.Location_Y < rootY && _extremityMode == ExtremityMode.Root ||
                                note.Location_Y > rootY && _extremityMode == ExtremityMode.Tip)
                            {
                                found = true;
                                rootY = note.Location_Y;
                                root = note;
                            }
                        }
                    }
                    if (!found)
                    {
                        rootY = (_extremityMode == ExtremityMode.Root) ?
                                            Infrastructure.Constants.Defaults.PlusInfinity : Infrastructure.Constants.Defaults.MinusInfinity;
                        foreach (Repository.DataService.Note note in Notes)
                        {
                            if (note.Location_Y < rootY && _extremityMode == ExtremityMode.Root ||
                                note.Location_Y > rootY && _extremityMode == ExtremityMode.Tip)
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
                    rootY = (_extremityMode == ExtremityMode.Root) ?
                                        Infrastructure.Constants.Defaults.MinusInfinity : Infrastructure.Constants.Defaults.PlusInfinity;
                    foreach (Repository.DataService.Note note in Notes)
                    {
                        if (CollaborationManager.IsActive(note))
                        {
                            if (note.Location_Y > rootY && _extremityMode == ExtremityMode.Root ||
                                note.Location_Y < rootY && _extremityMode == ExtremityMode.Tip)
                            {
                                found = true;
                                rootY = note.Location_Y;
                                root = note;
                            }
                        }
                    }
                    if (!found)
                    {
                        rootY = (_extremityMode == ExtremityMode.Root) ?
                                        Infrastructure.Constants.Defaults.MinusInfinity : Infrastructure.Constants.Defaults.PlusInfinity;
                        foreach (var note in Notes)
                        {
                            if (note.Location_Y > rootY && _extremityMode == ExtremityMode.Root ||
                                note.Location_Y < rootY && _extremityMode == ExtremityMode.Tip)
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
                    if (_extremityMode == ExtremityMode.Root)
                    {
                        root.IsSpanned = IsSpanned;
                    }
                }
            }
            return root;
        }
    }
}
