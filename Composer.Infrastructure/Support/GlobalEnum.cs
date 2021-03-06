﻿namespace Composer.Infrastructure
{
    public static class _Enum
    {
        public enum ArcSweepDirection { Clockwise, Counterclockwise };
        public enum Disposition { Accept, Reject, Na, None };
        public enum Accidental { Sharp, Flat, Natural, None };
        public enum Orientation { Up, Down, Rest, Na };
        public enum Direction { Up, Down, Left, Right, None };

		public enum Scope
		{
			Sequence,
			Measuregroup,
			Measure,
			All,
			None
		}

        public enum ReplaceMode
        {
            Note,
            Rest,
            None
        }

        public enum PlaybackInitiatedFrom
        {
            Palette,
            Hub,
            Measure,
            Unknown
        }

        public enum HyperlinkButton
        {
            Print,
            Lyrics,
            Save,
            Provenance,
            Collaboration,
            Transpose,
            AddStaff,
            All
        }

        public enum SocialChannel
        {
            All,
            Requests,
            Pinterest,
            Twitter,
            FacebookAll,
            FacebookLike,
            FacebookSend,
            FacebookFeed,
            GooglePlusone
        }

        public enum TranspositionMode
        {
            Octave,
            Key,
            Interval,
            None
        }

        public enum FacebookDependency
        {
            FriendNames,
            FriendIds,
            FriendPictureUrls,
            Username,
            UserId,
            UserPictureUrl
        }

        public enum PasteCommandSource
        {
            User,
            Programmatic,
            Na
        }

        public enum StaffConfiguration
        {
            Simple,
            Grand,
            MultiInstrument,
            None
        }

		public enum NotePlacementMode 
		{ 
            None,
			Append, 
			Insert, 
			PasteAppend, 
			PasteInsert, 
			PasteOverlay
		}

        public enum ArcType { Slur, Tie }

        public enum ClickState { First, Second, Third, None }

        public enum VectorClass
        {
            None,
            Note,
            Rest,
            Dot,
            Accidental,
            Clef,
            Key,
            TimeSignature,
            Ornament
        }

        public enum EntityFilter
        {
            Note = 2,
            Rest = 3,
            Notegroup,
            Chord,
            Measure,
            Staff,
            Staffgroup,
            Composition,
            Accidental,
            Tie,
            Slur,
            Verse,
            Lyrics,
            Provenance
        }
		
        public enum MeasureResizeScope 
        { 
            Staff, 
            Staffgroup,
            Composition,
            Global
        }

        public enum SortOrder
        {
            Ascending,
            Descending
        }

        public enum Filter
        {
            Distinct,
            Indistinct,
            None
        }

        public enum EditContext 
        { 
            Authoring, 
            Contributing 
        }

        public enum Status 
        {
            AuthorOriginal, //0
            AuthorAdded, //1
            AuthorAccepted, //2
            AuthorDeleted, //3
            Null, //4
            PendingAuthorAction, //5
            AuthorRejectedAdd, //6
            AuthorRejectedDelete, //7
            ContributorAdded, //8
            ContributorAccepted, //9
            ContributorDeleted, //10
            ContributorRejectedAdd, //11
            ContributorRejectedDelete, //12
            WaitingOnContributor, //13
            WaitingOnAuthor, //14
            Purged, //15
            AuthorModified, //16
            PendingContributorAction //17
        }
        public enum MeasureFooter { Editing, Collaboration }

        public enum DispositionLocation { BottomHorizontal, BottomVertical, SideHorizontal, SideVertical }
    }
}