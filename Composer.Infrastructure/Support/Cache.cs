using System.Linq;
using System.Data.Services.Client;
using Composer.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

public static class Cache
{
    public static void Clear()
    {
        Chords = new List<Composer.Repository.DataService.Chord>();
        Measures = new List<Composer.Repository.DataService.Measure>();
        Notes = new List<Composer.Repository.DataService.Note>();
        Staffs = new List<Composer.Repository.DataService.Staff>();
        Staffgroups = new List<Composer.Repository.DataService.Staffgroup>();
		Verses = new DataServiceCollection<Composer.Repository.DataService.Verse>();

        Ledgers = new List<Ledger>();
    }

    private static IEventAggregator ea;

    public static void Initialize()
    {
        ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
		verses = new DataServiceCollection<Composer.Repository.DataService.Verse>();
    }

    public static List<Ledger> ledgers = null;
    public static List<Ledger> Ledgers
    {
        get
        {
            if (ledgers == null)
            {
                ledgers = new List<Ledger>();
            }
            return ledgers;
        }
        set { ledgers = value; }
    }

    public static List<Composer.Repository.DataService.Staffgroup> Staffgroups { get; set; }

    public static List<Composer.Repository.DataService.Staff> staffs;
    public static List<Composer.Repository.DataService.Staff> Staffs
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

    private static List<Composer.Repository.DataService.Measure> measures;
    public static List<Composer.Repository.DataService.Measure> Measures
    {
        get { return measures; }
        set
        {
            measures = value;
        }
    }

	public static void AddNote(Composer.Repository.DataService.Note nT)
	{
		var a = (from b in Notes where b.Id == nT.Id select b);
		if (!a.Any())
		{
			Notes.Add(nT);
		}
		else
		{
			
		}
	}

	private static List<Composer.Repository.DataService.Note> notes;
	public static List<Composer.Repository.DataService.Note> Notes
	{
		get { return notes; }
		set
		{
			notes = value;
		}
	}

    public static List<Composer.Repository.DataService.Chord> Chords { get; set; }
    public static List<Composer.Repository.DataService.Note> PlaybackNotes { get; set; }

    public static DataServiceCollection<Composer.Repository.DataService.Verse> verses = null;
	public static DataServiceCollection<Composer.Repository.DataService.Verse> Verses
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
        var a = (from b in Measures where b.Id == measure.Id select b);
        if (!a.Any())
        {
            Measures.Add(measure);
        }
    }

    public static void AddStaff(Composer.Repository.DataService.Staff staff)
    {
        var a = (from b in Staffs where b.Id == staff.Id select b);
        if (!a.Any())
        {
            Staffs.Add(staff);
        }
    }

    public static void AddStaffgroup(Composer.Repository.DataService.Staffgroup staffgroup)
    {
        var a = (from b in Staffgroups where b.Id == staffgroup.Id select b);
        if (!a.Any())
        {
            Staffgroups.Add(staffgroup);
        }
    }
}