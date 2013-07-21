select * from compositions;
select * from staffgroups;
select * from staffs;
select * from Collaborations order by composition_id;
select * from measures;
select * from verses;
select * from notes;
select * from chords;
select * from arcs;

select audit_author_Id, starttime, status from notes  order by starttime
select audit_author_Id, starttime, status from chords order by starttime

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