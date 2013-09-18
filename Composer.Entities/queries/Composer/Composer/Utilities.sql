use [SQL2008R2_848836_cdata];

--DELETE Compositions
--DELETE Collaborations
--DELETE Verses
--DELETE Arcs;
--DELETE StaffGroups
--DELETE Staffs
--DELETE Notes
--DELETE Chords
--DELETE Measures

update measures set width = 560;
update notes set type = 2 where type = 10;

select * from compositions;
select * from staffgroups;
select * from staffs;
select * from Collaborations order by composition_id;
select * from measures;
select * from verses;

select * from notes order by starttime;
select * from notes order by audit_createdate desc;
select pitch, [type], audit_author_id  from notes order by audit_createdate desc;

select * from chords;
select * from chords order by audit_createdate desc;
select audit_author_id  from chords order by audit_createdate desc;

select * from arcs;
select * from notes;