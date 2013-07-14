using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Services.Client;
using Composer.Infrastructure;
using Composer.Repository;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

public static class Cache
{
    public static void Clear()
    {
        Chords = new ObservableCollection<Composer.Repository.DataService.Chord>();
        Measures = new ObservableCollection<Composer.Repository.DataService.Measure>();
        Notes = new ObservableCollection<Composer.Repository.DataService.Note>();
        Staffs = new ObservableCollection<Composer.Repository.DataService.Staff>();
        Staffgroups = new ObservableCollection<Composer.Repository.DataService.Staffgroup>();
        Verses = new ObservableCollection<Composer.Repository.DataService.Verse>();
        Spans = new ObservableCollection<LocalSpan>();
        Ledgers = new ObservableCollection<Ledger>();
    }

    private static IEventAggregator ea;

    public static void Initialize()
    {
        ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        verses = new ObservableCollection<Composer.Repository.DataService.Verse>();
    }

    public static ObservableCollection<LocalSpan> Spans { get; set; }
    public static ObservableCollection<Ledger> ledgers = null;
    public static ObservableCollection<Ledger> Ledgers
    {
        get
        {
            if (ledgers == null)
            {
                ledgers = new ObservableCollection<Ledger>();
            }
            return ledgers;
        }
        set { ledgers = value; }
    }

    public static ObservableCollection<Composer.Repository.DataService.Staffgroup> Staffgroups { get; set; }

    public static ObservableCollection<Composer.Repository.DataService.Staff> staffs;
    public static ObservableCollection<Composer.Repository.DataService.Staff> Staffs
    {
        get
        {
            return staffs;
        }
        set
        {
            staffs = value;
        }
    }

    private static ObservableCollection<Composer.Repository.DataService.Measure> measures;
    public static ObservableCollection<Composer.Repository.DataService.Measure> Measures
    {
        get { return measures; }
        set
        {
            measures = value;
        }
    }

    public static ObservableCollection<Composer.Repository.DataService.Chord> Chords { get; set; }
    public static ObservableCollection<Composer.Repository.DataService.Note> Notes { get; set; }
    public static ObservableCollection<Composer.Repository.DataService.Note> PlaybackNotes { get; set; }

    public static ObservableCollection<Composer.Repository.DataService.Verse> verses = null;
    public static ObservableCollection<Composer.Repository.DataService.Verse> Verses
    {
        get { return verses; }
        set
        {
            verses = value;
            if (verses != null)
            {
                if (verses.Count() > 0)
                {
                    ea.GetEvent<UpdateLyricsPanel>().Publish(verses);
                }
            }
        }
    }

    public static void AddMeasure(Composer.Repository.DataService.Measure measure)
    {
        var a = (from b in Cache.Measures where b.Id == measure.Id select b);
        if (!a.Any())
        {
            Measures.Add(measure);
        }
    }

    public static void AddStaff(Composer.Repository.DataService.Staff staff)
    {
        var a = (from b in Cache.Staffs where b.Id == staff.Id select b);
        if (!a.Any())
        {
            Staffs.Add(staff);
        }
    }

    public static void AddStaffgroup(Composer.Repository.DataService.Staffgroup staffgroup)
    {
        var a = (from b in Cache.Staffgroups where b.Id == staffgroup.Id select b);
        if (!a.Any())
        {
            Staffgroups.Add(staffgroup);
        }
    }
}