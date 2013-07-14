select * from compositions;
select * from staffgroups;
select * from staffs;
select * from Collaborations order by composition_id;
select * from measures;
select * from verses;
select * from notes;
select * from chords;
select * from arcs;
use cdata;
select * from Chords order by audit_createdate desc;
select [Status] STATUS,Pitch P,StartTime ST,Duration DUR,IsSpanned SPANNED,* from Notes order by audit_createdate desc;
update Notes set Location_Y=-22 where Location_Y=22 and StartTime=12
use cdata;
select * from Chords order by audit_createdate asc;
select [Status],Pitch P,StartTime ST,* from Notes order by audit_createdate asc;

update notes set isspanned=1  where starttime=4.5 and status ='0,2' and pitch='G4'

delete notes where audit_author_Id='675485908' and starttime >= 4 and starttime < 8
delete notes  where starttime=4 and status ='0,1' and pitch='D3'
delete chords  where starttime=4
--AuthorOriginal, //0
--AuthorAdded, //1
--AuthorAccepted, //2
--AuthorDeleted, //3
--NotApplicable, //4
--PendingAuthorAction, //5
--AuthorRejectedAdd, //6
--AuthorRejectedDelete, //7
--ContributorAdded, //8
--ContributorAccepted, //9
--ContributorDeleted, //10
--ContributorRejectedAdd, //11
--ContributorRejectedDelete, //12
--WaitingOnContributor, //13
--WaitingOnAuthor, //14
--Purged, //15
--AuthorModified, //16
--PendingContributorAction //17