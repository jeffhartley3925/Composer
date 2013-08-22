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

select * from compositions;
select * from staffgroups;
select * from staffs;
select * from Collaborations order by composition_id;
select * from measures;
select * from verses;
select * from notes order by location_X;
select * from chords;
select * from arcs;