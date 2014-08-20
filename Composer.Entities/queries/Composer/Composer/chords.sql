select [index], m.Sequence, c.StartTime, c.Location_X, c.Duration from measures m, chords c where c.Measure_Id = m.Id order by [index], starttime;

select c.* from measures m, chords c where c.Measure_Id = m.Id order by starttime;

select * from notes order by starttime;

select * from chords order by audit_createdate desc;