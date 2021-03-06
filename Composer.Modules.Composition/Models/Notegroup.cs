﻿using System;
using System.Collections.Generic;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Models
{
	/// <summary>
	/// Chords contain 1 or more notegroups. The notes in a notegroup have the same stem direction and duration.
	/// All the notes in the notegroup comprise the notegroup unit, and can be acted upon as a unit.
	/// </summary>
    public sealed class Notegroup
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

        public Notegroup(decimal dU, Double sT, short? oR, int? sS, Repository.DataService.Note nT, Repository.DataService.Chord cH)
        {
            Id = Guid.NewGuid();
            Duration = dU;
            StartTime = sT;
            Orientation = oR;
            Notes = new List<Repository.DataService.Note>();
            IsSpanned = false;
            Notes.Add(nT);
            GroupY = Root.Location_Y;
            if (cH != null)
            {
                GroupX = cH.Location_X;
            }
        }

		/// <summary>
		/// reverse the stem direction of a notegroup unit
		/// </summary>
        public void Reverse()
        {
            Orientation = (Orientation == (short)_Enum.Orientation.Up) ? (short)_Enum.Orientation.Down : (short)_Enum.Orientation.Up;
        }

		/// <summary>
		/// The root note is the top-most note in a notegroup if the stem direction is up,
		/// or the bottom-most chord in a notegroup if the stem direction is down. we need to
		/// know the root when we span/despan the notegroup. the spans are drawn using the x,y location 
		/// of the root.
		/// </summary>
		/// <returns></returns>
        private Repository.DataService.Note GetExtremity()
        {
            Repository.DataService.Note root = null;
            var found = false;
            if (Notes.Count > 0)
            {
                var rootY = (_mode == ExtremityMode.Root) ? Infrastructure.Constants.Defaults.PlusInfinity : Infrastructure.Constants.Defaults.MinusInfinity;
                if (Orientation == (short)_Enum.Orientation.Up)
                {
                    foreach (Repository.DataService.Note note in Notes)
                    {
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
